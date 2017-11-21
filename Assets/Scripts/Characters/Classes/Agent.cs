//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Agent : PlayableChar
//{
//    #region stats
//    /// <summary>
//    /// Gets character's current health
//    /// </summary>
//    public override int Health
//    {
//        get { return health; }
//    }
//    /// <summary>
//    /// Gets character's maximum health
//    /// </summary>
//    public override int MaxHealth
//    {
//        get { return maxHealth; }
//    }
//    /// <summary>
//    /// Character's current MP
//    /// </summary>
//    public override int MutationPoints
//    {
//        get { return mutationPoints; }
//    }
//    /// <summary>
//    /// Character's maximum MP
//    /// </summary>
//    public override int MaxMutationPoints
//    {
//        get { return MaxMutationPoints; }
//    }
//    /// <summary>
//    /// Gets character's movement speed
//    /// </summary>
//    public override int Speed
//    {
//        get { return speed; }
//    }
//    /// <summary>
//    /// Gets character's max speed
//    /// </summary>
//    public override int MaxSpeed
//    {
//        get { return maxSpeed; }
//    }
//    /// <summary>
//    /// Gets character's attack. Used for physical damage
//    /// </summary>
//    public override int Attack
//    {
//        get { return attack; }
//    }
//    /// <summary>
//    /// Gets character's magic attack. Used for magic damage
//    /// </summary>
//    public override int MagicAttack
//    {
//        get { return magicAttack; }
//    }
//    /// <summary>
//    /// Gets character's defense. Used to defend against physical attacks
//    /// </summary>
//    public override int Defense
//    {
//        get { return defense; }
//    }
//    /// <summary>
//    /// Gets character's resistance. Used to defend against magical attacks
//    /// </summary>
//    public override int Resistance
//    {
//        get { return resistance; }
//    }

//    /// <summary>
//    /// Gets character's attack range
//    /// </summary>
//    public override int AttackRange
//    {
//        get { return 1; }
//    }

//    /// <summary>
//    /// Gets character's abilities
//    /// </summary>
//    public List<string> AbilityList
//    {
//        get { return abilityList; }
//    }
//    /// <summary>
//    /// Gets character's spells
//    /// </summary>
//    public List<string> SpellList
//    {
//        get { return spellList; }
//    }
//    #endregion

//    //not finished
//    #region Equipment
//    #endregion












//    //NOTE: make this specific to Agent at some point
//    /// <summary>
//    /// Determines which actions the character can take from it's current position
//    /// </summary>
//    /// <returns>A list of strings representing all possible actions</returns>
//    protected override List<string> GetActions()
//    {
//        List<string> actionList = new List<string>();

//        //checks to see if there are enemies in adjacent squares and adds "Melee" to the action list if so
//        List<Vector3> adjacentSquares = new List<Vector3>
//        {
//            new Vector3(transform.position.x - 1, transform.position.y),
//            new Vector3(transform.position.x + 1, transform.position.y),
//            new Vector3(transform.position.x, transform.position.y + 1),
//            new Vector3(transform.position.x, transform.position.y - 1)
//        };
//        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
//        foreach (GameObject enemy in enemies)
//        {
//            if (adjacentSquares.Contains(enemy.transform.position))
//            {
//                actionList.Add("Melee");
//                break;
//            }
//        }

//        //checks to see if there are enemies within line of sight and max range
//        foreach (GameObject enemy in enemies)
//        {
//            if (!Physics2D.Linecast(transform.position, enemy.transform.position) && (System.Math.Abs(transform.position.x - enemy.transform.position.x) + System.Math.Abs(transform.position.y - enemy.transform.position.y) <= attackRange) && !adjacentSquares.Contains(enemy.transform.position))
//            {
//                actionList.Add("Ranged");
//                break;
//            }
//        }

//        //checks if this character knows any abiliities and adds "Ability" if so
//        if (abilityList.Count > 0)
//        {
//            actionList.Add("Ability");
//        }

//        //checks if this character knows any spells and adds "Spell" if so
//        if (spellList.Count > 0)
//        {
//            actionList.Add("Spell");
//        }

//        //"End" vs "Defend" ??
//        actionList.Add("End");

//        return actionList;
//    }



















//    //implement this
//    /// <summary>
//    /// Levels up this character
//    /// </summary>
//    protected override void LevelUp()
//    {
        
//    }
//}
