using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : CombatChar
{
    // Use this for initialization
    protected void Awake ()
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

    /// <summary>
    /// Handles the entire turn for this enemy
    /// </summary>
    protected override IEnumerator TakeTurn()
    {
        //the finishedTurn variable tells the turn handler to wait until TakeTurn() completes before starting the next turn
        finishedTurn = false;

        yield return null;

        Debug.Log("This enemy takes a turn!");

        //this will cause the turn manager to begin the next turn
        finishedTurn = true;
    }
}
