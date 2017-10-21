using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CombatChar : MonoBehaviour
{
    #region Fields
    //character stats
    protected int health;
    protected int maxHealth;
    protected int speed;
    protected int maxSpeed;
    protected int strength;
    protected int dexterity;
    protected int intelligence;
    protected int defense;
    protected int resistance;

    //control variables
    protected bool finishedTurn;
    protected int id;

    //private control variables
    private bool takingDamage;
    #endregion

    #region Properties
    /// <summary>
    /// Character's current health
    /// </summary>
    public int Health
    {
        get { return health; }
        set { health = value; }
    }
    /// <summary>
    /// Character's maximum health
    /// </summary>
    public int MaxHealth
    {
        get { return maxHealth; }
        set { maxHealth = value; }
    }
    /// <summary>
    /// Character's movement speed
    /// </summary>
    public int Speed
    {
        get { return speed; }
        set { speed = value; }
    }
    /// <summary>
    /// Character's max speed
    /// </summary>
    public int MaxSpeed
    {
        get { return maxSpeed; }
        set { maxSpeed = value; }
    }
    /// <summary>
    /// Character's strength. Used for physical damage
    /// </summary>
    public int Strength
    {
        get { return strength; }
        set { strength = value; }
    }
    /// <summary>
    /// Character's dexterity. Used for speed and initiative
    /// </summary>
    public int Dexterity
    {
        get { return dexterity; }
        set { dexterity = value; }
    }
    /// <summary>
    /// Character's intelligence. Used for magic damage
    /// </summary>
    public int Intelligence
    {
        get { return intelligence; }
        set { intelligence = value; }
    }
    /// <summary>
    /// Character's defense. Used to defend against physical attacks
    /// </summary>
    public int Defense
    {
        get { return defense; }
        set { defense = value; }
    }
    /// <summary>
    /// Character's resistance. Used to defend against magical attacks
    /// </summary>
    public int Resistance
    {
        get { return resistance; }
        set { resistance = value; }
    }
    /// <summary>
    /// Character's unique ID. Used for targetting
    /// </summary>
    public int ID
    {
        get { return id; }
    }

    /// <summary>
    /// This bool will be set to true at the end of a character's turn.
    /// This will be used to tell the turn handler to move on to the next turn.
    /// </summary>
    public bool FinishedTurn
    {
        get { return finishedTurn; }
    }
    /// <summary>
    /// Gets true when taking damage and false otherwise
    /// </summary>
    public bool TakingDamage
    {
        get { return takingDamage; }
    }
    #endregion

    //calculates initiative for the character for the turn
    //implemented differently in PCs and NPCs
    public abstract int GetInitiative();

    /// <summary>
    /// Handles a character's turn
    /// </summary>
    protected abstract IEnumerator TakeTurn();

    /// <summary>
    /// Starts the coroutine that handles a character's turn
    /// </summary>
    public void BeginTurn()
    {
        StartCoroutine("TakeTurn");
    }

    public void BeginTakeDamage(int damage)
    {
        StartCoroutine(TakeDamage(damage));
    }

    /// <summary>
    /// Runs when a character takes damage
    /// </summary>
    /// <param name="damage">The amount of damage to take</param>
    public IEnumerator TakeDamage(int damage)
    {
        //this tells the attacking character to pause while this method happens
        takingDamage = true;

        //takes the damage
        health -= damage;
        if(health < 0) { health = 0; }

        //play the taking damage animation here and make sure it takes the correct amount of time
        //yield return null;

        if(health == 0)
        {
            //run the death animation here

            Debug.Log("ooooooookay");

            //GameObject thisCharacter = this.GetComponent<GameObject>();
            Destroy(gameObject);
        }

        //resume the attacking object's turn
        takingDamage = false;

        yield break;
    }
}
