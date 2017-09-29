using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayableChar : MonoBehaviour, ICombatChar
{
    #region Instance data
    protected int health;
    protected int maxHealth;
    protected int speed;
    protected int maxSpeed;
    protected int strength;
    protected int dexterity;
    protected int intelligence;
    #endregion

    #region Properties
    /// <summary>
    /// Character's current health
    /// </summary>
    public int Health
    {
        get { return health; }
        set {  health = value; }
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
    /// Characters intelligence. Used for magic damage
    /// </summary>
    public int Intelligence
    {
        get { return intelligence; }
        set { intelligence = value; }
    }
    #endregion


    // Use this for initialization
    void Start ()
    {
        //default testing values
        health = 10;
        maxHealth = 10;
        strength = 12;
        dexterity = 12;
        intelligence = 12;
        speed = 5;
        maxSpeed = 5;
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    /// <summary>
    /// Calculates the initiative of the character
    /// </summary>
    /// <returns>The calculated initiative value</returns>
    public int GetInitiative()
    {
        //do some stuff to get an initiative value


        //returns 1 right now to ensure that a value is returned and that 
        //the value is higher than any test enemies (they will get 0)
        return 1;
    }
}
