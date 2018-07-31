using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum Status { MovePhase, Moving, ActionMenu, WaitingForSubclass, None }

public abstract class PlayerCharacter : CombatChar
{
    #region Stats fields and properties
    protected int level;
    protected int totalExp;

    protected int health;
    protected int maxHealth;
    protected int mutationPoints;
    protected int maxMutationPoints;
    protected int speed;
    protected int maxSpeed;
    protected int attack;
    protected int magicAttack;
    protected int defense;
    protected int initiative;
    protected int resistance;

    /// <summary>
    /// Character's current level
    /// </summary>
    public abstract override int Level { get; }
    /// <summary>
    /// Character's current exp total
    /// </summary>
    public abstract int TotalExp { get; }

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
    //status stuff
    protected Status status;
    protected bool finishedTurn;
    protected bool takingDamage;
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
                //this set is only called if a turn has been cancelled, thus the character needs to be returned to where it was when the turn started
                transform.position = startingPosition;
                End();
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

    //character-personal UI elements
    protected List<GameObject> unusedPathSegments;
    protected List<GameObject> pathSegments;
    protected List<GameObject> unusedActionButtons;
    protected Dictionary<Vector2, GameObject> actionButtons;
    protected List<GameObject> unusedTargetIcons;
    protected Dictionary<Vector3, GameObject> targetIcons;
    private List<GameObject> unusedMoveRangeIndicators;
    private List<GameObject> unusedRangeIndicators;
    private Dictionary<Vector3, GameObject> moveRangeIndicators;
    private Dictionary<Vector3, GameObject> attackRangeIndicators;
    protected Canvas canvas;

    //movement stuff
    protected Vector3 startingPosition;
    protected List<Vector3> moveRange;
    protected List<Vector3> takenPath;

    protected Vector3 moveStart;
    protected Vector3 moveEnd;
    protected float lerpTime;

    protected List<Vector3> playerList;
    #endregion

    /// <summary>
    /// Runs once per frame and handles all of the players actions
    /// </summary>
    protected virtual void Update()
    {
        if (status == Status.MovePhase)
        {
            //input for action menu
            if (Input.GetButtonDown("Submit") && !playerList.Contains(transform.position)) //add additional checks to make sure character is not in a space it can't end in
            {
                //character can't move while the menu is up
                status = Status.ActionMenu;
                ActionMenu();
            }
            else
            {
                //when the player is not actively moving looks for input in x and y directions and calls the move coroutine
                //gets input for movement
                Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

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
                moveEnd = new Vector3(transform.position.x + System.Math.Sign(input.x), transform.position.y + System.Math.Sign(input.y));
                if (input != Vector2.zero && moveRange.Contains(moveEnd))
                {
                    moveStart = transform.position;
                    moveEnd = new Vector3(transform.position.x + System.Math.Sign(input.x), transform.position.y + System.Math.Sign(input.y));
                    lerpTime = 0f;

                    status = Status.Moving;
                }

            }
        }
        else if(status == Status.ActionMenu)
        {
            //allows player to back out of action menu
            if (Input.GetButtonDown("Cancel"))
            {
                ResetActionButtons();

                status = Status.MovePhase;
            }
        }

        //moves the player smoothly to its target position
        if(status == Status.Moving)
        {
            lerpTime += Time.deltaTime * 5;
            transform.position = Vector3.Lerp(moveStart, moveEnd, lerpTime);

            //finsh the move
            if (lerpTime >= 1f)
            {
                //snap to the target square in case of float errors
                transform.position = moveEnd;
                //player can now continue to move
                status = Status.MovePhase;

                //draw possible targets
                DrawTargets();

                //if the player has left its starting position, set up its path
                if (transform.position != startingPosition)
                {
                    //calculate move range
                    float[,] moveCosts = CombatSceneController.MoveCosts;
                    List<Enemy> enemies = CombatSceneController.Enemies;
                    for (int i = 0; i < enemies.Count; i++)
                    {
                        if (enemies[i] != null)
                        { 
                            moveCosts[(int)enemies[i].transform.position.x, (int)enemies[i].transform.position.y] = 0;
                        }
                    }

                    //if the player moves back into a space they have already traversed,
                    //reduce takenPath to only be the path to that square
                    if(takenPath.Contains(moveEnd))
                    {
                        for(int i = takenPath.Count - 1; i >= 0; i--)
                        {
                            if(takenPath[i] == moveEnd)
                            {
                                i = -1;
                            }
                            else
                            {
                                takenPath.RemoveAt(i);
                            }
                        }
                    }
                    //otherwise add the new position to the end of the path
                    else
                    {
                        if (!takenPath.Contains(moveStart)) { takenPath.Add(moveStart); }
                        takenPath.AddRange(AStarNode.FindPath(moveStart, moveEnd, moveCosts));
                        //if the player's custom path is too long, recalculate it to the shortest path
                        if (takenPath.Count > speed + 1)
                        {
                            takenPath.Clear();
                            takenPath.Add(startingPosition);
                            takenPath.AddRange(AStarNode.FindPath(startingPosition, moveEnd, moveCosts));
                        }
                    }

                    DrawPath();
                }
                //if the player has returned to its starting position, remove the path
                else
                {
                    ResetPathSegments();
                    takenPath.Clear();
                    //an empty path should technically still contain the player's starting position
                    takenPath.Add(startingPosition);
                }
            }
        }
    }

    /// <summary>
    /// Draws the currently proposed path that this character will take if its turn is ended
    /// </summary>
    private void DrawPath()
    {
        //clear segments before drawing new ones
        ResetPathSegments();

        //loop through path and draw segment for each part
        for(int i = 1; i < takenPath.Count; i++)
        {
            //determine the location of the new segment
            Vector2 pathSegmentPosition = new Vector2((takenPath[i - 1].x + takenPath[i].x) / 2, (takenPath[i - 1].y + takenPath[i].y) / 2);

            //determine the orientation of the new segment
            bool horizontal = false;
            if(takenPath[i - 1].y - takenPath[i].y == 0)
            {
                horizontal = true;
            }

            //set up the new segment to draw
            pathSegments.Add(unusedPathSegments[unusedPathSegments.Count - 1]);
            unusedPathSegments.RemoveAt(unusedPathSegments.Count - 1);
            pathSegments[pathSegments.Count - 1].SetActive(true);
            pathSegments[pathSegments.Count - 1].GetComponent<PathSegment>().SetImage(horizontal);
            pathSegments[pathSegments.Count - 1].transform.SetAsLastSibling();

            pathSegments[pathSegments.Count - 1].GetComponent<RectTransform>().anchoredPosition = pathSegmentPosition;
            pathSegments[pathSegments.Count - 1].GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
        }

    }

    /// <summary>
    /// Sets up the stats of this character. Should only be called at character creation.
    /// </summary>
    public void Init(int maxHealth, int maxSpeed, int attack, int magicAttack, int defense, int resistance, int level = 1)
    {
        this.level = level;

        status = Status.None;

        this.health = maxHealth;
        this.maxHealth = maxHealth;
        this.speed = maxSpeed;
        this.maxSpeed = maxSpeed;
        this.attack = attack;
        this.magicAttack = magicAttack;
        this.defense = defense;
        this.resistance = resistance;

        moveRange = new List<Vector3>();
        takenPath = new List<Vector3>();
        playerList = new List<Vector3>();

        //the playable party will always transfer between scenes
        DontDestroyOnLoad(transform);

        //UI object pooling set up
        canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
        moveRangeIndicators = new Dictionary<Vector3, GameObject>();
        attackRangeIndicators = new Dictionary<Vector3, GameObject>();
        unusedMoveRangeIndicators = new List<GameObject>();
        unusedRangeIndicators = new List<GameObject>();
        actionButtons = new Dictionary<Vector2, GameObject>();
        unusedActionButtons = new List<GameObject>();
        targetIcons = new Dictionary<Vector3, GameObject>();
        unusedTargetIcons = new List<GameObject>();
        pathSegments = new List<GameObject>();
        unusedPathSegments = new List<GameObject>();
        //movement range sprites
        for (int x = (int)transform.position.x - speed; x <= (int)transform.position.x + speed; x++)
        {
            for (int y = (int)transform.position.y - (speed - System.Math.Abs((int)transform.position.x - x)); System.Math.Abs((int)transform.position.x - x) + System.Math.Abs((int)transform.position.y - y) <= speed; y++)
            {
                GameObject indicator = Instantiate(GameController.MoveRangeSprite);
                indicator.SetActive(false);

                unusedMoveRangeIndicators.Add(indicator);
                DontDestroyOnLoad(indicator);

                indicator.SetActive(false);
                indicator.transform.SetParent(canvas.transform);
            }
        }
        //attack range sprites
        for (int i = 0; i < unusedMoveRangeIndicators.Count; i++)
        {
            GameObject indicator = Instantiate(GameController.AttackSquarePrefab);
            indicator.SetActive(false);

            unusedRangeIndicators.Add(indicator);
            DontDestroyOnLoad(indicator);

            indicator.SetActive(false);
            indicator.transform.SetParent(canvas.transform);
        }
        //action buttons
        for (int i = 0; i < 15; i++)
        {
            GameObject button = Instantiate(GameController.ButtonPrefab);
            button.SetActive(false);

            unusedActionButtons.Add(button);
            DontDestroyOnLoad(button);

            button.SetActive(false);
            button.transform.SetParent(canvas.transform);
        }
        //target icons
        for (int i = 0; i < 10; i++)
        {
            GameObject indicator = Instantiate(GameController.SelectionPrefab);
            indicator.SetActive(false);

            unusedTargetIcons.Add(indicator);
            DontDestroyOnLoad(indicator);

            indicator.SetActive(false);
            indicator.transform.SetParent(canvas.transform);
        }
        //path segments
        for (int i = 0; i < speed; i++)
        {
            GameObject indicator = Instantiate(GameController.PathPrefab);
            indicator.SetActive(false);

            unusedPathSegments.Add(indicator);
            DontDestroyOnLoad(indicator);

            indicator.SetActive(false);
            indicator.transform.SetParent(canvas.transform);
        }
    }

    /// <summary>
    /// Starts a player's turn
    /// </summary>
    public override void BeginTurn()
    {
        finishedTurn = false;

        startingPosition = transform.position;
        takenPath.Clear();

        CalculateRanges();

        DrawTargets();


        //used to prevent the character from ending their turn in another character's space
        List<Vector3> playerList = new List<Vector3>();
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < playerObjects.Length; i++)
        {
            playerList.Add(playerObjects[i].transform.position);
        }
        playerList.Remove(transform.position); //the character's own square should not be restricted
        
        //turns on player movement
        status = Status.MovePhase;
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
            if (enemies[i] != null)
            {
                moveCosts[(int)enemies[i].transform.position.x, (int)enemies[i].transform.position.y] = 0;
            }

        }
        moveRange.Clear();
        moveRange.AddRange(DijkstraNode.MoveRange(transform.position, speed, moveCosts));

        //UI object set up
        for (int i = 0; i < moveRange.Count; i++)
        {
            moveRangeIndicators[moveRange[i]] = unusedMoveRangeIndicators[unusedMoveRangeIndicators.Count - 1];
            unusedMoveRangeIndicators.RemoveAt(unusedMoveRangeIndicators.Count - 1);
            moveRangeIndicators[moveRange[i]].SetActive(true);

            moveRangeIndicators[moveRange[i]].GetComponent<RectTransform>().anchoredPosition = moveRange[i];
            moveRangeIndicators[moveRange[i]].GetComponent<RectTransform>().sizeDelta = new Vector2(1,1);
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
                        attackRangeIndicators[testAtk].GetComponent<RectTransform>().anchoredPosition = testAtk;
                        attackRangeIndicators[testAtk].GetComponent<RectTransform>().sizeDelta = new Vector2(1,1);

                    }
                }
            }
        }
    }

    /// <summary>
    /// Ends a player's turn by setting all game flow variables to the state they should be at the start and end of a turn
    /// </summary>
    protected virtual void End()
    {
        //disable current UI elements and return them to the object pools
        //move range indicators
        List<GameObject> indicators = moveRangeIndicators.Values.ToList();
        for (int i = 0; i < indicators.Count; i++)
        {
            indicators[i].SetActive(false);
            unusedMoveRangeIndicators.Add(indicators[i]);
        }
        moveRangeIndicators.Clear();
        //attack range indicators
        indicators = attackRangeIndicators.Values.ToList();
        for (int i = 0; i < indicators.Count; i++)
        {
            indicators[i].SetActive(false);
            unusedRangeIndicators.Add(indicators[i]);
        }
        attackRangeIndicators.Clear();
        //action buttons
        ResetActionButtons();
        //target icons
        ResetTargetIcons();
        //path segments
        ResetPathSegments();

        //notify any subscribers of the path taken to the character's current position
        NotifyOfMove(takenPath);
        takenPath.Clear();

        status = Status.None;

        //completes the turn
        finishedTurn = true;
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
    /// Removes path segments from the screen
    /// </summary>
    protected void ResetPathSegments()
    {
        for (int i = 0; i < pathSegments.Count; i++)
        {
            pathSegments[i].SetActive(false);
            unusedPathSegments.Add(pathSegments[i]);
        }
        pathSegments.Clear();
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
    /// highlights all enemies targetable from this space
    /// </summary>
    protected abstract void DrawTargets();

    /// <summary>
    /// Brings up a menu with a list of possible actions
    /// </summary>
    protected void ActionMenu()
    {
        status = Status.ActionMenu;

        //create buttons based on possible actions
        List<string> menuList = GetActions();
        List<GameObject> buttonList = new List<GameObject>();
        for (int i = 0; i < menuList.Count; i++)
        {
            //set up each button appropriately
            Vector2 buttonPosition = new Vector3(transform.position.x + .5f, transform.position.y);
            buttonPosition.y = buttonPosition.y - (.7f * i);
            actionButtons[buttonPosition] = unusedActionButtons[unusedActionButtons.Count - 1];
            unusedActionButtons.RemoveAt(unusedActionButtons.Count - 1);
            actionButtons[buttonPosition].SetActive(true);
            actionButtons[buttonPosition].GetComponentInChildren<Text>().text = menuList[i];
            string word = menuList[i];
            actionButtons[buttonPosition].GetComponent<Button>().onClick.AddListener(() => Invoke(word, 0f));
            actionButtons[buttonPosition].GetComponent<RectTransform>().anchoredPosition = buttonPosition;
            actionButtons[buttonPosition].GetComponent<RectTransform>().SetAsLastSibling();

            buttonList.Add(actionButtons[buttonPosition]);
        }

        //select the first button to enable keyboard/gamepad control
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttonList[0]);
    }

    /// <summary>
    /// Determines which actions the character can take from it's current position
    /// </summary>
    /// <returns>A list of strings representing all possible actions</returns>
    protected abstract List<string> GetActions();

    /// <summary>
    /// Levels up this character
    /// </summary>
    protected abstract void LevelUp();
}
