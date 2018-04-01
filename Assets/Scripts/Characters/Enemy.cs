using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;





using System.Diagnostics;

public enum EnemyStatus { Patrolling, Searching, Attacking, Hunting }

/// <summary>
/// NPC combat participants
/// </summary>
public class Enemy : CombatChar
{
    #region Stats fields and properties
    protected int health;
    [SerializeField] int maxHealth;
    protected int mutationPoints;
    [SerializeField] int maxMutationPoints;
    protected int speed;
    [SerializeField] int maxSpeed;
    [SerializeField] int attack;
    [SerializeField] int magicAttack;
    [SerializeField] int defense;
    [SerializeField] int resistance;
    [SerializeField] int initiative;
    [SerializeField] int attackRange;

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
    /// Character's current MP
    /// </summary>
    public override int MutationPoints
    {
        get { return mutationPoints; }
    }
    /// <summary>
    /// Character's maximum MP
    /// </summary>
    public override int MaxMutationPoints
    {
        get { return MaxMutationPoints; }
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
    /// Gets character's attack. Used for physical damage
    /// </summary>
    public override int Attack
    {
        get { return attack; }
    }
    /// <summary>
    /// Gets character's magic attack. Used for magic damage
    /// </summary>
    public override int MagicAttack
    {
        get { return magicAttack; }
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
    /// Character's initiative. Used to determine turn order
    /// </summary>
    public override int Initiative
    {
        get { return initiative; }
    }
    /// <summary>
    /// Gets character's attack range
    /// </summary>
    public override int AttackRange
    {
        get { return attackRange; }
    }
    #endregion

    #region Fields and properties for game flow
    //target tracking
    protected Dictionary<CombatChar, Vector3> lastKnowPositions;
    protected List<CombatChar> targets;
    protected List<CombatChar> currentlySeenTargets;



    //character UI
    protected List<GameObject> unusedSightConeIndicators;
    protected Dictionary<Vector3, GameObject> sightConeIndicators;
    protected GameObject canvas;
    //search related
    SearchZone currentSearchArea;
    List<List<Vector3>> positionsToSearch;
    [SerializeField] List<GameObject> searchZoneList;
    protected Dictionary<SearchZone, bool> searchZones;
    protected int speedRemaining;
    //patrol related
    [SerializeField] List<Vector3> patrolPositions;
    protected int patrolPositionIndex;
    //general control stuff
    protected bool isMoving;
    protected bool isTurning;
    [SerializeField] EnemyStatus status;
    [SerializeField] int visionRange;
    [SerializeField] int currentFacingAngle;
    [SerializeField] int visionAngle;
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
            if (finishedTurn == false)
            {
                //ResetTurnVariables();
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
    protected void Awake()
    {
        lastKnowPositions = new Dictionary<CombatChar, Vector3>();

        currentlySeenTargets = new List<CombatChar>();

        searchZones = new Dictionary<SearchZone, bool>();
        foreach(GameObject searchZone in searchZoneList)
        {
            searchZones[searchZone.GetComponent<SearchZone>()] = false;
        }
        
        //inherited control variables
        finishedTurn = true;
        isTurning = false;

        //////stats
        ////health = 10;
        ////maxHealth = 10;
        ////speed = 0;
        ////maxSpeed = 0;
        ////defense = 5;
        ////resistance = 5;
        ////attackRange = 5;

        health = maxHealth;
        mutationPoints = maxMutationPoints;
        speed = maxSpeed;

        //status = EnemyStatus.Searching;
        patrolPositionIndex = -1;

        //visionAngle = 90;

        sightConeIndicators = new Dictionary<Vector3, GameObject>();
        unusedSightConeIndicators = new List<GameObject>();

        canvas = GameObject.FindGameObjectWithTag("Canvas");


        for (int i = (int)transform.position.x - visionRange; i <= transform.position.x + visionRange; i++)
        {
            for (int j = (int)transform.position.y - visionRange; j <= transform.position.y + visionRange; j++)
            {
                GameObject indicator = Instantiate(GameController.SightSquarePrefab);
                indicator.SetActive(false);
                indicator.transform.SetParent(canvas.transform);


                unusedSightConeIndicators.Add(indicator);
            }
        }
    }

    /// <summary>
    /// Stores all possible targets in a list (playable characters and their allies)
    /// </summary>
    /// <param name="controller">The scene controller of the current scen</param>
    public void CreateTargetList()
    {
        //stores the positions of the enemy's targets
        targets = CombatSceneController.GoodGuys;

        //when a target moves, check if it left the sight range or entered it
        for(int i = 0; i < targets.Count; i++)
        {
            targets[i].OnMove += TargetMoved;
        }

        //this is the first time before the scene fully starts that the enemy has enough information to calculate its vision cone
        CalculateVisionCone();
    }

    /// <summary>
    /// Starts the coroutine that handles a character's turn
    /// </summary>
    public override void BeginTurn()
    {
        finishedTurn = false;

        speedRemaining = speed;
        CalculateVisionCone();

        UnityEngine.Debug.Log(";alksjdf");

        //if(currentlySeenTargets.Count == 0 && lastKnowPositions.Count > 0) { status = EnemyStatus.Hunting; }

        if (status == EnemyStatus.Patrolling)
        {
            StartCoroutine(Patrol());
        }
        else if(status == EnemyStatus.Searching)
        {
            StartCoroutine(Search());
        }
        else if(status == EnemyStatus.Attacking)
        {
            StartCoroutine(TargetSighted());
        }
        //******************************************************************Possibly some kind of special hunt routine for right after losing sight of a target?
    }

    /// <summary>
    /// Handles the entire turn for this enemy
    /// </summary>
    protected IEnumerator Patrol()
    {
        //the finishedTurn variable tells the turn handler to wait until TakeTurn() completes before starting the next turn
        finishedTurn = false;

        CalculateVisionCone();

        //keep up with the position that the character will move to next and
        //start its next turn in (assuming nothing happens to change the character's state
        patrolPositionIndex++;
        if(patrolPositionIndex >= patrolPositions.Count) { patrolPositionIndex = 0; }

        //gets a path to the next 
        List<Vector3> path = null;
        if (transform.position != patrolPositions[patrolPositionIndex])
        {
            float[,] moveCosts = CombatSceneController.MoveCosts;
            List<CombatChar> goodGuys = CombatSceneController.GoodGuys;
            for (int i = 0; i < goodGuys.Count; i++)
            {
                moveCosts[(int)goodGuys[i].transform.position.x, (int)goodGuys[i].transform.position.y] = 0;
            }

            path = AStarNode.FindPath(transform.position, patrolPositions[patrolPositionIndex], moveCosts);
        }

        //move through every square in the calculated path
        if(path != null)
        {
            foreach(Vector3 position in path)
            {
                StartCoroutine(Move(position));
                //wait until finished moving
                while (isMoving) { yield return null; }
            }
        }


        CalculateVisionCone();

        //this will cause the turn manager to begin the next turn
        finishedTurn = true;
    }
    
    //******************************Possibly intelligently select one search zone that the target wouldn't be in (the enemy was just there/saw the target run the other way/etc.)
    /// <summary>
    /// Searches the area based on editor assigned search areas
    /// </summary>
    protected IEnumerator Search()
    {
        //the finishedTurn variable tells the turn handler to wait until TakeTurn() completes before starting the next turn
        finishedTurn = false;

        CalculateVisionCone();

        //determines the next area to search based on distance
        if (currentSearchArea == null)
        {
            int shortestDistance = int.MaxValue;
            foreach (KeyValuePair<SearchZone, bool> searchZone in searchZones)
            {
                float[,] moveCosts = CombatSceneController.MoveCosts;
                List<CombatChar> goodGuys = CombatSceneController.GoodGuys;
                for (int i = 0; i < goodGuys.Count; i++)
                {
                    moveCosts[(int)goodGuys[i].transform.position.x, (int)goodGuys[i].transform.position.y] = 0;
                }

                int pathDistance = AStarNode.PathDistance(transform.position, searchZone.Key.transform.position, moveCosts);
                if (searchZone.Value == false && pathDistance <= shortestDistance)
                {
                    shortestDistance = pathDistance;
                    currentSearchArea = searchZone.Key;
                }
            }
            positionsToSearch = new List<List<Vector3>>(currentSearchArea.KeyPositionLists);
        }

        //create a list of all reachable positions in the current search zone
        List<Vector3> reachablePositions = new List<Vector3>();
        for (int i = 0; i < positionsToSearch.Count; i++)
        {
            for (int j = 0; j < positionsToSearch[i].Count; j++)
            {
                float[,] moveCosts = CombatSceneController.MoveCosts;
                List<CombatChar> goodGuys = CombatSceneController.GoodGuys;
                for (int k = 0; i < goodGuys.Count; i++)
                {
                    moveCosts[(int)goodGuys[i].transform.position.x, (int)goodGuys[i].transform.position.y] = 0;
                }

                if (AStarNode.CheckSquare(transform.position, positionsToSearch[i][j], moveCosts, speed))
                {
                    reachablePositions.Add(positionsToSearch[i][j]);
                }
            }
        }

        //finds the path with the shortest distance and chooses that one as the path to follow
        if(reachablePositions.Count == 0)
        {
            int index = 0;
            int shortestDistance = int.MaxValue;

            for (int i = 0; i < positionsToSearch.Count; i++)
            {
                for (int j = 0; j < positionsToSearch[i].Count; j++)
                {
                    int pathDistance = AStarNode.PathDistance(transform.position, positionsToSearch[i][j], CombatSceneController.MoveCosts);
                    if (pathDistance < shortestDistance)
                    {
                        shortestDistance = pathDistance;
                        index = i;
                    }
                }
            }
            reachablePositions.AddRange(positionsToSearch[index]);
        }

        //if the enemy can reach one of the positions that needs to be searched it will do so this turn
        if (reachablePositions.Count > 0)
        {
            int index = (new System.Random()).Next(reachablePositions.Count);

            float[,] moveCosts = CombatSceneController.MoveCosts;
            List<CombatChar> goodGuys = CombatSceneController.GoodGuys;
            for (int i = 0; i < goodGuys.Count; i++)
            {
                moveCosts[(int)goodGuys[i].transform.position.x, (int)goodGuys[i].transform.position.y] = 0;
            }

            List<Vector3> path = AStarNode.FindPath(transform.position, reachablePositions[index], moveCosts);

            if (path != null)
            {
                foreach (Vector3 position in path)
                {
                    if (speedRemaining > 0)
                    {
                        StartCoroutine(Move(position));
                        //wait until finished moving
                        while (isMoving) { yield return null; }
                        speedRemaining--;
                    }
                }
            }
        }
             
        //removes the list of positions that has been searched
        //and causes enemy to look around in current position
        for (int i = 0; i < positionsToSearch.Count; i++)
        {
            int numlists = positionsToSearch[i].Count;
            for (int j = 0; j < numlists; j++)
            {
                if (positionsToSearch[i][j] == transform.position)
                {
                    positionsToSearch.Remove(positionsToSearch[i]);
                    j = numlists;

                    StartCoroutine(LookAround(45));
                    while (isTurning) { yield return null; }
                }
            }
        }

        //resets the current search zone to null and checks off this search zone
        if (positionsToSearch.Count == 0)
        {
            searchZones[currentSearchArea] = true;
            currentSearchArea = null;
        }

        //if all search zones have been searched, reset the list
        if (!searchZones.ContainsValue(false))
        {
            List<SearchZone> zones = new List<SearchZone>(searchZones.Keys);
            for(int i = 0; i < zones.Count; i++)
            {
                List<List<Vector3>> positionList = zones[i].KeyPositionLists;
                for(int j = 0; j < positionList.Count; j++)
                {
                    if (positionList[j].Contains(transform.position))
                    {
                        searchZones[zones[i]] = true;
                        j = positionList.Count;
                    }
                    else
                    {
                        searchZones[zones[i]] = false;
                    }
                }
            }
        }

        CalculateVisionCone();

        //this will cause the turn manager to begin the next turn
        finishedTurn = true;
    }


    /// <summary>
    /// Smoothly moves the character from their current position to a position one tile in the direction of input
    /// </summary>
    /// <param name="input">The target position to move to</param>
    protected IEnumerator Move(Vector3 endPos)
    {
        isMoving = true; //while running this routine the AI waits to resume

        //if facing the wrong direction for movement, run the turn method before moving
        Vector3 startPos = transform.position;
        if (endPos.x == startPos.x - 1 && currentFacingAngle != 180)
        {
            StartCoroutine(Turn(180));
        }
        else if (endPos.x == startPos.x + 1 && currentFacingAngle != 0)
        {
            StartCoroutine(Turn(0));
        }
        else if (endPos.y == startPos.y - 1 && currentFacingAngle != 270)
        {
            StartCoroutine(Turn(270));
        }
        else if (endPos.y == startPos.y + 1 && currentFacingAngle != 90)
        {
            StartCoroutine(Turn(90));
        }
        //wait for the turn to finish before moving
        while (isTurning) { yield return null; }

        float t = 0; //time
        float moveSpeed = 5f;

        //smoothly moves the character across the distance with lerp
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        //calculate vision cone for final position
        CalculateVisionCone();

        //when done moving allow more input to be received
        isMoving = false;
    }

    /// <summary>
    /// Turns to a specific angle
    /// </summary>
    /// <param name="targetDirection">The direction to turn to</param>
    protected IEnumerator Turn(int targetDirection)
    {
        isTurning = true;

        //normalize angles and numerically determine which direction to turn
        int startAngle = currentFacingAngle;
        while (startAngle > 360) { startAngle -= 360; }
        while (startAngle < 0) { startAngle += 360; }
        int goalAngle = targetDirection;
        int difference = goalAngle - startAngle;
        if (difference < 0) { difference += 360; }
        if (difference <= 180)
        {
            //left
            int degrees = goalAngle;
            degrees -= startAngle;
            if (degrees < 0) { degrees += 360; }

            goalAngle = startAngle + degrees;
        }
        else
        {
            //right
            int degrees = startAngle;
            degrees -= goalAngle;
            if (degrees < 0) { degrees += 360; }

            goalAngle = startAngle - degrees;
        }

        float t = 0;
        float turnSpeed = 1.5f;

        //smoothly turn using lerp
        while (t < 1f)
        {
            t += Time.deltaTime * turnSpeed;
            currentFacingAngle = (int)Mathf.Lerp(startAngle, goalAngle, t);
            CalculateVisionCone();
            yield return null;
        }
        //ensures that the final facing angles are exact
        currentFacingAngle = targetDirection;

        isTurning = false;
    }

    /// <summary>
    /// Moves and turns simultaneously, creating a "strafing" effect
    /// </summary>
    /// <param name="endPos">The target destination</param>
    /// <param name="targetDirection">The direction to finish facing</param>
    /// <returns></returns>
    protected IEnumerator MoveAndTurn(Vector3 endPos, int targetDirection)
    {
        isMoving = true;

        //normalize angles to [0, 360] and then numerically determine which direction to turn
        int startAngle = currentFacingAngle;
        while (startAngle > 360) { startAngle -= 360; }
        while (startAngle < 0) { startAngle += 360; }
        int goalAngle = targetDirection;
        int difference = goalAngle - startAngle;
        if (difference < 0) { difference += 360; }
        if (difference <= 180)
        {
            //left
            int degrees = goalAngle;
            degrees -= startAngle;
            if (degrees < 0) { degrees += 360; }

            goalAngle = startAngle + degrees;
        }
        else
        {
            //right
            int degrees = startAngle;
            degrees -= goalAngle;
            if (degrees < 0) { degrees += 360; }

            goalAngle = startAngle - degrees;
        }


        Vector3 startPos = transform.position;

        float t = 0; //time
        float moveSpeed = 3.5f;

        //smoothly moves and turns the character across the distance with lerp
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            currentFacingAngle = (int)Mathf.Lerp(startAngle, goalAngle, t);
            CalculateVisionCone(true);
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        //this is to ensure that the final angle is precise and does not suffer from math errors
        currentFacingAngle = targetDirection;

        yield return null;
        CalculateVisionCone();
        isMoving = false;
    }
    
    /// <summary>
    /// Calculates and displays the enemy's range of vision
    /// </summary>
    protected void CalculateVisionCone(bool skipTargetting = false)
    {
        //gets rid of any old sight indicators
        List<GameObject> indicators = sightConeIndicators.Values.ToList();
        for(int i = 0; i < indicators.Count; i++)
        {
            indicators[i].SetActive(false);
            unusedSightConeIndicators.Add(indicators[i]);
        }
        sightConeIndicators.Clear();

        //determines the bounds of the play area
        CombatSceneController controller = GameObject.FindGameObjectWithTag("SceneController").GetComponent<CombatSceneController>();
        Vector3 bottomLeft = controller.BottomLeftCorner;
        Vector3 topRight = controller.TopRightCorner;

        //sets up needed UI elements and calculations
        Vector2 bottom = Camera.main.WorldToScreenPoint(new Vector3(0, -.5f));
        Vector2 top = Camera.main.WorldToScreenPoint(new Vector3(0, .5f));
        Vector2 sightIndicatorDimensions = new Vector2(top.y - bottom.y, top.y - bottom.y);

        //check every square within vision range
        for(int i = (int)transform.position.x - visionRange; i <= transform.position.x + visionRange; i++)
        {
            for(int j = (int)transform.position.y - visionRange; j <= transform.position.y + visionRange; j++)
            {
                int x = i - (int)transform.position.x;
                int y = j - (int)transform.position.y;
                int r = (int)System.Math.Sqrt((x * x) + (y * y));
                if(r <= visionRange && new Vector3(i, j) != transform.position)
                {
                    //calculate all relevant angles
                    int theta = visionAngle / 2;
                    int startAngle = currentFacingAngle - theta;
                    int endAngle = currentFacingAngle + theta;
                    double pointAngle = System.Math.Atan2(y, x) * (180 / System.Math.PI);
                    //normalize angles to [0, 360]
                    while(startAngle > 360) { startAngle -= 360; }
                    while(startAngle < 0) { startAngle += 360; }
                    while(endAngle > 360) { endAngle -= 360; }
                    while(endAngle < 0) { endAngle += 360; }
                    while(pointAngle > 360) { pointAngle -= 360; }
                    while(pointAngle < 0) { pointAngle += 360; }
                    //normalize angles to [0, 360] with startAngle = 0
                    endAngle -= startAngle;
                    pointAngle -= startAngle;
                    startAngle = 0;
                    if(endAngle < 0) { endAngle += 360; }
                    if(pointAngle < 0) { pointAngle += 360; }

                    //if the square can be seen, then instantiate a UI element in the vision cone
                    Vector3 sightSquare = new Vector3(i, j);
                    if (pointAngle >= startAngle && pointAngle <= endAngle && !Physics2D.Linecast(transform.position, sightSquare)
                        && sightSquare.x >= bottomLeft.x && sightSquare.x <= topRight.x && sightSquare.y >= bottomLeft.y && sightSquare.y <= topRight.y)
                    {
                        sightConeIndicators[sightSquare] = unusedSightConeIndicators[unusedSightConeIndicators.Count - 1];
                        unusedSightConeIndicators.RemoveAt(unusedSightConeIndicators.Count - 1);
                        sightConeIndicators[sightSquare].SetActive(true);
                        sightConeIndicators[sightSquare].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(sightSquare);
                        sightConeIndicators[sightSquare].GetComponent<RectTransform>().sizeDelta = sightIndicatorDimensions;
                    }
                }
            }
        }

        //removes null references from the dictionary
        foreach (KeyValuePair<Vector3, GameObject> sightConeIndicator in sightConeIndicators.Where(Item => Item.Value == null).ToList())
        {
            sightConeIndicators.Remove(sightConeIndicator.Key);
        }

        if (!skipTargetting)
        {
            //removes any targets that are no longer within the enemy's vision from the list
            for (int i = 0; i < currentlySeenTargets.Count; i++)
            {
                if (!sightConeIndicators.ContainsKey(currentlySeenTargets[i].transform.position))
                {
                    currentlySeenTargets.Remove(currentlySeenTargets[i]);
                }
            }

            //while calculating vision during an enemy's turn, it should check to see if any new targets have been added to it's range of vision
            if (!finishedTurn)
            {
                //get seen targets
                List<CombatChar> seenTargets = new List<CombatChar>();
                foreach (CombatChar target in targets)
                {
                    if (sightConeIndicators.ContainsKey(target.transform.position))
                    {
                        seenTargets.Add(target);
                    }
                }

                //sets the list of targets to only those that were not already within the enemy's line of sight before this method ran and adds those to the primary list of seen targets
                seenTargets = (from CombatChar target in seenTargets where !currentlySeenTargets.Contains(target) select target).ToList();
                currentlySeenTargets.AddRange(seenTargets);

                //subscribe to see when newly seen targets move
                for(int i = 0; i < seenTargets.Count; i++)
                {
                    seenTargets[i].OnMove += TargetMoved;
                }

                //if the enemy has seen a new target activate it's target sighted algorithm
                if (seenTargets.Count > 0)
                {
                    StopAllCoroutines();
                    status = EnemyStatus.Attacking;
                    StartCoroutine(TargetSighted());
                }
            }
        }
    }


    /// <summary>
    /// Looks around by turning a set amount in both directions before returning to front-facing
    /// </summary>
    /// <param name="degrees">The amount to turn in each direction</param>
    protected IEnumerator LookAround(int degrees)
    {
        isTurning = true;

        int startAngle = currentFacingAngle;
        int goalAngle = currentFacingAngle + degrees;

        //moves the direction the enemy is facing from start to goal
        float t = 0;
        float turnSpeed = 1.5f;
        while(t < 1f)
        {
            t += Time.deltaTime * turnSpeed;
            currentFacingAngle = (int)Mathf.Lerp(startAngle, goalAngle, t);
            CalculateVisionCone();
            yield return null;
        }

        yield return new WaitForSeconds(.5f);

        //turns back the other direction
        t = 0;
        startAngle = currentFacingAngle;
        goalAngle = currentFacingAngle - degrees * 2;
        while (t < 1f)
        {
            t += Time.deltaTime * turnSpeed;
            currentFacingAngle = (int)Mathf.Lerp(startAngle, goalAngle, t);
            CalculateVisionCone();
            yield return null;
        }

        yield return new WaitForSeconds(.5f);

        //returns to the original direction
        t = 0;
        startAngle = currentFacingAngle;
        goalAngle = currentFacingAngle + degrees;
        while (t < 1f)
        {
            t += Time.deltaTime * turnSpeed;
            currentFacingAngle = (int)Mathf.Lerp(startAngle, goalAngle, t);
            CalculateVisionCone();
            yield return null;
        }

        isTurning = false;
    }





    //protected IEnumerator Attacking()
    //{
        //finishedTurn = false;

        ////calculate this enemy's range of movement
        //float[,] moveCosts = CombatSceneController.MoveCosts;
        //List<CombatChar> goodGuys = CombatSceneController.GoodGuys;
        //for (int i = 0; i < goodGuys.Count; i++)
        //{
        //    moveCosts[(int)goodGuys[i].transform.position.x, (int)goodGuys[i].transform.position.y] = 0;
        //}
        //List<Vector3> moveRange = DijkstraNode.MoveRange(transform.position, speed, moveCosts);


        ////if (currentlySeenTargets.Count > 0 && lastKnowPositions.Count == 0)
        ////{
        //    //calculates and turns towards the average of all target positions
        //    List<double> angles = new List<double>();
        //    foreach (CombatChar seenTarget in currentlySeenTargets)
        //    {
        //        int relX = (int)seenTarget.transform.position.x - (int)transform.position.x;

        //        int relY = (int)seenTarget.transform.position.y - (int)transform.position.y;

        //        double degrees = System.Math.Atan2(relY, relX) * 180 / System.Math.PI;
        //        while (degrees > 360) { degrees -= 360; }
        //        while (degrees < 0) { degrees += 360; }
        //        angles.Add(degrees);
        //    }
        //    int targetDirection = (int)(angles.Sum() / angles.Count);
        //    if (currentFacingAngle != targetDirection)
        //    {
        //        StartCoroutine(Turn(targetDirection)); //the actual turn
        //        while (isTurning) { yield return null; }
        //    }

        //    //make a list of all targets that can be attacked without moving
        //    List<CombatChar> targetsInRange = new List<CombatChar>();
        //    for(int i = 0; i < currentlySeenTargets.Count; i++)
        //    {
        //        int distance = System.Math.Abs((int)currentlySeenTargets[i].transform.position.x - (int)transform.position.x) + System.Math.Abs((int)currentlySeenTargets[i].transform.position.y - (int)transform.position.y);
        //        if (distance <= attackRange && !targetsInRange.Contains(currentlySeenTargets[i]))
        //        {
        //            targetsInRange.Add(currentlySeenTargets[i]);
        //        }
        //    }
        //    //add all targets that can be attacked by moving
        //    for(int i = 0; i < moveRange.Count; i++)
        //    {
        //        if(moveRange[i].x == 19)
        //        {
        //            UnityEngine.Debug.Log("a;lkdfj;a");
        //        }

        //        for(int j = 0; j < currentlySeenTargets.Count; j++)
        //        {
        //            int distance = System.Math.Abs((int)currentlySeenTargets[j].transform.position.x - (int)moveRange[i].x) + System.Math.Abs((int)currentlySeenTargets[j].transform.position.y - (int)moveRange[i].y);

        //            if(distance < attackRange && !Physics2D.Linecast(moveRange[i], currentlySeenTargets[j].transform.position) && !targetsInRange.Contains(currentlySeenTargets[j]))
        //            {
        //                targetsInRange.Add(currentlySeenTargets[j]);
        //            }
        //        }
        //    }

        //    //**********************************************************************************************************************analyze targets and pick the best one
        //    CombatChar target = targetsInRange[0];

        //    Vector3 targetPos = new Vector3();
        //    int shortestDistance = int.MaxValue;
        //    foreach (KeyValuePair<Vector3, GameObject> sightSquare
        //        in sightConeIndicators.Where(Item => (System.Math.Abs((int)Item.Key.x - (int)target.transform.position.x) + System.Math.Abs((int)Item.Key.y - (int)target.transform.position.y)) <= attackRange).ToList())
        //    {
        //        if (!Physics2D.Linecast(sightSquare.Key, target.transform.position))
        //        {
        //            int pathDistance = AStarNode.PathDistance(transform.position, sightSquare.Key, moveCosts);
        //            if (pathDistance <= shortestDistance)
        //            {
        //                shortestDistance = pathDistance;
        //                targetPos = sightSquare.Key;
        //            }
        //        }
        //    }

        //    //moves towards the target position with as much movement as the enemy has remaing
        //    List<Vector3> path = AStarNode.FindPath(transform.position, targetPos, moveCosts);
        //    if (path != null)
        //    {
        //        foreach (Vector3 position in path)
        //        {
        //            if (speedRemaining > 0)
        //            {
        //                //calculates and turns towards the avearage of all target positions
        //                angles.Clear();
        //                foreach (CombatChar seenTarget in currentlySeenTargets)
        //                {
        //                    int relX = (int)target.transform.position.x - (int)position.x;

        //                    int relY = (int)target.transform.position.y - (int)position.y;

        //                    double degrees = System.Math.Atan2(relY, relX) * 180 / System.Math.PI;
        //                    while (degrees > 360) { degrees -= 360; }
        //                    while (degrees < 0) { degrees += 360; }
        //                    angles.Add(degrees);
        //                }
        //                targetDirection = (int)(angles.Sum() / angles.Count);
        //                //move and turn simultaneously towards the chosen target - "strafing"
        //                StartCoroutine(MoveAndTurn(position, targetDirection));
        //                while (isMoving) { yield return null; }
        //                speedRemaining--;
        //            }
        //        }
        //    }
        //    yield return null;









        //}
        //else if(currentlySeenTargets.Count == 0 && lastKnowPositions.Count > 0)
        //{

        //}
        //else if(currentlySeenTargets.Count > 0 && lastKnowPositions.Count > 0)
        //{
        //    //if(/*reachable*/)
        //    //else if(/*unreachable*/)
        //}









        //finishedTurn = true;
    //}



    /// <summary>
    /// Runs when an enemy sees a hostile target and handles the ensuing behaviour
    /// </summary>
    protected IEnumerator TargetSighted()
    {
        finishedTurn = false;

        if (currentlySeenTargets.Count > 0 && lastKnowPositions.Count == 0)
        {
            #region turn toward targets
            //calculates and turns towards the average of all target positions
            List<double> angles = new List<double>();
            foreach (CombatChar seenTarget in currentlySeenTargets)
            {
                int relX = (int)seenTarget.transform.position.x - (int)transform.position.x;

                int relY = (int)seenTarget.transform.position.y - (int)transform.position.y;

                double degrees = System.Math.Atan2(relY, relX) * 180 / System.Math.PI;
                while (degrees > 360) { degrees -= 360; }
                while (degrees < 0) { degrees += 360; }
                angles.Add(degrees);
            }
            int targetDirection = (int)(angles.Sum() / angles.Count);
            if (currentFacingAngle != targetDirection)
            {
                StartCoroutine(Turn(targetDirection)); //the actual turn
                while (isTurning) { yield return null; }
            }
            #endregion

            //**************************************************************************************************************************************code timing
            Stopwatch sw = new Stopwatch();
            sw.Start();

            //creates a list of all targets that the enemy can attack from it's current position
            List<CombatChar> reachableTargets = new List<CombatChar>();
            foreach (CombatChar target in currentlySeenTargets)
            {
                int distance = System.Math.Abs((int)target.transform.position.x - (int)transform.position.x) + System.Math.Abs((int)target.transform.position.y - (int)transform.position.y);
                if (distance <= attackRange)
                {
                    reachableTargets.Add(target);
                }

                //if (Node.PathDistance(transform.position, target.transform.position) <= attackRange)
                //{
                //    reachableTargets.Add(target);
                //}
            }

            //if the enemy can attack a target without moving, it does so
            if (reachableTargets.Count > 0)
            {
                //***********************************************************************analyze targets in someway
                CombatChar target = reachableTargets[0];

                //calculates damage to apply and calls TakeDamage()
                //int damage = attack + attack - target.Defense;
                target.BeginTakeDamage(100);
                while (target.TakingDamage) { yield return null; }

                yield return null; //wait for one Update so that the target is registered as null if it is destroyed
                while (currentlySeenTargets.Contains(null))
                {
                    currentlySeenTargets.Remove(null);
                }
            }
            //otherwise it moves towards its intended target
            else
            {
                //**********************************************************************analyze targets in someway
                CombatChar target = currentlySeenTargets[0];

                //finds the shortest path to a square from which the enemy would be able to attack its target
                Vector3 targetPos = new Vector3();
                int shortestDistance = int.MaxValue;


                //**************************Analyzing all squares within movement range takes too long...
                //for (int x = (int)target.transform.position.x - attackRange; x <= (int)target.transform.position.x + attackRange; x++)
                //{
                //    for (int y = (int)target.transform.position.y - (attackRange - System.Math.Abs((int)target.transform.position.x - x)); System.Math.Abs((int)target.transform.position.x - x) + System.Math.Abs((int)target.transform.position.y - y) <= attackRange; y++)
                //    {
                //        Vector3 testPos = new Vector3(x, y);
                //        int pathDistance = Node.PathDistance(transform.position, testPos);
                //        if (pathDistance <= shortestDistance && pathDistance <= speedRemaining)
                //        {
                //            shortestDistance = pathDistance;
                //            targetPos = testPos;
                //        }
                //    }
                //}

                //...so instead we check each square within the sight cone
                foreach (KeyValuePair<Vector3, GameObject> sightSquare
                    in sightConeIndicators.Where(Item => (System.Math.Abs((int)Item.Key.x - (int)target.transform.position.x) + System.Math.Abs((int)Item.Key.y - (int)target.transform.position.y)) <= attackRange).ToList())
                {
                    //************************************************************perhaps make sure that the target can be seen from the end square before calculating distance
                    int pathDistance = Node.PathDistance(transform.position, sightSquare.Key);
                    if (pathDistance <= shortestDistance)
                    {
                        shortestDistance = pathDistance;
                        targetPos = sightSquare.Key;
                    }
                }

                //moves towards the target position with as much movement as the enemy has remaing
                List<Vector3> path = Node.FindPath(transform.position, targetPos);
                if (path != null)
                {
                    foreach (Vector3 position in path)
                    {
                        if (speedRemaining > 0)
                        {
                            //calculates and turns towards the avearage of all target positions
                            angles.Clear();
                            foreach (CombatChar seenTarget in currentlySeenTargets)
                            {
                                int relX = (int)target.transform.position.x - (int)position.x;

                                int relY = (int)target.transform.position.y - (int)position.y;

                                double degrees = System.Math.Atan2(relY, relX) * 180 / System.Math.PI;
                                while (degrees > 360) { degrees -= 360; }
                                while (degrees < 0) { degrees += 360; }
                                angles.Add(degrees);
                            }
                            targetDirection = (int)(angles.Sum() / angles.Count);
                            //move and turn simultaneously towards the chosen target - "strafing"
                            StartCoroutine(MoveAndTurn(position, targetDirection));
                            while (isMoving) { yield return null; }
                            speedRemaining--;
                        }
                    }
                }
                yield return null;
            }
        }

        //********************************************This will need to account for targets running out of sight, but still being known to be around
        if (currentlySeenTargets.Count == 0)
        {
            status = EnemyStatus.Patrolling;
        }
        //********************************************Add anybody still in sightrange to a last known position list

        //tells the scene controller to start the next turn
        finishedTurn = true;
    }




    /// <summary>
    /// When a seen target moves, this determines if they are still seen and if not,
    /// adds it to lastKnownPositions
    /// </summary>
    /// <param name="path">the path the character took</param>
    /// <param name="character">the character that moved</param>
    protected void TargetMoved(List<Vector3> path, CombatChar character)
    {
        //if the target is visible after its move, nothing needs to happen
        if (sightConeIndicators.ContainsKey(character.transform.position)) { return; }
        //if the target did not pass through this enemy's vision range, nothing needs to happen
        bool seen = false;
        for(int i = 0; i < path.Count; i++)
        {
            if (sightConeIndicators.ContainsKey(path[i])) { seen = true; }
        }
        if (!seen) { return; }

        //if the target is not visible and was before its move, it is removed from the list of seen targets
        if (currentlySeenTargets.Contains(character)) { currentlySeenTargets.Remove(character); }

        //sets lastKnownPosition to the first vector3 after the last vector3 that is within sightConeIndicators 
        Vector3 lastKnownPosition = path[path.Count - 1];
        for (int i = path.Count - 2; i >= 0; i--)
        {
            if (sightConeIndicators.ContainsKey(path[i]))
            {
                i = -1;
            }
            else
            {
                lastKnownPosition = path[i];
            }
        }

        lastKnowPositions[character] = lastKnownPosition;
    }





    /// <summary>
    /// Calculates the vision cone based on a specific origin square. Useful for when the enemy is lerping between locations
    /// and its transform.position will not be a nice vector
    /// </summary>
    /// <param name="origin">The square from which to calculate the cone</param>
    //protected void CalculateVisionCone2(Vector3 origin)
    //{
    //    List<GameObject> indicators = sightConeIndicators.Values.ToList();
    //    for (int i = 0; i < indicators.Count; i++)
    //    {
    //        indicators[i].SetActive(false);
    //        unusedSightConeIndicators.Add(indicators[i]);
    //    }
    //    sightConeIndicators.Clear();

    //    //determines the bounds of the play area
    //    CombatSceneController controller = GameObject.FindGameObjectWithTag("SceneController").GetComponent<CombatSceneController>();
    //    Vector3 bottomLeft = controller.BottomLeftCorner;
    //    Vector3 topRight = controller.TopRightCorner;

    //    //sets up needed UI elements and calculations
    //    Vector2 bottom = Camera.main.WorldToScreenPoint(new Vector3(0, -.5f));
    //    Vector2 top = Camera.main.WorldToScreenPoint(new Vector3(0, .5f));
    //    Vector2 sightIndicatorDimensions = new Vector2(top.y - bottom.y, top.y - bottom.y);

    //    for (int i = (int)origin.x - visionRange; i <= origin.x + visionRange; i++)
    //    {
    //        for (int j = (int)origin.y - visionRange; j <= origin.y + visionRange; j++)
    //        {
    //            int x = i - (int)origin.x;
    //            int y = j - (int)origin.y;
    //            int r = (int)System.Math.Sqrt((x * x) + (y * y));
    //            if (r <= visionRange && new Vector3(i, j) != origin)
    //            {
    //                //calculate all relevant angles
    //                int theta = visionAngle / 2;
    //                int startAngle = currentFacingAngle - theta;
    //                int endAngle = currentFacingAngle + theta;
    //                double pointAngle = System.Math.Atan2(y, x) * (180 / System.Math.PI);
    //                //normalize angles to [0, 360]
    //                while (startAngle > 360) { startAngle -= 360; }
    //                while (startAngle < 0) { startAngle += 360; }
    //                while (endAngle > 360) { endAngle -= 360; }
    //                while (endAngle < 0) { endAngle += 360; }
    //                while (pointAngle > 360) { pointAngle -= 360; }
    //                while (pointAngle < 0) { pointAngle += 360; }
    //                //normalize angles to [0, 360] with startAngle = 0
    //                endAngle -= startAngle;
    //                pointAngle -= startAngle;
    //                startAngle = 0;
    //                if (endAngle < 0) { endAngle += 360; }
    //                if (pointAngle < 0) { pointAngle += 360; }


    //                Vector3 sightSquare = new Vector3(i, j);
    //                if (pointAngle >= startAngle && pointAngle <= endAngle && !Physics2D.Linecast(origin, sightSquare)
    //                    && sightSquare.x >= bottomLeft.x && sightSquare.x <= topRight.x && sightSquare.y >= bottomLeft.y && sightSquare.y <= topRight.y)
    //                {
    //                    sightConeIndicators[sightSquare] = unusedSightConeIndicators[unusedSightConeIndicators.Count - 1];
    //                    unusedSightConeIndicators.RemoveAt(unusedSightConeIndicators.Count - 1);
    //                    sightConeIndicators[sightSquare].SetActive(true);

    //                    sightConeIndicators[sightSquare].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(sightSquare);
    //                    sightConeIndicators[sightSquare].GetComponent<RectTransform>().sizeDelta = sightIndicatorDimensions;






    //                    //sightConeIndicators[sightSquare] = Instantiate(GameController.SightSquarePrefab);
    //                    //sightConeIndicators[sightSquare].transform.SetParent(canvas.transform);
    //                    //sightConeIndicators[sightSquare].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(sightSquare);
    //                    //sightConeIndicators[sightSquare].GetComponent<RectTransform>().sizeDelta = sightIndicatorDimensions;
    //                }
    //            }
    //        }
    //    }

    //    //removes null references from the dictionary
    //    foreach (KeyValuePair<Vector3, GameObject> sightConeIndicator in sightConeIndicators.Where(Item => Item.Value == null).ToList())
    //    {
    //        sightConeIndicators.Remove(sightConeIndicator.Key);
    //    }
    //}





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
            //Destroy(canvas);
            //run the death animation here
            List<GameObject> indicators = sightConeIndicators.Values.ToList();
            for (int i = 0; i < indicators.Count; i++)
            {
                indicators[i].SetActive(false);
                unusedSightConeIndicators.Add(indicators[i]);
            }
            sightConeIndicators.Clear();

            Destroy(gameObject);
        }

        //resume the attacking object's turn
        takingDamage = false;

        yield break;
    }
}
