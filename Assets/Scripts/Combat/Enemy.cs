using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : CombatChar
{
    // Use this for initialization
    protected override void Awake ()
    {
        //default testing values
        health = 10;
        maxHealth = 10;
        speed = 0;
        maxSpeed = 0;
	}
	
	// Update is called once per frame
	void Update ()
    {
		
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

    public override IEnumerator TakeTurn()
    {
        //run the enemy's AI here
        yield return null;
    }

    public override void BeginTurn()
    {
        throw new System.NotImplementedException();
    }
}
