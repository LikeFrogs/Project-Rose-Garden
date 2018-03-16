using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public enum PlayerStatus {MovePhase, Moving, ActionMenu, Finished }

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
    protected PlayerStatus status;

    protected List<Vector3> takenPath;

    //character-personal UI elements
    protected List<GameObject> unusedActionButtons;
    protected Dictionary<Vector2, GameObject> actionButtons;
    protected List<GameObject> unusedTargetIcons;
    protected Dictionary<Vector3, GameObject> targetIcons;
    private List<GameObject> unusedMoveRangeIndicators;
    private List<GameObject> unusedRangeIndicators;
    private Dictionary<Vector3, GameObject> moveRangeIndicators;
    private Dictionary<Vector3, GameObject> attackRangeIndicators;

    protected bool finishedTurn;
    protected bool takingDamage;

    //these all store either data that tells this class when to perform certain actions or objects that need to be accessed from more than one method
    protected Vector3 startingPosition;
    protected List<Vector3> moveRange;
    protected Canvas canvas;

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
        takenPath = new List<Vector3>();
        startingPosition = new Vector3();
        moveRange = new List<Vector3>();

        //UI object pooling set up
        moveRangeIndicators = new Dictionary<Vector3, GameObject>();
        attackRangeIndicators = new Dictionary<Vector3, GameObject>();
        unusedMoveRangeIndicators = new List<GameObject>();
        unusedRangeIndicators = new List<GameObject>();
        actionButtons = new Dictionary<Vector2, GameObject>();
        unusedActionButtons = new List<GameObject>();
        targetIcons = new Dictionary<Vector3, GameObject>();
        unusedTargetIcons = new List<GameObject>();
        //movement range sprites
        for (int x = (int)transform.position.x - speed; x <= (int)transform.position.x + speed; x++)
        {
            for (int y = (int)transform.position.y - (speed - System.Math.Abs((int)transform.position.x - x)); System.Math.Abs((int)transform.position.x - x) + System.Math.Abs((int)transform.position.y - y) <= speed; y++)
            {
                GameObject indicator = Instantiate(GameController.MoveRangeSprite);
                indicator.SetActive(false);

                unusedMoveRangeIndicators.Add(indicator);
                DontDestroyOnLoad(indicator);
            }
        }
        //attack range sprites
        for(int i = 0; i < unusedMoveRangeIndicators.Count; i++)
        {
            GameObject indicator = Instantiate(GameController.AttackSquarePrefab);
            indicator.SetActive(false);

            unusedRangeIndicators.Add(indicator);
            DontDestroyOnLoad(indicator);
        }
        //action buttons
        for (int i = 0; i < 15; i++)
        {
            GameObject button = Instantiate(GameController.ButtonPrefab);
            button.SetActive(false);

            unusedActionButtons.Add(button);
            DontDestroyOnLoad(button);
        }
        //target icons
        for (int i = 0; i < 10; i++)
        {
            GameObject indicator = Instantiate(GameController.SelectionPrefab);
            indicator.SetActive(false);

            unusedTargetIcons.Add(indicator);
            DontDestroyOnLoad(indicator);
        }
    }

    /// <summary>
    /// Links this character to scene's canvas
    /// </summary>
    public void OnSceneLoad()
    {
        canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();

        List<GameObject> indicators = unusedActionButtons;
        for (int i = 0; i < indicators.Count; i++)
        {
            indicators[i].SetActive(false);
            indicators[i].transform.SetParent(canvas.transform);
        }
        indicators = unusedMoveRangeIndicators;
        for (int i = 0; i < indicators.Count; i++)
        {
            indicators[i].SetActive(false);
            indicators[i].transform.SetParent(canvas.transform);
        }
        indicators = unusedRangeIndicators;
        for (int i = 0; i < indicators.Count; i++)
        {
            indicators[i].SetActive(false);
            indicators[i].transform.SetParent(canvas.transform);
        }
        indicators = unusedTargetIcons;
        for (int i = 0; i < indicators.Count; i++)
        {
            indicators[i].SetActive(false);
            indicators[i].transform.SetParent(canvas.transform);
        }

    }

    // Update is called once per frame
    void Update()
    {
        //***********************************************************TEMP
        if (takenPath.Count > 0)
        {
            //Debug.DrawLine(startingPosition, takenPath[0]);

            for (int i = 1; i < takenPath.Count; i++)
            {
                Debug.DrawLine(takenPath[i - 1], takenPath[i]);
            }
        }


        //looks for input to bring up the action menu
        if (status == PlayerStatus.MovePhase)
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
                //movePhase = false;
                status = PlayerStatus.ActionMenu;
                //ActionMenu();
                StartCoroutine("ActionMenu");
            }
        }

        //if it is still the move phase after the action menu checks then check for movment input
        if (status == PlayerStatus.MovePhase)
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

        //keep up with character's movement
        startingPosition = transform.position;
        takenPath.Clear();


        //*********************************************************************************************environmental effects


        CalculateRanges();

        DrawTargets();

        //turns on player movement
        status = PlayerStatus.MovePhase;

        //pause the turn flow while the user is navigating action menus
        while(status != PlayerStatus.Finished) { yield return null; }

        //sets this character back to the state it should be in for its next turn
        ResetTurnVariables();

        //this will cause the turn manager to begin the next turn
        finishedTurn = true;
    }

    /// <summary>
    /// Calculates movement and attack ranges
    /// </summary>
    private void CalculateRanges()
    {
        //calculate move range
        float[,] moveCosts = CombatSceneController.MoveCosts;
        List<Enemy> enemies = CombatSceneController.Enemies;
        for (int i = 0; i < enemies.Count; i++)
        {
            moveCosts[(int)enemies[i].transform.position.x, (int)enemies[i].transform.position.y] = 0;
        }
        moveRange.Clear();
        moveRange.AddRange(DijkstraNode.MoveRange(transform.position, speed, moveCosts));

        //UI object set up
        Vector2 bottom = Camera.main.WorldToScreenPoint(new Vector3(0, -.5f));
        Vector2 top = Camera.main.WorldToScreenPoint(new Vector3(0, .5f));
        Vector2 rangeIndicatorDimensions = new Vector2(top.y - bottom.y, top.y - bottom.y);
        for (int i = 0; i < moveRange.Count; i++)
        {
            moveRangeIndicators[moveRange[i]] = unusedMoveRangeIndicators[unusedMoveRangeIndicators.Count - 1];
            unusedMoveRangeIndicators.RemoveAt(unusedMoveRangeIndicators.Count - 1);
            moveRangeIndicators[moveRange[i]].SetActive(true);

            moveRangeIndicators[moveRange[i]].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(moveRange[i]);
            moveRangeIndicators[moveRange[i]].GetComponent<RectTransform>().sizeDelta = rangeIndicatorDimensions;
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
                        if (unusedRangeIndicators.Count == 0)
                        {
                            GameObject newIndicator = Instantiate(GameController.AttackSquarePrefab);
                            newIndicator.SetActive(false);
                            newIndicator.transform.SetParent(canvas.transform);

                            unusedRangeIndicators.Add(newIndicator);
                        }

                        //creates UI for this square
                        GameObject indicator = unusedRangeIndicators[unusedRangeIndicators.Count - 1];
                        unusedRangeIndicators.Remove(indicator);
                        indicator.SetActive(true);
                        attackRangeIndicators[testAtk] = indicator;

                        attackRangeIndicators[testAtk].transform.SetParent(canvas.transform);
                        attackRangeIndicators[testAtk].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(testAtk);
                        attackRangeIndicators[testAtk].GetComponent<RectTransform>().sizeDelta = rangeIndicatorDimensions;

                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Sets all game flow variables to the state they should be in at the start and end of a turn
    /// </summary>
    private void ResetTurnVariables()
    {
        //disable current UI elements and return them to the object pools
        //move range indicators
        List<GameObject> indicators = moveRangeIndicators.Values.ToList();
        for(int i = 0; i < indicators.Count; i++)
        {
            indicators[i].SetActive(false);
            unusedMoveRangeIndicators.Add(indicators[i]);
        }
        moveRangeIndicators.Clear();
        //attack range indicators
        indicators = attackRangeIndicators.Values.ToList();
        for(int i = 0; i < indicators.Count; i++)
        {
            indicators[i].SetActive(false);
            unusedRangeIndicators.Add(indicators[i]);
        }
        attackRangeIndicators.Clear();
        //action buttons
        ResetActionButtons();
        //target icons
        ResetTargetIcons();

        //notify any subscribers of the path taken to the character's current position
        NotifyOfMove(takenPath);
        takenPath.Clear();
    }

    /// <summary>
    /// Removes action buttons from the screen
    /// </summary>
    protected void ResetActionButtons()
    {
        List<GameObject> buttons = actionButtons.Values.ToList();
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].GetComponent<Button>().onClick.RemoveAllListeners();
            buttons[i].SetActive(false);
            unusedActionButtons.Add(buttons[i]);
        }
        actionButtons.Clear();
    }

    /// <summary>
    /// Removes target icons from the screen
    /// </summary>
    protected void ResetTargetIcons()
    {
        List<GameObject> icons = targetIcons.Values.ToList();
        for (int i = 0; i < icons.Count; i++)
        {
            icons[i].SetActive(false);
            unusedTargetIcons.Add(icons[i]);
        }
        targetIcons.Clear();
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

        //***********************************************************************play the taking damage animation here and make sure it takes the correct amount of time
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
        //while running this routine no new input is accepted
        status = PlayerStatus.Moving;

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

        //keeps track of the character's movement
        if (transform.position == endPos && endPos != startPos)
        {
            //calculate move range
            float[,] moveCosts = CombatSceneController.MoveCosts;
            List<Enemy> enemies = CombatSceneController.Enemies;
            for (int i = 0; i < enemies.Count; i++)
            {
                moveCosts[(int)enemies[i].transform.position.x, (int)enemies[i].transform.position.y] = 0;
            }
            takenPath.AddRange(AStarNode.FindPath(startPos, endPos, moveCosts));
            //if the player's custom path is too long, recalculate it to the shortest path
            if (takenPath.Count > speed)
            {
                takenPath = AStarNode.FindPath(startingPosition, endPos, moveCosts);
            }
        }else if(transform.position == endPos && endPos == startPos) { takenPath.Clear(); }
        //if(endPos == startingPosition)
        //{
        //    takenPath.Clear();
        //}

        //**************************************************************Update this to actual graphical drawing of the path (this is currently in update because Debug)
        //if (takenPath.Count > 0)
        //{
        //    Debug.DrawLine(startingPosition, takenPath[0]);

        //    for(int i = 1; i < takenPath.Count; i++)
        //    {
        //        Debug.DrawLine(takenPath[i - 1], takenPath[i]);
        //    }
        //}

        //when done moving allow more input to be received
        status = PlayerStatus.MovePhase;
    }

    /// <summary>
    /// highlights all enemies targetable from this space
    /// </summary>
    protected abstract void DrawTargets();

    /// <summary>
    /// Instantiates UI buttons for all possible actions
    /// </summary>
    protected IEnumerator ActionMenu()
    {
        status = PlayerStatus.ActionMenu;

        //create buttons based on possible actions
        List<string> menuList = GetActions();
        List<GameObject> buttonList = new List<GameObject>();
        for (int i = 0; i < menuList.Count; i++)
        {
            //set up each button appropriately
            Vector2 buttonPosition = Camera.main.WorldToScreenPoint(new Vector3(transform.position.x + .5f, transform.position.y));
            buttonPosition.y = buttonPosition.y - (30 * i);
            actionButtons[buttonPosition] = unusedActionButtons[unusedActionButtons.Count - 1];
            unusedActionButtons.RemoveAt(unusedActionButtons.Count - 1);
            actionButtons[buttonPosition].SetActive(true);
            actionButtons[buttonPosition].GetComponentInChildren<Text>().text = menuList[i];
            string word = menuList[i];
            actionButtons[buttonPosition].GetComponent<Button>().onClick.AddListener(() => StartCoroutine(word));
            actionButtons[buttonPosition].GetComponent<RectTransform>().anchoredPosition = buttonPosition;
            actionButtons[buttonPosition].GetComponent<RectTransform>().SetAsLastSibling();

            buttonList.Add(actionButtons[buttonPosition]);
        }

        //select the first button to enable keyboard/gamepad control
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttonList[0]);

        while (true)
        {
            yield return null;

            if (Input.GetButtonDown("Submit")) { break; }
            if (Input.GetButtonDown("Cancel"))
            {
                ResetActionButtons();

                status = PlayerStatus.MovePhase;

                break;
            }
        }
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
        status = PlayerStatus.Finished;
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