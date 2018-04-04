using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum AlertStatus { Patrolling, Searching, Attacking }
public enum NewEnemyStatus { Moving, Turning, MovingAndTurning, LookingAround, Pathing, Strafing, Deciding, WaitingForSeconds }

public class NewEnemy : CombatChar
{
    #region Stats fields and properties
    protected int health;
    [SerializeField] protected int maxHealth;
    protected int mutationPoints;
    [SerializeField] protected int maxMutationPoints;
    protected int speed;
    [SerializeField] protected int maxSpeed;
    [SerializeField] protected int attack;
    [SerializeField] protected int magicAttack;
    [SerializeField] protected int defense;
    [SerializeField] protected int resistance;
    [SerializeField] protected int initiative;
    [SerializeField] protected int attackRange;

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
    //alertStatus stuff
    [SerializeField] protected AlertStatus alertStatus;
    protected NewEnemyStatus status;
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

    //character UI
    protected List<GameObject> unusedSightConeIndicators;
    protected Dictionary<Vector3, GameObject> sightConeIndicators;
    protected GameObject canvas;

    //movement stuff
    protected int speedRemaining;

    protected List<Vector3> path;
    protected int pathIndex;

    protected Vector3 moveStart;
    protected Vector3 moveEnd;

    protected int angleStart;
    protected int angleEnd;

    protected float lerpTime;
    
    //target tracking
    protected Dictionary<CombatChar, Vector3> lastKnowPositions;
    protected List<CombatChar> targets;
    protected List<CombatChar> currentlySeenTargets;

    //patrol related
    [SerializeField] protected List<Vector3> patrolPositions;
    protected int patrolPositionIndex;

    //search related
    SearchZone currentSearchArea;
    List<List<Vector3>> positionsToSearch;
    [SerializeField] protected List<GameObject> searchZoneList;
    protected Dictionary<SearchZone, bool> searchZones;


    //general control stuff
    protected bool isMoving;
    protected bool isTurning;
    [SerializeField] protected int visionRange;
    [SerializeField] protected int currentFacingAngle;
    [SerializeField] protected int visionAngle;
    #endregion


    /// <summary>
    /// Used for initialization
    /// </summary>
    protected void Awake()
    {
        lastKnowPositions = new Dictionary<CombatChar, Vector3>();

        currentlySeenTargets = new List<CombatChar>();

        searchZones = new Dictionary<SearchZone, bool>();
        foreach (GameObject searchZone in searchZoneList)
        {
            searchZones[searchZone.GetComponent<SearchZone>()] = false;
        }

        //control variables
        finishedTurn = true;
        isTurning = false;

        health = maxHealth;
        mutationPoints = maxMutationPoints;
        speed = maxSpeed;

        path = new List<Vector3>();

        patrolPositionIndex = -1;

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
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].OnMove += TargetMoved;
        }

        //this is the first time before the scene fully starts that the enemy has enough information to calculate its vision cone
        CalculateVisionCone();
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
        for (int i = 0; i < path.Count; i++)
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
    /// Calculates and displays the enemy's range of vision
    /// </summary>
    protected void CalculateVisionCone(bool skipTargetting = false)
    {
        //gets rid of any old sight indicators
        List<GameObject> indicators = sightConeIndicators.Values.ToList();
        for (int i = 0; i < indicators.Count; i++)
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
        for (int i = (int)transform.position.x - visionRange; i <= transform.position.x + visionRange; i++)
        {
            for (int j = (int)transform.position.y - visionRange; j <= transform.position.y + visionRange; j++)
            {
                int x = i - (int)transform.position.x;
                int y = j - (int)transform.position.y;
                int r = (int)System.Math.Sqrt((x * x) + (y * y));
                if (r <= visionRange && new Vector3(i, j) != transform.position)
                {
                    //calculate all relevant angles
                    int theta = visionAngle / 2;
                    int angleStart = currentFacingAngle - theta;
                    int endAngle = currentFacingAngle + theta;
                    double pointAngle = System.Math.Atan2(y, x) * (180 / System.Math.PI);
                    //normalize angles to [0, 360]
                    while (angleStart > 360) { angleStart -= 360; }
                    while (angleStart < 0) { angleStart += 360; }
                    while (endAngle > 360) { endAngle -= 360; }
                    while (endAngle < 0) { endAngle += 360; }
                    while (pointAngle > 360) { pointAngle -= 360; }
                    while (pointAngle < 0) { pointAngle += 360; }
                    //normalize angles to [0, 360] with angleStart = 0
                    endAngle -= angleStart;
                    pointAngle -= angleStart;
                    angleStart = 0;
                    if (endAngle < 0) { endAngle += 360; }
                    if (pointAngle < 0) { pointAngle += 360; }

                    //if the square can be seen, then instantiate a UI element in the vision cone
                    Vector3 sightSquare = new Vector3(i, j);
                    if (pointAngle >= angleStart && pointAngle <= endAngle && !Physics2D.Linecast(transform.position, sightSquare)
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
                for (int i = 0; i < seenTargets.Count; i++)
                {
                    seenTargets[i].OnMove += TargetMoved;
                }

                //if the enemy has seen a new target activate it's target sighted algorithm
                if (seenTargets.Count > 0)
                {
                    StopAllCoroutines();
                    alertStatus = AlertStatus.Attacking;
                    //StartCoroutine(TargetSighted());
                }
            }
        }
    }


    // Update is called once per frame
    void Update ()
    {
        if(status == NewEnemyStatus.Pathing)
        {
            pathIndex++;

            //if out of speed or at the end of the path, finish the turn
            if(pathIndex >= path.Count || speedRemaining == 0)
            {
                FinishTurn();
            }

            //move to the next position
            BeginMoving(path[pathIndex]);
        }

        if (status == NewEnemyStatus.Moving)
        {            
            //smoothly move to the goal position
            lerpTime += Time.deltaTime * 5;
            transform.position = Vector3.Lerp(moveStart, moveEnd, lerpTime);

            if (lerpTime >= 1f)
            {
                transform.position = moveEnd;
                speedRemaining--;

                CalculateVisionCone();

                status = NewEnemyStatus.Deciding;
            }
        }
        else if(status == NewEnemyStatus.Turning)
        {
            //smoothly turn to the target direction
            lerpTime += Time.deltaTime * 1.5f;

            currentFacingAngle = (int)Mathf.Lerp(angleStart, angleEnd, lerpTime);
            CalculateVisionCone();

            if(lerpTime >= 1f)
            {
                currentFacingAngle = angleEnd;
                CalculateVisionCone();

                status = NewEnemyStatus.Deciding;
            }
        }
        else if(status == NewEnemyStatus.MovingAndTurning)
        {
            //smoothly move and turn to the target position and direction
            lerpTime += Time.deltaTime * 3.5f;

            currentFacingAngle = (int)Mathf.Lerp(angleStart, angleEnd, lerpTime);
            CalculateVisionCone(true);
            transform.position = Vector3.Lerp(moveStart, moveEnd, lerpTime);

            if(lerpTime >= 1f)
            {
                transform.position = moveEnd;
                speedRemaining--;
                
                currentFacingAngle = angleEnd;

                CalculateVisionCone();

                status = NewEnemyStatus.Deciding;
            }
        }




    }

    public override void BeginTurn()
    {
        finishedTurn = false;

        speedRemaining = speed;
        CalculateVisionCone();

        if(alertStatus == AlertStatus.Patrolling)
        {
            BeginPatrol();
        }
        else if(alertStatus == AlertStatus.Searching)
        {
            BeginSearch();
        }
        else if(alertStatus == AlertStatus.Attacking)
        {
            BeginAttack();
        }
    }

    protected void FinishTurn()
    {
        if (alertStatus == AlertStatus.Searching)
        {
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

                        LookAround(45);
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
                for (int i = 0; i < zones.Count; i++)
                {
                    List<List<Vector3>> positionList = zones[i].KeyPositionLists;
                    for (int j = 0; j < positionList.Count; j++)
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
        }

    }


    /// <summary>
    /// Sets up the patrol action for this turn
    /// </summary>
    protected void BeginPatrol()
    {
        //keep up with the position that the character will move to next and
        //start its next turn in (assuming nothing happens to change the character's state
        patrolPositionIndex++;
        if (patrolPositionIndex >= patrolPositions.Count) { patrolPositionIndex = 0; }

        //gets a path to the next 
        if (transform.position != patrolPositions[patrolPositionIndex])
        {
            float[,] moveCosts = CombatSceneController.MoveCosts;
            List<CombatChar> goodGuys = CombatSceneController.GoodGuys;
            for (int i = 0; i < goodGuys.Count; i++)
            {
                moveCosts[(int)goodGuys[i].transform.position.x, (int)goodGuys[i].transform.position.y] = 0;
            }
            path = AStarNode.FindPath(transform.position, patrolPositions[patrolPositionIndex], moveCosts);

            //set enemy to follo path
            if (path != null)
            {
                pathIndex = -1;
                status = NewEnemyStatus.Pathing;
            }
        }
    }

    /// <summary>
    /// Sets up the search action for this turn
    /// </summary>
    protected void BeginSearch()
    {
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
        if (reachablePositions.Count == 0)
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

            path = AStarNode.FindPath(transform.position, reachablePositions[index], moveCosts);
            pathIndex = -1;

            if (path != null)
            {
                foreach (Vector3 position in path)
                {
                    if (speedRemaining > 0)
                    {
                        status = NewEnemyStatus.Pathing;
                    }
                }
            }
        }
    }

    protected void BeginAttack()
    {

    }

    protected void BeginMoving(Vector3 goal)
    {
        status = NewEnemyStatus.Moving;

        moveStart = transform.position;
        moveEnd = goal;

        lerpTime = 0f;

        //turn if not facing the correct direction
        if (moveEnd.x == moveStart.x - 1 && currentFacingAngle != 180)
        {
            BeginTurning(180);
            status = NewEnemyStatus.Turning;
        }
        else if (moveEnd.x == moveStart.x + 1 && currentFacingAngle != 0)
        {
            BeginTurning(0);
            status = NewEnemyStatus.Turning;
        }
        else if (moveEnd.y == moveStart.y - 1 && currentFacingAngle != 270)
        {
            BeginTurning(270);
            status = NewEnemyStatus.Turning;
        }
        else if (moveEnd.y == moveStart.y + 1 && currentFacingAngle != 90)
        {
            BeginTurning(90);
            status = NewEnemyStatus.Turning;
        }
    }

    /// <summary>
    /// Sets up a turn
    /// </summary>
    /// <param name="targetDirection">The direction to turn towards</param>
    protected void BeginTurning(int targetDirection)
    {
        status = NewEnemyStatus.Turning;

        //normalize angles and numerically determine which direction to turn
        angleStart = currentFacingAngle;
        while (angleStart > 360) { angleStart -= 360; }
        while (angleStart < 0) { angleStart += 360; }
        angleEnd = targetDirection;
        int difference = angleEnd - angleStart;
        if (difference < 0) { difference += 360; }
        if (difference <= 180)
        {
            //left
            int degrees = angleEnd;
            degrees -= angleStart;
            if (degrees < 0) { degrees += 360; }

            angleEnd = angleStart + degrees;
        }
        else
        {
            //right
            int degrees = angleStart;
            degrees -= angleEnd;
            if (degrees < 0) { degrees += 360; }

            angleEnd = angleStart - degrees;
        }

        lerpTime = 0f;
    }

    /// <summary>
    /// Sets up moving and turning simultaneously for "strafing"
    /// </summary>
    /// <param name="goal">Target position</param>
    /// <param name="targetDirection">Target direction to face</param>
    protected void BeginMovingAndTurning(Vector3 goal, int targetDirection)
    {
        status = NewEnemyStatus.MovingAndTurning;

        moveStart = transform.position;
        moveEnd = goal;

        //normalize angles and numerically determine which direction to turn
        angleStart = currentFacingAngle;
        while (angleStart > 360) { angleStart -= 360; }
        while (angleStart < 0) { angleStart += 360; }
        angleEnd = targetDirection;
        int difference = angleEnd - angleStart;
        if (difference < 0) { difference += 360; }
        if (difference <= 180)
        {
            //left
            int degrees = angleEnd;
            degrees -= angleStart;
            if (degrees < 0) { degrees += 360; }

            angleEnd = angleStart + degrees;
        }
        else
        {
            //right
            int degrees = angleStart;
            degrees -= angleEnd;
            if (degrees < 0) { degrees += 360; }

            angleEnd = angleStart - degrees;
        }

        lerpTime = 0f;
    }



    //********************************************************************************************FINISH
    protected void LookAround(int degrees)
    {
        status = NewEnemyStatus.LookingAround;

        angleStart = currentFacingAngle;
        angleEnd = currentFacingAngle + degrees;
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
