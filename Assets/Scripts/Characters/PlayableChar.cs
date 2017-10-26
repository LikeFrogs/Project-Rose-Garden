using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public enum PlayerClass { Agent, Assassin, DroneCommander, Grenadier, Pistoleer, Sniper, Tank }

/// <summary>
/// A playable character
/// </summary>
public class PlayableChar : CombatChar
{
    #region Stats fields and properties
    private int health;
    private int maxHealth;
    private int speed;
    private int maxSpeed;
    private int strength;
    private int dexterity;
    private int intelligence;
    private int defense;
    private int resistance;
    private int attackRange;

    private PlayerClass playerClass;
    private List<string> abilityList;
    private List<string> spellList;

    /// <summary>
    /// Gets character's current health
    /// </summary>
    public override int Health
    {
        get { return health; }
    }
    /// <summary>
    /// Gets character's maximum health
    /// </summary>
    public override int MaxHealth
    {
        get { return maxHealth; }
    }
    /// <summary>
    /// Gets character's movement speed
    /// </summary>
    public override int Speed
    {
        get { return speed; }
    }
    /// <summary>
    /// Gets character's max speed
    /// </summary>
    public override int MaxSpeed
    {
        get { return maxSpeed; }
    }
    /// <summary>
    /// Gets character's strength. Used for physical damage
    /// </summary>
    public override int Strength
    {
        get { return strength; }
    }
    /// <summary>
    /// Gets character's dexterity. Used for speed and initiative
    /// </summary>
    public override int Dexterity
    {
        get { return dexterity; }
    }
    /// <summary>
    /// Gets character's intelligence. Used for magic damage
    /// </summary>
    public override int Intelligence
    {
        get { return intelligence; }
    }
    /// <summary>
    /// Gets character's defense. Used to defend against physical attacks
    /// </summary>
    public override int Defense
    {
        get { return defense; }
    }
    /// <summary>
    /// Gets character's resistance. Used to defend against magical attacks
    /// </summary>
    public override int Resistance
    {
        get { return resistance; }
    }
    /// <summary>
    /// Gets character's attack range
    /// </summary>
    public override int AttackRange
    {
        get { return attackRange; }
    }

    /// <summary>
    /// Gets character's class
    /// </summary>
    public PlayerClass Class
    {
        get { return playerClass; }
    }
    /// <summary>
    /// Gets character's abilities
    /// </summary>
    public List<string> AbilityList
    {
        get { return abilityList; }
    }
    /// <summary>
    /// Gets character's spells
    /// </summary>
    public List<string> SpellList
    {
        get { return spellList; }
    }
    #endregion

    #region Fields and properties for game flow
    private bool finishedTurn;
    private bool takingDamage;

    //these all store either data that tells this class when to perform certain actions or objects that need to be accessed from more than one method
    private Vector3 startingPosition;
    private bool movePhase;
    private bool isMoving;
    private List<Vector3> moveRange;
    private bool actionCompleted;
    private GameObject UICanvas;
    private bool waitingForAction;
    Dictionary<Vector3, GameObject> moveRangeIndicators;
    Dictionary<Vector3, GameObject> attackRangeIndicators;


    /// <summary>
    /// This bool will be set to true at the end of a character's turn.
    /// This will be used to tell the turn handler to move on to the next turn.
    /// </summary>
    public override bool FinishedTurn
    {
        get { return finishedTurn; }
        set
        {
            finishedTurn = value;
            if (finishedTurn == true)
            {
                //StopAllCoroutines();
                //this set is only called if a turn has been cancelled, thus the character needs to be returned to where it was when the turn started
                transform.position = startingPosition;
                ResetTurnVariables();
            }
        }
    }
    /// <summary>
    /// Gets true when taking damage and false otherwise
    /// </summary>
    public override bool TakingDamage
    {
        get { return takingDamage; }
    }
    #endregion


    // Use this for initialization
    //all ints are default testing values for the moment
    protected void Awake()
    {
        //the playable party will always transfer between scenes
        DontDestroyOnLoad(transform);

        //control variables with properties
        finishedTurn = false;
        takingDamage = false;

        //local control variables
        startingPosition = new Vector3();
        movePhase = false;
        isMoving = false;
        moveRange = new List<Vector3>();
        actionCompleted = false;
        UICanvas = null;
        waitingForAction = false;
        moveRangeIndicators = new Dictionary<Vector3, GameObject>();
        attackRangeIndicators = new Dictionary<Vector3, GameObject>();
    }

    /// <summary>
    /// Sets up the stats of this character. Should only be called at character creation.
    /// </summary>
    public void Init(int maxHealth, int maxSpeed, int strength, int dexterity, int intelligence, int defense, int resistance, int attackRange)
    {
        this.health = maxHealth;
        this.maxHealth = maxHealth;
        this.speed = maxSpeed;
        this.maxSpeed = maxSpeed;
        this.strength = strength;
        this.dexterity = dexterity;
        this.intelligence = intelligence;
        this.defense = defense;
        this.resistance = resistance;
        this.attackRange = attackRange;

        playerClass = PlayerClass.Agent;
        abilityList = new List<string>();
        spellList = new List<string>();
    }

    // Update is called once per frame
    void Update()
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
                if (Input.GetButtonDown("Submit") && !playerList.Contains(transform.position)) //add additional checks to make sure character is not in a space it can't end in
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
            if (Input.GetButtonDown("Cancel"))
            {
                Destroy(UICanvas);

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
    /// Starts the coroutine that handles a character's turn
    /// </summary>
    public override void BeginTurn()
    {
        StartCoroutine("TakeTurn");
    }

    /// <summary>
    /// Handles the entire turn for playable characters
    /// </summary>
    private IEnumerator TakeTurn()
    {
        //the finishedTurn variable tells the turn handler to wait until TakeTurn() completes before starting the next turn
        finishedTurn = false;
        startingPosition = transform.position;

        //environmental effects

        //if health == 0 {yield break;}
        //finishedTurn = true;
        //put this check anywhere it would be possible for the character to take damage

        #region movement calculations
        UICanvas = Instantiate(GameController.CanvasPrefab);
        Vector2 bottom = Camera.main.WorldToScreenPoint(new Vector3(0, - .5f));
        Vector2 top = Camera.main.WorldToScreenPoint(new Vector3(0, .5f));
        Vector2 rangeIndicatorDimensions = new Vector2(top.y - bottom.y, top.y - bottom.y);


        //moveRange must be recalculated on every turn
        moveRange.Clear();

        //this for loop runs the inner functions on every square that would be within a character's unimpeded movement range
        for (int x = (int)transform.position.x - speed; x <= (int)transform.position.x + speed; x++)
        {
            for (int y = (int)transform.position.y - (speed - System.Math.Abs((int)transform.position.x - x)); System.Math.Abs((int)transform.position.x - x) + System.Math.Abs((int)transform.position.y - y) <= speed; y++)
            {
                //determines if the square can actually be reached and if so adds it to moveRange
                Vector3 testMov = new Vector3(x, y);
                if (testMov == transform.position)
                {
                    moveRange.Add(testMov);
                    moveRangeIndicators[testMov] = Instantiate(GameController.MoveRangeSprite);
                    moveRangeIndicators[testMov].transform.SetParent(UICanvas.transform);
                    moveRangeIndicators[testMov].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(testMov);
                    moveRangeIndicators[testMov].GetComponent<RectTransform>().sizeDelta = rangeIndicatorDimensions;
                }
                else if (Node.CheckSquare(transform.position, testMov, speed))
                {
                    moveRange.Add(testMov);
                    moveRangeIndicators[testMov] = Instantiate(GameController.MoveRangeSprite);
                    moveRangeIndicators[testMov].transform.SetParent(UICanvas.transform);
                    moveRangeIndicators[testMov].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(testMov);
                    moveRangeIndicators[testMov].GetComponent<RectTransform>().sizeDelta = rangeIndicatorDimensions;
                }
            }
        }

        //for every reachable square the entire attack range is checked for line of sight
        //this is necessary due to the possiblity that certain squares are only visible from certain positions
        foreach (KeyValuePair<Vector3, GameObject> moveRangeIndicator in moveRangeIndicators)
        {
            int x = (int)moveRangeIndicator.Key.x;
            int y = (int)moveRangeIndicator.Key.y;
            for (int i = x - attackRange; i <= x + attackRange; i++)
            {
                for (int j = y - (attackRange - System.Math.Abs(x - i)); System.Math.Abs(x - i) + System.Math.Abs(y - j) <= attackRange; j++)
                {
                    Vector3 testAtk = new Vector3(i, j);

                    //if the target square can be seen from (x, y) and does not already have an indicator it is added to attackRangeIndicatorLocations
                    if (!Physics2D.Linecast(moveRangeIndicator.Key, testAtk) && !moveRangeIndicators.ContainsKey(testAtk) && !attackRangeIndicators.ContainsKey(testAtk))
                    {
                        attackRangeIndicators[testAtk] = Instantiate(GameController.AttackSquarePrefab);
                        attackRangeIndicators[testAtk].transform.SetParent(UICanvas.transform);
                        attackRangeIndicators[testAtk].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(testAtk);
                        attackRangeIndicators[testAtk].GetComponent<RectTransform>().sizeDelta = rangeIndicatorDimensions;

                    }
                }
            }
        }

        //turns on player movement
        movePhase = true;
        #endregion

        //pause the turn flow while the user is navigating action menus
        //bringing up the action menu is handled in Update()
        actionCompleted = false;
        while (!actionCompleted) { yield return null; }

        //sets this character back to the state it should be in for its next turn
        ResetTurnVariables();

        //this will cause the turn manager to begin the next turn
        finishedTurn = true;
    }

    /// <summary>
    /// Sets all game flow variables to the state they should be in at the start and end of a turn
    /// </summary>
    private void ResetTurnVariables()
    {
        //destroy any UI this turn created
        Destroy(UICanvas);

        //removes the movement range visual
        foreach (KeyValuePair<Vector3, GameObject> moveRangeIndicator in moveRangeIndicators)
        {
            Destroy(moveRangeIndicator.Value);
        }
        moveRangeIndicators.Clear();
        //removes the attack range visual
        foreach (KeyValuePair<Vector3, GameObject> attackRangeIndicator in attackRangeIndicators)
        {
            Destroy(attackRangeIndicator.Value);
        }
        attackRangeIndicators.Clear();

        //reset all variables for next turn
        movePhase = false;
        isMoving = false;
        waitingForAction = false;
        actionCompleted = false;
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
    /// Starts the coroutine to deal damage to a character
    /// </summary>
    /// <param name="damage">The amount of damage to deal</param>
    public override void BeginTakeDamage(int damage)
    {
        StartCoroutine(TakeDamage(damage));
    }

    /// <summary>
    /// Runs when a character takes damage
    /// </summary>
    /// <param name="damage">The amount of damage to take</param>
    private IEnumerator TakeDamage(int damage)
    {
        //this tells the attacking character to pause while this method happens
        takingDamage = true;

        //takes the damage
        health -= damage;
        if (health < 0) { health = 0; }

        //play the taking damage animation here and make sure it takes the correct amount of time
        //yield return null;

        if (health == 0)
        {
            //run the death animation here

            Destroy(gameObject);
        }

        //resume the attacking object's turn
        takingDamage = false;

        yield break;
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

        //prevents players from moving into unreachable squares
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
        UICanvas = Instantiate(GameController.CanvasPrefab);
        //Canvas canvas = (Canvas)UICanvas.GetComponent("Canvas");

        //create buttons based on possible actions
        List<string> menuList = GetActions();
        List<GameObject> buttonList = new List<GameObject>();
        for (int i = 0; i < menuList.Count; i++)
        {
            //instantiate button, change text to correct menu option and connect button to method of the same name
            GameObject button = Instantiate(GameController.ButtonPrefab);
            button.transform.SetParent(UICanvas.transform);
            button.GetComponentInChildren<Text>().text = menuList[i];
            string word = menuList[i];
            button.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(word));
            //position the button next to this character
            Vector2 buttonPosition = Camera.main.WorldToScreenPoint(new Vector3(transform.position.x + .5f, transform.position.y));
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(buttonPosition.x, buttonPosition.y - (30 * i));


            buttonList.Add(button);
        }

        //select the first button to enable keyboard/gamepad control
        GameObject eventSystem = GameObject.FindGameObjectWithTag("EventSystem");
        eventSystem.GetComponent<EventSystem>().SetSelectedGameObject(buttonList[0]);

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

        //checks to see if there are enemies in adjacent squares and adds "Melee" to the action list if so
        List<Vector3> adjacentSquares = new List<Vector3>
        {
            new Vector3(transform.position.x - 1, transform.position.y),
            new Vector3(transform.position.x + 1, transform.position.y),
            new Vector3(transform.position.x, transform.position.y + 1),
            new Vector3(transform.position.x, transform.position.y - 1)
        };
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (adjacentSquares.Contains(enemy.transform.position))
            {
                actionList.Add("Melee");
                break;
            }
        }

        //checks if this character knows any abiliities and adds "Ability" if so
        if (abilityList.Count > 0)
        {
            actionList.Add("Ability");
        }

        //checks if this character knows any spells and adds "Spell" if so
        if (spellList.Count > 0)
        {
            actionList.Add("Spell");
        }

        //"End" vs "Defend" ??
        actionList.Add("End");

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
        Destroy(UICanvas);
        //creates a list of Vector3's with attackable targets
        List<Vector3> adjacentSquares = new List<Vector3>
        {
            new Vector3(transform.position.x - 1, transform.position.y),
            new Vector3(transform.position.x + 1, transform.position.y),
            new Vector3(transform.position.x, transform.position.y + 1),
            new Vector3(transform.position.x, transform.position.y - 1)
        };
        //creates a list of possible targets and a dictionary to hold the UI target icons based on their position
        List<Vector3> targets = (from gameObject in GameObject.FindGameObjectsWithTag("Enemy") where adjacentSquares.Contains(gameObject.transform.position) select gameObject.transform.position).ToList();
        Dictionary<Vector3, GameObject> targetIcons = new Dictionary<Vector3, GameObject>();
        foreach(Vector3 targetPos in targets)
        {
            targetIcons.Add(targetPos, null);
        }
        //instantiates a canvas to display the action menu on
        UICanvas = Instantiate(GameController.CanvasPrefab);
        //creates UI for targetting
        for (int i = 0; i < targets.Count; i++)
        {
            targetIcons[targets[i]] = Instantiate(GameController.SelectionPrefab);
            targetIcons[targets[i]].transform.SetParent(UICanvas.transform);
            targetIcons[targets[i]].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(targets[i]);
        }
        //changes the icon on the first target "selection icon" to a "selected icon"
        int targetIndex = 0;
        Vector3 selectedPosition = targets[targetIndex];
        Destroy(targetIcons[selectedPosition]);
        targetIcons[selectedPosition] = Instantiate(GameController.SelectedPrefab);
        targetIcons[selectedPosition].transform.SetParent(UICanvas.transform);
        targetIcons[selectedPosition].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(selectedPosition);
        #endregion

        //waits while the user is selecting a target
        while (true)
        {
            yield return null;

            //changes which enemy is selected based on next and previous input and keeps track of the last selected position
            Vector3 lastSelectedPosition = selectedPosition;
            if (Input.GetButtonDown("Next"))
            {
                targetIndex++;
                if (targetIndex > targets.Count - 1) { targetIndex = 0; }
            }
            if (Input.GetButtonDown("Previous"))
            {
                targetIndex--;
                if (targetIndex < 0) { targetIndex = targets.Count - 1; }
            }
            selectedPosition = targets[targetIndex];

            //redraws the UI if the selected enemy changes
            if (lastSelectedPosition != selectedPosition)
            {
                //canvas.worldCamera = null; //camera must be removed and reassigned for new UI to render correctly
                //sets the previously selected square to have selection UI
                Destroy(targetIcons[lastSelectedPosition]);
                targetIcons[lastSelectedPosition] = Instantiate(GameController.SelectionPrefab);
                targetIcons[lastSelectedPosition].transform.SetParent(UICanvas.transform);
                targetIcons[lastSelectedPosition].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(lastSelectedPosition);
                //sets the newly selected square to have selected UI
                Destroy(targetIcons[selectedPosition]);
                targetIcons[selectedPosition] = Instantiate(GameController.SelectedPrefab);
                targetIcons[selectedPosition].transform.SetParent(UICanvas.transform);
                targetIcons[selectedPosition].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(selectedPosition);
            }

            //confirms attack target
            if (Input.GetButtonDown("Submit")) { break; }
            //returns to the previous action menu
            if (Input.GetButtonDown("Cancel"))
            {
                Destroy(UICanvas);
                ActionMenu();
                yield break;
            }
        }

        //removes UI as attack goes through
        Destroy(UICanvas);

        //gets the enemy whose position matches the currently selected square
        CombatChar target = (from gameObject in GameObject.FindGameObjectsWithTag("Enemy") where gameObject.transform.position == selectedPosition select gameObject).ToList()[0].GetComponent<CombatChar>();

        //calculates damage to apply and calls TakeDamage()
        int damage = strength /*+ weapon damage*/ - target.Defense;
        target.BeginTakeDamage(damage);
        while (target.TakingDamage) { yield return null; }



        //allows TakeTurn to finish
        actionCompleted = true;
    }

    //not yet implemented
    private IEnumerator Ability()
    {
        yield break;
    }
    private IEnumerator Spell()
    {
        yield break;
    }

    #region Level up mnthods
    /// <summary>
    /// Levels up this character if it is an Agent
    /// </summary>
    private void AgentLevelUP()
    {
        Debug.Log("This works!");
    }
    #endregion
}