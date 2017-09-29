using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, ICombatChar
{
    #region Instance data
    protected int health;
    protected int maxHealth;
    #endregion

    #region Properties
    /// <summary>
    /// Enemy's current health
    /// </summary>
    public int Health
    {
        get { return health; }
        set { health = value; }
    }
    /// <summary>
    /// Enemy's max health
    /// </summary>
    public int MaxHealth
    {
        get { return maxHealth; }
        set { maxHealth = value; }
    }
    #endregion

    // Use this for initialization
    void Start ()
    {
        //default testing values
        health = 10;
        maxHealth = 10;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// Calculates the initiative of the enemy
    /// </summary>
    /// <returns>The calculated initiative value</returns>
    public int GetInitiative()
    {
        //determine the enemy's initiative
        //we could just do a set value per enemy instance

        //for testing purposes returns 0 (1 less than playable characters)
        return 0;
    }
}
