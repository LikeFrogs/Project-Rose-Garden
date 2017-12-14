using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

/// <summary>
/// A playable character
/// </summary>
public abstract class PlayableChar : CombatChar
{
    #region Stats fields and properties
    protected int health;
    protected int maxHealth;
    protected int mutationPoints; //not in base class
    protected int maxMutationPoints; //not in base class
    protected int speed;
    protected int maxSpeed;
    protected int attack;
    protected int magicAttack;
    protected int defense;
    protected int initiative;
    protected int resistance;


    /// <summary>
    /// Character's current health
    /// </summary>
    public abstract override int Health { get; }
    /// <summary>
    /// Character's maximum health
    /// </summary>
    public abstract override int MaxHealth { get; }
    /// <summary>
    /// Character's current MP
    /// </summary>
    public abstract override int MutationPoints { get; }
    /// <summary>
    /// Character's maximum MP
    /// </summary>
    public abstract override int MaxMutationPoints { get; }
    /// <summary>
    /// Character's movement speed
    /// </summary>
    public abstract override int Speed { get; }
    /// <summary>
    /// Character's max speed
    /// </summary>
    public abstract override int MaxSpeed { get; }
    /// <summary>
    /// Character's attack. Used for physical damage
    /// </summary>
    public abstract override int Attack { get; }
    /// <summary>
    /// Character's magic attack. Used for magic attack
    /// </summary>
    public abstract override int MagicAttack { get; }
    /// <summary>
    /// Character's defense. Used to defend against physical attacks
    /// </summary>
    public abstract override int Defense { get; }
    /// <summary>
    /// Character's resistance. Used to defend against magical attacks
    /// </summary>
    public abstract override int Resistance { get; }
    /// <summary>
    /// Character's initiative. Used to determine turn order
    /// </summary>
    public override int Initiative
    {
        get { return initiative; }
    }
    /// <summary>
    /// Gets character's attack range
    /// </summary>
    public abstract override int AttackRange { get; }
    #endregion

    #region Fields and properties for game flow
    List<GameObject> unusedMoveRangeIndicators;
    List<GameObject> unusedRangeIndicators;
    Dictionary<Vector3, GameObject> moveRangeIndicators;
    Dictionary<Vector3, GameObject> attackRangeIndicators;






















    protected bool finishedTurn;
    protected bool takingDamage;

    //these all store either data that tells this class when to perform certain actions or objects that need to be accessed from more than one method
    protected Vector3 startingPosition;
    protected bool movePhase;
    protected bool isMoving;
    protected List<Vector3> moveRange;
    protected bool actionCompleted;
    protected GameObject UICanvas;
    protected Canvas MovementCanvas;
    protected bool waitingForAction;

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


    //// Use this for initialization
    ////all ints are default testing values for the moment
    //protected void Awake()
    //{
    //    //the playable party will always transfer between scenes
    //    DontDestroyOnLoad(transform);

    //    //control variables with properties
    //    finishedTurn = false;
    //    takingDamage = false;

    //    //local control variables
    //    startingPosition = new Vector3();
    //    movePhase = false;
    //    isMoving = false;
    //    moveRange = new List<Vector3>();
    //    actionCompleted = false;
    //    UICanvas = null;
    //    waitingForAction = false;
    //}

    /// <summary>
    /// Sets up the stats of this character. Should only be called at character creation.
    /// </summary>
    public void Init(int maxHealth, int maxSpeed, int attack, int magicAttack, int defense, int resistance)
    {
        this.health = maxHealth;
        this.maxHealth = maxHealth;
        this.speed = maxSpeed;
        this.maxSpeed = maxSpeed;
        this.attack = attack;
        this.magicAttack = magicAttack;
        this.defense = defense;
        this.resistance = resistance;



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




        MovementCanvas = gameObject.GetComponentInChildren<Canvas>();
        moveRangeIndicators = new Dictionary<Vector3, GameObject>();
        attackRangeIndicators = new Dictionary<Vector3, GameObject>();
        unusedMoveRangeIndicators = new List<GameObject>();
        unusedRangeIndicators = new List<GameObject>();




        for (int x = (int)transform.position.x - speed; x <= (int)transform.position.x + speed; x++)
        {
            for (int y = (int)transform.position.y - (speed - System.Math.Abs((int)transform.position.x - x)); System.Math.Abs((int)transform.position.x - x) + System.Math.Abs((int)transform.position.y - y) <= speed; y++)
            {
                GameObject indicator = Instantiate(GameController.MoveRangeSprite);
                indicator.SetActive(false);
                indicator.transform.SetParent(MovementCanvas.transform);


                unusedMoveRangeIndicators.Add(indicator);
            }
        }

        for(int i = 0; i < unusedMoveRangeIndicators.Count; i++)
        {
            GameObject indicator = Instantiate(GameController.AttackSquarePrefab);
            indicator.SetActive(false);
            indicator.transform.SetParent(MovementCanvas.transform);


            unusedRangeIndicators.Add(indicator);
        }
        //Debug.Log(unusedMoveRangeIndicators.Count + "");
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


        #region movement calculations
        //UI object set up
        //////////////////////////////////////////////////////////////////////////////////////////////MovementCanvas = Instantiate(GameController.CanvasPrefab);
        Vector2 bottom = Camera.main.WorldToScreenPoint(new Vector3(0, - .5f));
        Vector2 top = Camera.main.WorldToScreenPoint(new Vector3(0, .5f));
        Vector2 rangeIndicatorDimensions = new Vector2(top.y - bottom.y, top.y - bottom.y);
        //Dictionary<Vector3, GameObject> moveRangeIndicators = new Dictionary<Vector3, GameObject>();
        //Dictionary<Vector3, GameObject> attackRangeIndicators = new Dictionary<Vector3, GameObject>();


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
                    //creates UI for this square
                    //moveRangeIndicators[testMov] = Instantiate(GameController.MoveRangeSprite);
                    moveRangeIndicators[testMov] = unusedMoveRangeIndicators[unusedMoveRangeIndicators.Count - 1];
                    unusedMoveRangeIndicators.RemoveAt(unusedMoveRangeIndicators.Count - 1);
                    moveRangeIndicators[testMov].SetActive(true);

                    moveRangeIndicators[testMov].transform.SetParent(MovementCanvas.transform);
                    moveRangeIndicators[testMov].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(testMov);
                    moveRangeIndicators[testMov].GetComponent<RectTransform>().sizeDelta = rangeIndicatorDimensions;
                }
                else if (Node.CheckSquare(transform.position, testMov, speed))
                {
                    moveRange.Add(testMov);
                    //creates UI for this square
                    //moveRangeIndicators[testMov] = Instantiate(GameController.MoveRangeSprite);
                    GameObject indicator = unusedMoveRangeIndicators[unusedMoveRangeIndicators.Count - 1];
                    unusedMoveRangeIndicators.Remove(indicator);
                    indicator.SetActive(true);
                    moveRangeIndicators[testMov] = indicator;

                    //moveRangeIndicators[testMov] = unusedMoveRangeIndicators[unusedMoveRangeIndicators.Count - 1];
                    //moveRangeIndicators[testMov].SetActive(true);
                    //unusedMoveRangeIndicators.RemoveAt(unusedMoveRangeIndicators.Count - 1);

                    moveRangeIndicators[testMov].transform.SetParent(MovementCanvas.transform);
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
            for (int i = x - AttackRange; i <= x + AttackRange; i++)
            {
                for (int j = y - (AttackRange - System.Math.Abs(x - i)); System.Math.Abs(x - i) + System.Math.Abs(y - j) <= AttackRange; j++)
                {
                    Vector3 testAtk = new Vector3(i, j);

                    //if the target square can be seen from (x, y) and does not already have an indicator it is added to attackRangeIndicatorLocations
                    if (!Physics2D.Linecast(moveRangeIndicator.Key, testAtk) && !moveRangeIndicators.ContainsKey(testAtk) && !attackRangeIndicators.ContainsKey(testAtk))
                    {
                        if(unusedRangeIndicators.Count == 0)
                        {
                            GameObject newIndicator = Instantiate(GameController.AttackSquarePrefab);
                            newIndicator.SetActive(false);
                            newIndicator.transform.SetParent(MovementCanvas.transform);


                            unusedRangeIndicators.Add(newIndicator);
                        }

                        //creates UI for this square
                        GameObject indicator = unusedRangeIndicators[unusedRangeIndicators.Count - 1];
                        unusedRangeIndicators.Remove(indicator);
                        indicator.SetActive(true);
                        attackRangeIndicators[testAtk] = indicator;

                        //attackRangeIndicators[testAtk] = Instantiate(GameController.AttackSquarePrefab);
                        attackRangeIndicators[testAtk].transform.SetParent(MovementCanvas.transform);
                        attackRangeIndicators[testAtk].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(testAtk);
                        attackRangeIndicators[testAtk].GetComponent<RectTransform>().sizeDelta = rangeIndicatorDimensions;

                    }
                }
            }
        }

        DrawTargets();

        //List<GameObject> enemyList = (from GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy") where System.Math.Abs((int)transform.position.x - enemy.transform.position.x) + System.Math.Abs((int)transform.position.y - enemy.transform.position.y) <= AttackRange select enemy).ToList();
        //foreach (GameObject enemy in enemyList)
        //{
        //    if (!Physics2D.Linecast(new Vector2(transform.position.x, transform.position.y), enemy.transform.position))
        //    {
        //        GameObject newIndicator = Instantiate(GameController.SelectionPrefab);
        //        newIndicator.transform.SetParent(MovementCanvas.transform);
        //        newIndicator.GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(enemy.transform.position);
        //    }
        //}


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
        //Destroy(MovementCanvas);
        List<GameObject> indicators = moveRangeIndicators.Values.ToList();
        for(int i = 0; i < indicators.Count; i++)
        {
            indicators[i].SetActive(false);
            unusedMoveRangeIndicators.Add(indicators[i]);
        }
        moveRangeIndicators.Clear();

        indicators = attackRangeIndicators.Values.ToList();
        for(int i = 0; i < indicators.Count; i++)
        {
            indicators[i].SetActive(false);
            unusedRangeIndicators.Add(indicators[i]);
        }
        attackRangeIndicators.Clear();


        //destroy no longer relevant UI
        foreach (GameObject oldIndicator in GameObject.FindGameObjectsWithTag("SelectionIcon")) //destroys old targettable ui
        {
            Destroy(oldIndicator);
        }



        //reset all variables for next turn
        movePhase = false;
        isMoving = false;
        waitingForAction = false;
        actionCompleted = false;
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

        //highlights all enemies targetable from this space
        DrawTargets();

        //when done moving allow more input to be received
        isMoving = false;
    }

    /// <summary>
    /// highlights all enemies targetable from this space
    /// </summary>
    protected abstract void DrawTargets();

    /// <summary>
    /// Instantiates UI buttons for all possible actions
    /// </summary>
    protected void ActionMenu()
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
    protected abstract List<string> GetActions();

    /// <summary>
    /// Ends the character's turn without performing any other actions
    /// </summary>
    private IEnumerator End()
    {
        actionCompleted = true;
        yield break;
    }


    //not yet implemented
    //prolly should make these abstract and move them to subclasses
    private IEnumerator Ability()
    {
        yield break;
    }
    private IEnumerator Spell()
    {
        yield break;
    }

    /// <summary>
    /// Levels up this character
    /// </summary>
    protected abstract void LevelUp();
}