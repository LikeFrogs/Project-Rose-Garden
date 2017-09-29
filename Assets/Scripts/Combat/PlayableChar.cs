using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayableChar : CombatChar
{
    #region Instance data
    protected int strength;
    protected int dexterity;
    protected int intelligence;
    #endregion

    #region Properties
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
    protected override void Start ()
    {
        //default testing values
        health = 10;
        maxHealth = 10;
        speed = 5;
        maxSpeed = 5;
        strength = 12;
        dexterity = 12;
        intelligence = 12;
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    /// <summary>
    /// Calculates the initiative of the character
    /// </summary>
    /// <returns>The calculated initiative value</returns>
    public override int GetInitiative()
    {
        //do some stuff to get an initiative value

        //testing value
        return 1;
    }
}
