using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
//using System.Diagnostics;

public enum EnemyStatus { Patrolling, Searching, CanSeeTarget, Investigating, Testing }
public enum WorkingStatus { WaitingOnCoroutine, Moving, Turning, MovingAndTurning, LookingAround, WaitingForSeconds }
public enum AlertStatus { Unalerted, SemiAlert, HighAlert }

/// <summary>
/// NPC combat participants
/// </summary>
public class Enemy : CombatChar
{
    protected AlertStatus alerted;




    #region Stats fields and properties
    protected int level;
    [SerializeField] protected int expForKill;

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
    ///  Character's current level
    /// </summary>
    public override int Level { get { return level; } }
    /// <summary>
    /// The amount of exp a character earns for killing this enemy
    /// </summary>
    public int ExpForKill { get { return expForKill; } }

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
    //ai state decoupled from alert state for use in update
    protected WorkingStatus workingStatus;

    //update version of move
    protected Vector3 moveStart;
    protected Vector3 moveEnd;
    protected float lerpTime;

    //update version of turn and look around
    protected int originalAngle; //only for look around
    protected int lookDegrees; //only for look around
    protected float lookSpeed; //only for look around
    protected int startAngle;
    protected int goalAngle;

    //update version of wait for seconds
    protected float goalSeconds;
    protected float currentSeconds;
    protected WorkingStatus prevStatus;

    //target tracking
    protected Vector3 lastKnownPosition;
    protected List<CombatChar> targets;
    protected List<CombatChar> currentlySeenTargets;

    //character UI
    protected List<GameObject> unusedSightConeIndicators;
    protected Dictionary<Vector3, GameObject> sightConeIndicators;
    protected GameObject canvas;

    //patrol related
    [SerializeField] List<Vector3> patrolPositions;
    protected int patrolPositionIndex;

    //search related
    SearchZone currentSearchArea;
    List<List<Vector3>> positionsToSearch;
    Vector3 searchTarget;
    [SerializeField] List<GameObject> searchZoneList;
    protected Dictionary<SearchZone, bool> searchZones;
    protected int speedRemaining;
    
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
    
    /// <summary>
    /// Use this for initialization
    /// </summary>
    protected void Awake()
    {
        searchTarget.x = -1;

        positionsToSearch = new List<List<Vector3>>();

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
    /// Runs every frame
    /// </summary>
    protected virtual void Update()
    {
        //pause execution of some action
        if(workingStatus == WorkingStatus.WaitingForSeconds)
        {
            currentSeconds += Time.deltaTime;
            
            if(currentSeconds >= goalSeconds)
            {
                workingStatus = prevStatus;
            }
        }

        //moves the enemy smoothly to its target position
        if (workingStatus == WorkingStatus.Moving)
        {
            //smoothly move from one position to another
            lerpTime += Time.deltaTime * 5;
            transform.position = Vector3.Lerp(moveStart, moveEnd, lerpTime);

            //send control back to coroutine when done
            if (lerpTime >= 1f)
            {
                CalculateVisionCone();

                workingStatus = WorkingStatus.WaitingOnCoroutine;
            }
        }

        //turns the enemy smoothly towards its target direction
        if(workingStatus == WorkingStatus.Turning)
        {
            //smoothly turn vision cone
            lerpTime += Time.deltaTime * 1.5f;
            currentFacingAngle = (int)Mathf.Lerp(startAngle, goalAngle, lerpTime);
            CalculateVisionCone();

            //return control to coroutine when done
            if(lerpTime >= 1f)
            {
                //ensures that the final facing angles are exact
                currentFacingAngle = goalAngle;
                while(currentFacingAngle < 0) { currentFacingAngle += 360; }
                while(currentFacingAngle >= 360) { currentFacingAngle -= 360; }

                workingStatus = WorkingStatus.WaitingOnCoroutine;
            }
        }

        //turns and moves the enemy smoothly towards target position and direction
        if(workingStatus == WorkingStatus.MovingAndTurning)
        {
            //smoothly move and turn
            lerpTime += Time.deltaTime * 3.5f;
            currentFacingAngle = (int)Mathf.Lerp(startAngle, goalAngle, lerpTime);
            CalculateVisionCone(true);
            transform.position = Vector3.Lerp(moveStart, moveEnd, lerpTime);

            //return control to coroutine
            if(lerpTime >= 1f)
            {
                //ensures that the final facing angles are exact
                currentFacingAngle = goalAngle;
                while (currentFacingAngle < 0) { currentFacingAngle += 360; }
                while (currentFacingAngle >= 360) { currentFacingAngle -= 360; }

                CalculateVisionCone();

                workingStatus = WorkingStatus.WaitingOnCoroutine;
            }
        }

        //the enemy looks around for its targets
        if(workingStatus == WorkingStatus.LookingAround)
        {
            //smoothly turn between angles
            lerpTime += Time.deltaTime * (lookSpeed);
            currentFacingAngle = (int)Mathf.Lerp(startAngle, goalAngle, lerpTime);
            CalculateVisionCone();

            //either return control to the coroutine or set up the next turn when finished turning
            if(lerpTime >= 1f)
            {
                currentFacingAngle = goalAngle;

                if(currentFacingAngle == originalAngle - lookDegrees)
                {
                    goalAngle = originalAngle + lookDegrees;
                    startAngle = currentFacingAngle;
                    lerpTime = 0;

                    lookSpeed /= 2;

                    WaitForSeconds(.5f);
                }
                else if(currentFacingAngle == originalAngle + lookDegrees)
                {
                    goalAngle = originalAngle;
                    startAngle = currentFacingAngle;
                    lerpTime = 0;

                    lookSpeed *= 2;

                    WaitForSeconds(.5f);
                }
                else if(currentFacingAngle == originalAngle)
                {
                    workingStatus = WorkingStatus.WaitingOnCoroutine;
                }

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
        //targets = CombatSceneController.GoodGuys;
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

        speedRemaining = speed; //recalculate for slowing effects if necessary
        CalculateVisionCone();
        
        //Finished
        if (status == EnemyStatus.Patrolling)
        {
            StartCoroutine(Patrol());
        }
        else if(status == EnemyStatus.Searching)
        {
            StartCoroutine(Search());
        }
        else if(status == EnemyStatus.CanSeeTarget)
        {
            StartCoroutine(CanSeeTarget());
        }


        else if(status == EnemyStatus.Investigating)
        {
            StartCoroutine(Investigate());
        }


        else if(status == EnemyStatus.Testing)
        {
            StartCoroutine(Testing());
        }
        //******************************************************************Possibly some kind of special hunt routine for right after losing sight of a target?
    }
    
    /// <summary>
    /// Handles the entire turn for this enemy
    /// </summary>
    protected IEnumerator Patrol()
    {
        //keep up with the position that the character will move to next and
        //start its next turn in (assuming nothing happens to change the character's state
        patrolPositionIndex++;
        if(patrolPositionIndex >= patrolPositions.Count) { patrolPositionIndex = 0; }

        //gets a path to the next 
        List<Vector3> path = null;
        if (transform.position != patrolPositions[patrolPositionIndex])
        {
            //float[,] moveCosts = CombatSceneController.MoveCosts;
            //List<CombatChar> goodGuys = CombatSceneController.GoodGuys;
            //for (int i = 0; i < goodGuys.Count; i++)
            //{
            //    moveCosts[(int)goodGuys[i].transform.position.x, (int)goodGuys[i].transform.position.y] = 0;
            //}
            //path = AStarNode.FindPath(transform.position, patrolPositions[patrolPositionIndex], moveCosts);
            path = AStarNode.FindPath(transform.position, patrolPositions[patrolPositionIndex], sightConeIndicators);
        }

        //move through every square in the calculated path
        if (path != null)
        {
            foreach (Vector3 position in path)
            {
                if (speedRemaining > 0)
                {
                    //check to see if turning is necessary
                    if (position.x == transform.position.x - 1 && currentFacingAngle != 180)
                    {
                        StartTurning(180);
                    }
                    else if (position.x == transform.position.x + 1 && currentFacingAngle != 0)
                    {
                        StartTurning(0);
                    }
                    else if (position.y == transform.position.y - 1 && currentFacingAngle != 270)
                    {
                        StartTurning(270);
                    }
                    else if (position.y == transform.position.y + 1 && currentFacingAngle != 90)
                    {
                        StartTurning(90);
                    }
                    //wait while turning
                    while (workingStatus == WorkingStatus.Turning) { yield return null; }

                    //start a move                    
                    StartMoving(position);

                    //wait until finished moving
                    while (workingStatus == WorkingStatus.Moving) { yield return null; }
                    speedRemaining--;
                }
            }
        }

        //this will cause the turn manager to begin the next turn
        finishedTurn = true;
    }

    //******************************Possibly intelligently select one search zone that the target wouldn't be in (the enemy was just there/saw the target run the other way/etc.)
    /// <summary>
    /// Searches the area based on editor assigned search areas
    /// </summary>
    protected IEnumerator Search()
    {
        float[,] moveCosts = CombatSceneController.MoveCosts;
        List<CombatChar> goodGuys = CombatSceneController.GoodGuys;
        for (int i = 0; i < goodGuys.Count; i++)
        {
            moveCosts[(int)goodGuys[i].transform.position.x, (int)goodGuys[i].transform.position.y] = 0;
        }

        //determines the next area to search based on distance
        if (currentSearchArea == null)
        {
            int shortestDistance = int.MaxValue;
            foreach (KeyValuePair<SearchZone, bool> searchZone in searchZones)
            {

                int pathDistance = AStarNode.PathDistance(transform.position, searchZone.Key.transform.position, moveCosts);
                if (searchZone.Value == false && pathDistance <= shortestDistance)
                {
                    shortestDistance = pathDistance;
                    currentSearchArea = searchZone.Key;
                }
            }

            positionsToSearch.Clear();
            positionsToSearch.AddRange(currentSearchArea.KeyPositionLists);
        }

        List<Vector3> reachablePositions = new List<Vector3>();
        if (searchTarget.x == -1)
        {
            //create a list of all reachable positions in the current search zone
            for (int i = 0; i < positionsToSearch.Count; i++)
            {
                for (int j = 0; j < positionsToSearch[i].Count; j++)
                {
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

            searchTarget = reachablePositions[(new System.Random()).Next(reachablePositions.Count)];
        }

        //if the enemy can reach one of the positions that needs to be searched it will do so this turn
        //if (reachablePositions.Count > 0)
        //{

        //List<Vector3> path = AStarNode.FindPath(transform.position, searchTarget, moveCosts);
        List<Vector3> path = AStarNode.FindPath(transform.position, searchTarget, sightConeIndicators);

        //int index = (new System.Random()).Next(reachablePositions.Count);
        //List<Vector3> path = AStarNode.FindPath(transform.position, reachablePositions[index], moveCosts);

        if (path != null)
        {
            foreach (Vector3 position in path)
            {
                if (speedRemaining > 0)
                {
                    //check to see if turning is necessary
                    if (position.x == transform.position.x - 1 && currentFacingAngle != 180)
                    {
                        StartTurning(180);
                    }
                    else if (position.x == transform.position.x + 1 && currentFacingAngle != 0)
                    {
                        StartTurning(0);
                    }
                    else if (position.y == transform.position.y - 1 && currentFacingAngle != 270)
                    {
                        StartTurning(270);
                    }
                    else if (position.y == transform.position.y + 1 && currentFacingAngle != 90)
                    {
                        StartTurning(90);
                    }
                    //wait while turning
                    while (workingStatus == WorkingStatus.Turning) { yield return null; }

                    //set the movement destination
                    StartMoving(position);
                    //wait until finished moving
                    while (workingStatus == WorkingStatus.Moving) { yield return null; }
                    speedRemaining--;
                }
            }
            if (reachablePositions.Count == 1) { searchTarget.x = -1; }
        }
        //}

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

                    StartLookingAround(45);
                    while (workingStatus == WorkingStatus.LookingAround || workingStatus == WorkingStatus.WaitingForSeconds) { yield return null; }

                    searchTarget.x = -1;
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
                if (zones.Count == 1) { searchZones[zones[i]] = false; i = zones.Count; }
                else
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

        CalculateVisionCone();

        //this will cause the turn manager to begin the next turn
        finishedTurn = true;
    }

    //still needs some intelligence upgrades: target and path selection is still rather rudimentary
    /// <summary>
    /// Handles situations when this Enemy can see hostile targets
    /// </summary>
    protected IEnumerator CanSeeTarget()
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
            StartTurning(targetDirection);
            //wait while turning
            while (workingStatus == WorkingStatus.Turning) { yield return null; }
        }
        #endregion

        WaitForSeconds(.5f);
        //wait until finished
        while (workingStatus == WorkingStatus.WaitingForSeconds) { yield return null; }

        //find the positions the enemy can see and move to
        List<Vector3> seenMoveRange = GetSeenMoveRange();

        //creates a list of all targets that the enemy can attack from it's current position
        List<CombatChar> reachableTargets = new List<CombatChar>();
        foreach (CombatChar potentialTarget in currentlySeenTargets)
        {
            int distance = System.Math.Abs((int)potentialTarget.transform.position.x - (int)transform.position.x) + System.Math.Abs((int)potentialTarget.transform.position.y - (int)transform.position.y);
            if (distance <= attackRange)
            {
                reachableTargets.Add(potentialTarget);
            }
        }


        //creates list of all targets that the enemy can reach from it's seenMoveRange 
        List<CombatChar> otherReachableTargets = new List<CombatChar>();
        for(int i = 0; i  < seenMoveRange.Count; i++)
        {
            foreach (CombatChar potentialTarget in currentlySeenTargets)
            {
                int distance = System.Math.Abs((int)potentialTarget.transform.position.x - (int)seenMoveRange[i].x) + System.Math.Abs((int)potentialTarget.transform.position.y - (int)seenMoveRange[i].y);
                if (distance <= attackRange && !reachableTargets.Contains(potentialTarget) && !otherReachableTargets.Contains(potentialTarget))
                {
                    otherReachableTargets.Add(potentialTarget);

                    //leave loop early if all seen targets are determined to be reachable
                    if(otherReachableTargets.Count + reachableTargets.Count == currentlySeenTargets.Count)
                    {
                        i = seenMoveRange.Count;
                    }
                }
            }
        }

        //choose a target
        //probably do an abstract method for this ***********************************************************************************************************************************
        CombatChar target = null;
        bool moving = false;
        if (reachableTargets.Count > 0)
        {
            target = reachableTargets[0];
        }
        else if(otherReachableTargets.Count > 0)
        {
            target = otherReachableTargets[0];
            moving = true;
        }

        //move if necessary
        if(moving)
        {
            int shortestDistance = int.MaxValue;
            Vector3 targetPos = new Vector3(-1, -1);
            for(int i = 0; i < seenMoveRange.Count; i++)
            {
                int pathDistance = AStarNode.PathDistance(transform.position, seenMoveRange[i], sightConeIndicators);
                int distance = System.Math.Abs((int)target.transform.position.x - (int)seenMoveRange[i].x) + System.Math.Abs((int)target.transform.position.y - (int)seenMoveRange[i].y);
               
                if (pathDistance <= shortestDistance && distance <= attackRange)
                {
                    shortestDistance = pathDistance;
                    targetPos = seenMoveRange[i];
                }
            }

            List<Vector3> path = AStarNode.FindPath(transform.position, targetPos, sightConeIndicators);
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
                        StartMovingAndTurning(position, targetDirection);
                        //wait until finished
                        while (workingStatus == WorkingStatus.MovingAndTurning) { yield return null; }
                        speedRemaining--;
                    }
                }
            }
        }

        //if the enemy has a target, it attacks
        if (target != null)
        {
            //calculates damage to apply and calls TakeDamage()
            //int damage = attack + attack - target.Defense;
            target.BeginTakeDamage(100);
            while (target.TakingDamage) { yield return null; }

            yield return null; //wait for one Update so that the target is registered as null if it is destroyed
            while (currentlySeenTargets.Contains(null))
            {
                currentlySeenTargets.Remove(null);
            }


            //NEEDS TO TAKE INTO ACCOUNT PREVIOUSLY SEEN ENEMIES FIX BY LATER *********************************************************************************************************************************************************************************************************************************************************************************
            if(currentlySeenTargets.Count == 0)
            {
                status = EnemyStatus.Patrolling;
            }
        }
        //if none of the seen targets can be reched, proceed as follows
        else
        {
            target = currentlySeenTargets[0];

            int shortestDistance = int.MaxValue;
            Vector3 targetPos = new Vector3(-1, -1);
            for (int i = 0; i < seenMoveRange.Count; i++)
            {
                int pathDistance = AStarNode.PathDistance(seenMoveRange[i], target.transform.position, sightConeIndicators);
                
                if (pathDistance <= shortestDistance)
                {
                    shortestDistance = pathDistance;
                    targetPos = seenMoveRange[i];
                }
            }

            List<Vector3> path = AStarNode.FindPath(transform.position, targetPos, sightConeIndicators);
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
                        StartMovingAndTurning(position, targetDirection);
                        //wait until finished
                        while (workingStatus == WorkingStatus.MovingAndTurning) { yield return null; }
                        speedRemaining--;
                    }
                }
            }
        }

        finishedTurn = true;
    }
    
    //When transitioning to searching, make the direction slightly more intelligent
    /// <summary>
    /// Investigate a target that moved through, but did not end, within this Enemy's range of vision. Transitions to Searching() if 
    /// the target is not easily found
    /// </summary>
    protected IEnumerator Investigate()
    {
        //calculates and turns towards the average of all target positions
        int relX = (int)lastKnownPosition.x - (int)transform.position.x;

        int relY = (int)lastKnownPosition.y - (int)transform.position.y;

        double targetDirection = System.Math.Atan2(relY, relX) * 180 / System.Math.PI;
        while (targetDirection > 360) { targetDirection -= 360; }
        while (targetDirection < 0) { targetDirection += 360; }

        if (currentFacingAngle != (int)targetDirection)
        {
            StartTurning((int)targetDirection);
            //wait while turning
            while (workingStatus == WorkingStatus.Turning) { yield return null; }
        }

        StartLookingAround(45);
        while (workingStatus == WorkingStatus.LookingAround || workingStatus == WorkingStatus.WaitingForSeconds) { yield return null; }

        status = EnemyStatus.Searching;
        alerted = AlertStatus.HighAlert;

        RestartTurn(Search());
    }

    /// <summary>
    /// Restarts this Enemy's turn without ending it first. Skips BeginTunr() and starts straight into the provided coroutine.
    /// Use when this Enemy's state changes inside a coroutine
    /// </summary>
    /// <param name="routine">The turn coroutine that should be used</param>
    private void RestartTurn(IEnumerator routine)
    {
        StopAllCoroutines();
        StartCoroutine(routine);
    }

    /// <summary>
    /// Begin a move
    /// </summary>
    /// <param name="targetPos">The position to move to</param>
    protected void StartMoving(Vector3 targetPos)
    {
        moveStart = transform.position;
        moveEnd = targetPos;
        lerpTime = 0f;
        workingStatus = WorkingStatus.Moving;
    }

    /// <summary>
    /// Begin turning around
    /// </summary>
    /// <param name="targetDirection">The desired angle to be facing</param>
    protected void StartTurning(int targetDirection)
    {
        //normalize angles and numerically determine which direction to turn
        startAngle = currentFacingAngle;
        while (startAngle > 360) { startAngle -= 360; }
        while (startAngle < 0) { startAngle += 360; }
        goalAngle = targetDirection;
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

        lerpTime = 0;
        workingStatus = WorkingStatus.Turning;
    }

    /// <summary>
    /// Begin moving and turning simultaneously "strafing"
    /// </summary>
    /// <param name="targetPos">Position to move to</param>
    /// <param name="targetDirection">Direction to turn towards</param>
    protected void StartMovingAndTurning(Vector3 targetPos, int targetDirection)
    {
        //normalize angles to [0, 360] and then numerically determine which direction to turn
        //normalize angles and numerically determine which direction to turn
        startAngle = currentFacingAngle;
        while (startAngle > 360) { startAngle -= 360; }
        while (startAngle < 0) { startAngle += 360; }
        goalAngle = targetDirection;
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

        moveStart = transform.position;
        moveEnd = targetPos;
        lerpTime = 0f;


        workingStatus = WorkingStatus.MovingAndTurning;
    }

    /// <summary>
    /// Begin looking in both directions
    /// </summary>
    /// <param name="degrees">The number of degrees from the current center to look</param>
    protected void StartLookingAround(int degrees)
    {
        originalAngle = currentFacingAngle;
        startAngle = currentFacingAngle;
        lookDegrees = degrees;
        goalAngle = currentFacingAngle - lookDegrees;
        lerpTime = 0;
        lookSpeed = .9f;

        workingStatus = WorkingStatus.LookingAround;
    }

    /// <summary>
    /// Wait to continue execution of the current task
    /// </summary>
    /// <param name="seconds">The number of seconds to wait</param>
    protected void WaitForSeconds(float seconds)
    {
        //fixes a bug where this method was being run twice from only one call
        if(workingStatus == WorkingStatus.WaitingForSeconds) { return; }

        currentSeconds = 0;
        goalSeconds = seconds;

        prevStatus = workingStatus;
        workingStatus = WorkingStatus.WaitingForSeconds;
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
        //CombatSceneController controller = GameObject.FindGameObjectWithTag("SceneController").GetComponent<CombatSceneController>();
        CombatSceneController controller = GameObject.FindGameObjectWithTag("SceneController").GetComponent<CombatSceneController>();
        //Vector3 bottomLeft = controller.BottomLeftCorner;
        Vector3 bottomLeft = Vector3.zero;
        Vector3 topRight = controller.TopRightCorner;

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
                    int startAngle = currentFacingAngle - theta;
                    int endAngle = currentFacingAngle + theta;
                    double pointAngle = System.Math.Atan2(y, x) * (180 / System.Math.PI);
                    //normalize angles to [0, 360]
                    while (startAngle > 360) { startAngle -= 360; }
                    while (startAngle < 0) { startAngle += 360; }
                    while (endAngle > 360) { endAngle -= 360; }
                    while (endAngle < 0) { endAngle += 360; }
                    while (pointAngle > 360) { pointAngle -= 360; }
                    while (pointAngle < 0) { pointAngle += 360; }
                    //normalize angles to [0, 360] with startAngle = 0
                    endAngle -= startAngle;
                    pointAngle -= startAngle;
                    startAngle = 0;
                    if (endAngle < 0) { endAngle += 360; }
                    if (pointAngle < 0) { pointAngle += 360; }

                    //if the square can be seen, then instantiate a UI element in the vision cone
                    Vector3 sightSquare = new Vector3(i, j);
                    if (pointAngle >= startAngle && pointAngle <= endAngle && !Physics2D.Linecast(transform.position, sightSquare)
                        && sightSquare.x >= bottomLeft.x && sightSquare.x <= topRight.x && sightSquare.y >= bottomLeft.y && sightSquare.y <= topRight.y)
                    {
                        sightConeIndicators[sightSquare] = unusedSightConeIndicators[unusedSightConeIndicators.Count - 1];
                        unusedSightConeIndicators.RemoveAt(unusedSightConeIndicators.Count - 1);
                        sightConeIndicators[sightSquare].SetActive(true);
                        sightConeIndicators[sightSquare].GetComponent<RectTransform>().anchoredPosition = sightSquare;
                        sightConeIndicators[sightSquare].GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
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

                //if the enemy has seen A NEW target activate it's target sighted algorithm
                if (seenTargets.Count > 0)
                {
                    ////*************************************************************************************************FIX HERE
                    StopAllCoroutines();
                    Debug.Log("target seen");
                    status = EnemyStatus.CanSeeTarget;
                    workingStatus = WorkingStatus.WaitingOnCoroutine;
                    alerted = AlertStatus.HighAlert;
                    StartCoroutine(CanSeeTarget());
                }
            }
        }
    }

    /// <summary>
    /// Gets all the positions within this Enemy's move range that it can see
    /// </summary>
    /// <returns>The list of positions</returns>
    protected List<Vector3> GetSeenMoveRange()
    {
        float[,] moveCosts = CombatSceneController.MoveCosts;
        List<CombatChar> goodGuys = CombatSceneController.GoodGuys;
        for (int i = 0; i < goodGuys.Count; i++)
        {
            moveCosts[(int)goodGuys[i].transform.position.x, (int)goodGuys[i].transform.position.y] = 0;
        }

        return (from Vector3 position in DijkstraNode.MoveRange(transform.position, speedRemaining, moveCosts) where sightConeIndicators.ContainsKey(position) select position).ToList();
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
        lastKnownPosition = path[path.Count - 1];
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

        status = EnemyStatus.Investigating;
        alerted = AlertStatus.HighAlert;
    }

    /// <summary>
    /// Starts the coroutine to deal damage to a character
    /// </summary>
    /// <param name="damage">The amount of damage to deal</param>
    public override void BeginTakeDamage(int damage)
    {
        //instant kill from stealth
        if(alerted == AlertStatus.Unalerted) { StartCoroutine(TakeDamage(maxHealth)); }

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










    //****************************************************************DELETE EVENTUALLY
    /// <summary>
    /// Used to test new turn routines before they're integrated into the class
    /// </summary>
    protected IEnumerator Testing()
    {
        //while(true)
        //{
        //StartCoroutine(LookAround(45));
        //while (isTurning) { yield return null; }

        StartLookingAround(90);
        while (workingStatus == WorkingStatus.LookingAround || workingStatus == WorkingStatus.WaitingForSeconds) { yield return null; }

        finishedTurn = true;
        //}
    }
}
