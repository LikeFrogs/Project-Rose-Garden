using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//used to notify other characters of a move made by a certain character
public delegate void MoveEventHandler(List<Vector3> path, CombatChar character);

/// <summary>
/// Parent class for all characters that participate in combat, playable or otherwise
/// </summary>
public abstract class CombatChar : MonoBehaviour
{
    public event MoveEventHandler OnMove;

    /// <summary>
    /// Get's character's current level
    /// </summary>
    public abstract int Level { get; }

    /// <summary>
    /// Character's current health
    /// </summary>
    public abstract int Health { get; }
    /// <summary>
    /// Character's maximum health
    /// </summary>
    public abstract int MaxHealth { get; }
    /// <summary>
    /// Character's current MP
    /// </summary>
    public abstract int MutationPoints { get; }
    /// <summary>
    /// Character's maximum MP
    /// </summary>
    public abstract int MaxMutationPoints { get; }
    /// <summary>
    /// Character's movement speed
    /// </summary>
    public abstract int Speed { get; }
    /// <summary>
    /// Character's max speed
    /// </summary>
    public abstract int MaxSpeed { get; }
    /// <summary>
    /// Character's strength. Used for physical damage
    /// </summary>
    public abstract int Attack { get; }
    /// <summary>
    /// Character's dexterity. Used for speed and initiative
    /// </summary>
    public abstract int MagicAttack { get; }
    /// <summary>
    /// Character's defense. Used to defend against physical attacks
    /// </summary>
    public abstract int Defense { get; }
    /// <summary>
    /// Character's resistance. Used to defend against magical attacks
    /// </summary>
    public abstract int Resistance { get; }
    /// <summary>
    /// Character's initiative. Used to determine turn order
    /// </summary>
    public abstract int Initiative { get; }
    /// <summary>
    /// Gets character's attack range
    /// </summary>
    public abstract int AttackRange { get; }

    /// <summary>
    /// This bool will be set to true at the end of a character's turn.
    /// This will be used to tell the turn handler to move on to the next turn.
    /// </summary>
    public abstract bool FinishedTurn { get; set; }
    /// <summary>
    /// Gets true when taking damage and false otherwise
    /// </summary>
    public abstract bool TakingDamage { get; }

    /// <summary>
    /// Starts the coroutine that handles a character's turn
    /// </summary>
    public abstract void BeginTurn();

    /// <summary>
    /// Starts the coroutine that handles a character taking damage
    /// </summary>
    /// <param name="damage">The amount of damage to take</param>
    public abstract void BeginTakeDamage(int damage);

    /// <summary>
    /// Notifies any subscribers of OnMove that this character has moved
    /// </summary>
    /// <param name="path">The path the character took</param>
    protected void NotifyOfMove(List<Vector3> path)
    {
        if(OnMove != null)
        {
            //gives the subscriber the path taken and a reference to this character
            OnMove(path, this);
        }
    }
}
