using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyStatus { Patrolling, Searching, Attacking }

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

    SearchZone currentSearchArea;
    List<List<Vector3>> positionsToSearch;
    [SerializeField] List<GameObject> searchZoneList;
    protected Dictionary<SearchZone, bool> searchZones;
    protected List<Vector3> lastKnownPosition;








    protected int speedRemaining;








    protected EnemyStatus status;
    [SerializeField] List<Vector3> patrolPositions;
    protected int patrolPositionIndex;
    protected bool isMoving;
    protected bool isTurning;
    [SerializeField] int visionRange;
    [SerializeField] int currentFacingAngle;
    [SerializeField] int visionAngle;
    Dictionary<Vector3, GameObject> sightConeIndicators;
    protected GameObject sightCanvas;

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
        searchZones = new Dictionary<SearchZone, bool>();
        foreach(GameObject searchZone in searchZoneList)
        {
            searchZones[searchZone.GetComponent<SearchZone>()] = false;
        }

















        //inherited control variables
        finishedTurn = true; //********************************************************************************************************************SET TO TRUE
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

        status = EnemyStatus.Searching;
        patrolPositionIndex = -1;

        //visionAngle = 90;

        sightConeIndicators = new Dictionary<Vector3, GameObject>();
    }


    /// <summary>
    /// Starts the coroutine that handles a character's turn
    /// </summary>
    public override void BeginTurn()
    {
        speedRemaining = speed;

        if (status == EnemyStatus.Patrolling)
        {
            StartCoroutine("Patrol");
        }else if(status == EnemyStatus.Searching)
        {
            StartCoroutine("Search");
        }
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
            path = Node.FindPath(transform.position, patrolPositions[patrolPositionIndex]);
        }

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

    /// <summary>
    /// 
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
                int pathDistance = Node.PathDistance(transform.position, searchZone.Key.transform.position);
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
                if (Node.CheckSquare(transform.position, positionsToSearch[i][j], speed))
                {
                    reachablePositions.Add(positionsToSearch[i][j]);
                }
            }
        }




        if(reachablePositions.Count == 0)
        {
            int index = 0;
            int shortestDistance = int.MaxValue;

            for (int i = 0; i < positionsToSearch.Count; i++)
            {
                for (int j = 0; j < positionsToSearch[i].Count; j++)
                {
                    int pathDistance = Node.PathDistance(transform.position, positionsToSearch[i][j]);
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
            List<Vector3> path = Node.FindPath(transform.position, reachablePositions[index]);

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
        for (int i = 0; i < positionsToSearch.Count; i++)
        {
            //if (positionsToSearch[i].Contains(transform.position))
            //{
            //    positionsToSearch.Remove(positionsToSearch[i]);
            //    i = positionsToSearch.Count;
            //}


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
        Vector3 startPos = transform.position;
        float t = 0; //time
        float moveSpeed = 5f;


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

        while (isTurning) { yield return null; }



        //smoothly moves the character across the distance with lerp
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        CalculateVisionCone();

        //when done moving allow more input to be received
        isMoving = false;
    }









    protected void CalculateVisionCone()
    {
        Destroy(sightCanvas);

        //determines the bounds of the play area
        CombatSceneController controller = GameObject.FindGameObjectWithTag("SceneController").GetComponent<CombatSceneController>();
        Vector3 bottomLeft = controller.BottomLeftCorner;
        Vector3 topRight = controller.TopRightCorner;

        //sets up needed UI elements and calculations
        sightCanvas = Instantiate(GameController.CanvasPrefab);
        Vector2 bottom = Camera.main.WorldToScreenPoint(new Vector3(0, -.5f));
        Vector2 top = Camera.main.WorldToScreenPoint(new Vector3(0, .5f));
        Vector2 sightIndicatorDimensions = new Vector2(top.y - bottom.y, top.y - bottom.y);

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


                    Vector3 sightSquare = new Vector3(i, j);
                    if (pointAngle >= startAngle && pointAngle <= endAngle && !Physics2D.Linecast(transform.position, sightSquare)
                        && sightSquare.x >= bottomLeft.x && sightSquare.x <= topRight.x && sightSquare.y >= bottomLeft.y && sightSquare.y <= topRight.y)
                    {
                        sightConeIndicators[sightSquare] = Instantiate(GameController.SightSquarePrefab);
                        sightConeIndicators[sightSquare].transform.SetParent(sightCanvas.transform);
                        sightConeIndicators[sightSquare].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(sightSquare);
                        sightConeIndicators[sightSquare].GetComponent<RectTransform>().sizeDelta = sightIndicatorDimensions;
                    }
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

    /// <summary>
    /// Turns to a specific angle
    /// </summary>
    /// <param name="targetDirection">The direction to turn to</param>
    protected IEnumerator Turn(int targetDirection)
    {
        isTurning = true;

        int startAngle = currentFacingAngle;
        while (startAngle > 360) { startAngle -= 360; }
        while (startAngle < 0) { startAngle += 360; }

        int goalAngle = targetDirection;
        int difference = goalAngle - startAngle;










        if (difference < 0) { difference += 360; }

        if(difference <=180)
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
        while(t < 1f)
        {
            t += Time.deltaTime * turnSpeed;
            currentFacingAngle = (int)Mathf.Lerp(startAngle, goalAngle, t);
            CalculateVisionCone();
            yield return null;
        }
        currentFacingAngle = targetDirection;


        isTurning = false;
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
            Destroy(sightCanvas);
            //run the death animation here
            Destroy(gameObject);
        }

        //resume the attacking object's turn
        takingDamage = false;

        yield break;
    }
}
