using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC combat participants
/// </summary>
public class Enemy : CombatChar
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
    #endregion

    #region Fields and properties for game flow
    private bool finishedTurn;
    private bool takingDamage;

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
        //ID???

        //inherited stats
        health = 10;
        maxHealth = 10;
        speed = 0;
        maxSpeed = 0;
        strength = 10;
        dexterity = 10;
        intelligence = 10;
        defense = 5;
        resistance = 5;
        attackRange = 5;
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
        StartCoroutine("TakeTurn");
    }

    /// <summary>
    /// Handles the entire turn for this enemy
    /// </summary>
    protected IEnumerator TakeTurn()
    {
        //the finishedTurn variable tells the turn handler to wait until TakeTurn() completes before starting the next turn
        finishedTurn = false;

        Debug.Log("This enemy takes a turn!");

        //this will cause the turn manager to begin the next turn
        finishedTurn = true;


        yield return null;
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
