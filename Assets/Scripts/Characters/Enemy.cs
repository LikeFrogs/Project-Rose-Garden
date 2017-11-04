using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyStatus { Patrolling, Attacking, Hunting }

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
    [SerializeField] int speed;
    protected int maxSpeed;
    [SerializeField] int attack;
    [SerializeField] int magicAttack;
    [SerializeField] int defense;
    [SerializeField] int resistance;
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
    /// Gets character's attack range
    /// </summary>
    public override int AttackRange
    {
        get { return attackRange; }
    }
    #endregion

    #region Fields and properties for game flow
    protected EnemyStatus status;
    [SerializeField] List<Vector3> patrolPositions;
    protected int patrolPositionIndex;
    protected bool isMoving;

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
        //inherited control variables
        finishedTurn = false;

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

        status = EnemyStatus.Patrolling;
        patrolPositionIndex = -1;
    }

    // Update is called once per frame
    void Update()
    {

    }


    /// <summary>
    /// Starts the coroutine that handles a character's turn
    /// </summary>
    public override void BeginTurn()
    {
        if (status == EnemyStatus.Patrolling)
        {
            StartCoroutine("Patroll");
        }
    }

    /// <summary>
    /// Handles the entire turn for this enemy
    /// </summary>
    protected IEnumerator Patroll()
    {
        //the finishedTurn variable tells the turn handler to wait until TakeTurn() completes before starting the next turn
        finishedTurn = false;

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
                //**************************************** Do any vision checks and reactions here after the enemy finishes its move to the next square *************************************************
            }
        }
        //this will cause the turn manager to begin the next turn
        finishedTurn = true;
    }

    /// <summary>
    /// Smoothly moves the character from their current position to a position one tile in the direction of input
    /// </summary>
    /// <param name="input">The target position to move to</param>
    private IEnumerator Move(Vector3 endPos)
    {
        isMoving = true; //while running this routine the AI waits to resume 
        Vector3 startPos = transform.position;
        float t = 0; //time
        float moveSpeed = 5;

        //prevents players from moving into unreachable squares
        //if (!moveRange.Contains(endPos))
        //{
        //    t = 1f;
        //}

        //smoothly moves the character across the distance with lerp
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
    /// Calculates the initiative of the enemy
    /// </summary>
    /// <returns>The calculated initiative value</returns>
    public override int GetInitiative()
    {
        //determine the enemy's initiative
        //we could just do a set value per enemy

        //for testing purposes returns 0 (1 less than playable characters)
        return 0;
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
}
