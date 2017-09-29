using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatChar : MonoBehaviour
{
    #region Instance data
    protected int health;
    protected int maxHealth;
    protected int speed;
    protected int maxSpeed;
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
    #endregion

    // Use this for initialization
    void Start ()
    {
        health = 10;
        maxHealth = 10;
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
    public virtual int GetInitiative()
    {
        //do some stuff to get an initiative value

        //testing value
        return 1;
    }

}
