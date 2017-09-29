using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CombatChar : MonoBehaviour
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
    protected virtual void Start ()
    {
    }

    // Update is called once per frame
    void Update ()
    {
		
	}

    public abstract int GetInitiative();

}
