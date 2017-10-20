using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class PlayableChar : CombatChar
{
    #region Fields
    //control variables
    private bool movePhase;
    private bool isMoving;
    private List<Vector3> moveRange;
    private bool actionCompleted;
    private GameObject UICanvas;
    private bool waitingForAction;

    //action specific control variables
    private bool selectingAttackTarget;
    #endregion
    
    // Use this for initialization
    //all ints are default testing values for the moment
    protected override void Awake ()
    {
        //inherited stats
        health = 10;
        maxHealth = 10;
        speed = 6;
        maxSpeed = 6;
        //inherited control variable
        finishedTurn = false;
        //stats
        strength = 12;
        dexterity = 12;
        intelligence = 12;
        //control variables
        movePhase = false;
        isMoving = false;
        moveRange = new List<Vector3>();
        actionCompleted = false;
        UICanvas = null;
        waitingForAction = false;
        //action specific control variables
        selectingAttackTarget = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
        //looks for input to bring up the action menu
        if (movePhase)
        {
            if (!isMoving)
            {
                //used to prevent the character from ending their turn in another character's space
                List<Vector3> playerList = new List<Vector3>();
                GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
                for (int i = 0; i < playerObjects.Length; i++)
                {
                    playerList.Add(playerObjects[i].transform.position);
                }
                playerList.Remove(transform.position); //the character's own square should not be restricted
                
                //input for action menu
                if(Input.GetButtonDown("Submit") && !playerList.Contains(transform.position)) //add additional checks to make sure character is not in a space it can't end in
                {
                    //character can't move while the menu is up
                    movePhase = false;
                    ActionMenu();
                }
            }
        }

        //allows the user to back out of the action menu and move again
        if (waitingForAction)
        {
            //if (Input.GetKeyDown(KeyCode.Escape))
            if(Input.GetButtonDown("Cancel"))
            {
                GameObject.Destroy(UICanvas);
                UICanvas = null;

                movePhase = true;
                waitingForAction = false;
            }
        }

        //if it is still the move phase after the action menu checks then check for movment input
        if (movePhase)
        {
            if (!isMoving)
            {
                //when the player is not actively moving looks for input in x and y directions and calls the move coroutine
                Vector2 input;

                //gets input for movement
                input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

                //the following code disables diagonal movement
                //if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                //{
                //    input.y = 0;
                //}
                //else
                //{
                //    input.x = 0;
                //}

                //if there was input begin moving
                if (input != Vector2.zero)
                {
                    StartCoroutine(Move(input));
                }
            }
        }
    }

    /// <summary>
    /// Calculates the initiative of the character
    /// </summary>
    /// <returns>The calculated initiative value</returns>
    public override int GetInitiative()
    {
        //do some stuff to get an initiative value

        //testing value
        return 1;
    }

    /// <summary>
    /// Handles the entire turn for playable characters
    /// </summary>
    protected override IEnumerator TakeTurn()
    {
        //the finishedTurn variable tells the turn handler to wait until TakeTurn() completes before starting the next turn
        finishedTurn = false;

        //environmental effects

        //if health == 0 {yield break;}
        //finishedTurn = true;
        //put this check anywhere it would be possible for the character to take damage

        #region movement calculations
        //holds the movement range visual which is created as squares are added to moveRange
        List<GameObject> moveRangeIndicators = new List<GameObject>();
        //this for loop runs the inner functions on every square that would be within a character's unimpeded movement range
        //the calculations in the middle determine if the square can actually be reached and if so adds it to moveRange
        for (int x = (int)transform.position.x - speed; x <= (int)transform.position.x + speed; x++)
        {
            for (int y = (int)transform.position.y - (speed - System.Math.Abs((int)transform.position.x - x)); System.Math.Abs((int)transform.position.x - x) + System.Math.Abs((int)transform.position.y - y) <= speed; y++)
            {
                Vector3 test = new Vector3(x, y);
                if (test == transform.position)
                {
                    moveRange.Add(test);
                    moveRangeIndicators.Add(GameObject.Instantiate<GameObject>(GameController.MoveRangeSprite, test, Quaternion.identity));
                }
                else if (Node.CheckSquare(transform.position, test, speed))
                {
                    moveRange.Add(test);
                    //instantiate a movement range visual square with position test here
                    moveRangeIndicators.Add(GameObject.Instantiate<GameObject>(GameController.MoveRangeSprite, test, Quaternion.identity));
                }
            }
        }
        //turns on player movement
        movePhase = true;
        #endregion

        //pause the turn flow while the user is navigating action menus
        //bringing up the action menu is handled in Update() while
        //the specific actions to be performed are each their own functions
        //or coroutines depending on the complexity
        actionCompleted = false;
        while (!actionCompleted){ yield return null; }

        #region end of turn variable resetting
        //moveRange must be recalculated on every turn
        moveRange.Clear();
        //destroy any UI this turn created
        GameObject.Destroy(UICanvas);
        UICanvas = null;
        //removes the movement range visual
        foreach (GameObject moveRangeIndicator in moveRangeIndicators)
        {
            GameObject.Destroy(moveRangeIndicator);
        }
        //reset all variables for next turn
        movePhase = false;
        isMoving = false;
        waitingForAction = false;
        actionCompleted = false;
        #endregion
        
        //this will cause the turn manager to begin the next turn
        finishedTurn = true;
    }

    /// <summary>
    /// Smoothly moves the player from their current position to a position one tile in the direction of input
    /// </summary>
    /// <param name="input">The target position to move to</param>
    private IEnumerator Move(Vector2 input)
    {
        isMoving = true; //while running this routine no new input is accepted
        Vector3 startPos = transform.position;
        float t = 0; //time
        //this vector equals the player's original position + 1 in the direction they are moving
        Vector3 endPos = new Vector3(startPos.x + System.Math.Sign(input.x), startPos.y + System.Math.Sign(input.y));

        float moveSpeed = 5;
        if (input.x != 0 && input.y != 0)//diagonal movement needs to take longer
        {
            moveSpeed = 3.5f;
        }

        //will eventually prevent player from moving into unreachable squares
        if (!moveRange.Contains(endPos))
        {
            t = 1f;
        }

        //smoothly moves the player across the distance with lerp
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        //when done moving allow more input to be received
        isMoving = false;
    }

    /// <summary>
    /// Instantiates UI buttons for all possible actions
    /// </summary>
    private void ActionMenu()
    {
        //instantiates a canvas to display the action menu on
        GameObject canvasObject = Instantiate(GameController.CanvasPrefab);
        Canvas canvas = (Canvas)canvasObject.GetComponent("Canvas");

        //create buttons based on possible actions
        List<string> menuList = GetActions();
        List<GameObject> buttonList = new List<GameObject>();
        for(int i = 0; i< menuList.Count; i++)
        {
            //instantiate button, change text to correct menu option and connect button to method of the same name
            GameObject button = Instantiate(GameController.ButtonPrefab);
            button.transform.SetParent(canvasObject.transform);
            button.GetComponentInChildren<Text>().text = menuList[i];
            string word = menuList[i];
            button.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(word));
            //position the button next to this character
            Vector3 buttonPosition = Camera.main.WorldToScreenPoint(new Vector3(transform.position.x + .5f, transform.position.y));
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(buttonPosition.x, buttonPosition.y - (30 * i));

            buttonList.Add(button);
        }
        //set the canvas camera
        canvas.worldCamera = Camera.main;
        canvas.planeDistance = 1;

        //select the first button to enable keyboard control
        GameObject eventSystem = GameObject.Find("EventSystem");
        eventSystem.GetComponent<EventSystem>().SetSelectedGameObject(buttonList[0]);

        //now the UI is accessable by all methods and can be destroyed when no longer needed
        UICanvas = canvasObject;

        //turns on the ability to back out of the action menu
        waitingForAction = true;
    }

    /// <summary>
    /// Determines which actions the character can take from it's current position
    /// </summary>
    /// <returns>A list of strings representing all possible actions</returns>
    private List<string> GetActions()
    {
        List<string> actionList = new List<string>();

        //checks to see if there are enemies in adjacent squares 
        //and adds "Melee" to the action list if so
        List<Vector3> adjacentSquares = new List<Vector3>
        {
            new Vector3(transform.position.x - 1, transform.position.y),
            new Vector3(transform.position.x + 1, transform.position.y),
            new Vector3(transform.position.x, transform.position.y + 1),
            new Vector3(transform.position.x, transform.position.y - 1)
        };
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach(GameObject enemy in enemies)
        {
            if (adjacentSquares.Contains(enemy.transform.position))
            {
                actionList.Add("Melee");
                break;
            }
        }
        

        //check length of list of abilities known for "Ability"

        //check length of list of spells known for "Spell"

        //"End" vs "Defend" ??

        actionList.Add("End");
        actionList.Add("f2");
        actionList.Add("f3");

        return actionList;
    }

    /// <summary>
    /// Ends the character's turn without performing any other actions
    /// </summary>
    private IEnumerator End()
    {
        actionCompleted = true;
        yield break;
    }

    /// <summary>
    /// Allows the character to make a melee attack
    /// </summary>
    private IEnumerator Melee()
    {
        waitingForAction = false; //this variable refers only to the main action menu

        #region UI set-up
        //removes the action menu before bringing up the next one
        GameObject.Destroy(UICanvas);
        UICanvas = null;
        //creates a list of Vector3's with attackable targets
        List<Vector3> adjacentSquares = new List<Vector3>
        {
            new Vector3(transform.position.x - 1, transform.position.y),
            new Vector3(transform.position.x + 1, transform.position.y),
            new Vector3(transform.position.x, transform.position.y + 1),
            new Vector3(transform.position.x, transform.position.y - 1)
        };
        List<Vector3> enemyPositions = (from gameObject in GameObject.FindGameObjectsWithTag("Enemy") select gameObject.transform.position).ToList();
        List<Vector3> targets = (from pos in enemyPositions where adjacentSquares.Contains(pos) select pos).ToList(); //this is the final list with target positions
        //instantiates a canvas to display the action menu on
        GameObject canvasObject = Instantiate(GameController.CanvasPrefab);
        Canvas canvas = (Canvas)canvasObject.GetComponent("Canvas");
        List<GameObject> selectionIcons = new List<GameObject>();
        //creates UI for targetting
        for (int i = 0; i < targets.Count; i++)
        {
            selectionIcons.Add(Instantiate(GameController.SelectionPrefab));
            selectionIcons[i].transform.SetParent(canvasObject.transform);
            Vector3 selectionPosition = Camera.main.WorldToScreenPoint(targets[i]);
            selectionIcons[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(selectionPosition.x, selectionPosition.y);
        }
        int selected = 0;
        Destroy(selectionIcons[selected]);
        selectionIcons[selected] = Instantiate(GameController.SelectedPrefab);
        selectionIcons[selected].transform.SetParent(canvasObject.transform);
        Vector3 selectedPosition = Camera.main.WorldToScreenPoint(targets[selected]);
        selectionIcons[selected].GetComponent<RectTransform>().anchoredPosition = new Vector2(selectedPosition.x, selectedPosition.y);
        canvas.worldCamera = Camera.main;
        canvas.planeDistance = 1;
        UICanvas = canvasObject;
        #endregion
        
        //waits while the user is selecting a target
        while (true)
        {
            yield return null;

            int lastSelected = selected;
            //selected = (int)Input.GetAxisRaw("Horizontal");

            //changes which enemy is selected based on next and previous input
            if (Input.GetButtonDown("Next"))
            {
                selected++;
            }
            if (Input.GetButtonDown("Previous"))
            {
                selected--;
            }
            //redraws the UI if the selected enemy changes
            if (lastSelected != selected)
            {
                if (selected < 0) { selected = selectionIcons.Count - 1; }
                if (selected > selectionIcons.Count - 1) { selected = 0; }

                canvas.worldCamera = null; //camera must be removed and reassigned for new UI to render correctly
                //sets the previously selected square to have selectable UI
                Destroy(selectionIcons[lastSelected]);
                selectionIcons[lastSelected] = Instantiate(GameController.SelectionPrefab);
                selectionIcons[lastSelected].transform.SetParent(canvasObject.transform);
                selectedPosition = Camera.main.WorldToScreenPoint(targets[lastSelected]);
                selectionIcons[lastSelected].GetComponent<RectTransform>().anchoredPosition = new Vector2(selectedPosition.x, selectedPosition.y);
                //sets the newly selected square to have selected UI
                Destroy(selectionIcons[selected]);
                selectionIcons[selected] = Instantiate(GameController.SelectedPrefab);
                selectionIcons[selected].transform.SetParent(canvasObject.transform);
                selectedPosition = Camera.main.WorldToScreenPoint(targets[selected]);
                selectionIcons[selected].GetComponent<RectTransform>().anchoredPosition = new Vector2(selectedPosition.x, selectedPosition.y);
                //resets the camera
                canvas.worldCamera = Camera.main;
                canvas.planeDistance = 1;
                UICanvas = canvasObject;
            }

            //confirms attack target
            if (Input.GetButtonDown("Submit")) { break; }
            //returns to the previous action menu
            if (Input.GetButtonDown("Cancel"))
            {
                selectingAttackTarget = false;
                Destroy(UICanvas);
                ActionMenu();
                yield break;
            }
        }

        //removes UI as attack goes through
        Destroy(UICanvas);


        //This is where the actual attack will take place
        Debug.Log("Yay! Attacking!");


        //allows TakeTurn to finish
        actionCompleted = true;
    }

    //these are placeholder methods so that more than one button can be shown in the action menu for testing
    //final action methods that require another menu should be coroutines
    private IEnumerator f2()
    {
        System.Random rng = new System.Random();
        Debug.Log("f2" + rng.Next(0, 500));
        yield break;
    }
    private IEnumerator f3()
    {
        System.Random rng = new System.Random();
        Debug.Log("f3" + rng.Next(0, 500));
        yield break;
    }
}
