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
    protected bool finishedTurn;
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
    /// This bool will be set to true at the end of a character's turn.
    /// This will be used to tell the turn handler to move on to the next turn.
    /// </summary>
    public bool FinishedTurn
    {
        get { return finishedTurn; }
    }
    #endregion

    //we can probably just delete this since we're initializing everything in the child classes but im not totally sure so it stays here for now...
    // Use this for initialization
    protected virtual void Awake ()
    {
    }

    // Update is called once per frame
    void Update ()
    {
		
	}

    //calculates initiative for the character for the turn
    //implemented differently in PCs and NPCs
    public abstract int GetInitiative();

    //coroutine that handles the entire turn of a character
    //implemented differently in PCs and NPCs
    public abstract IEnumerator TakeTurn();

    //Filler action for stuff to be done by the object on new turn
    public abstract void DoAction();
}
