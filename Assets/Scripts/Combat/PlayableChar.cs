using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayableChar : CombatChar
{
    #region Instance data
    protected int strength;
    protected int dexterity;
    protected int intelligence;
    protected bool movePhase;
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
    protected override void Awake ()
    {
        //default testing values
        health = 10;
        maxHealth = 10;
        speed = 5;
        maxSpeed = 5;
        strength = 12;
        dexterity = 12;
        intelligence = 12;
        movePhase = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
        //until we get an object to handle passing the turn order between characters
        //we're just going to start a turn when you press Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine("TakeTurn");
        }

        Vector3 pos = transform.position;
        if (movePhase)
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                pos += Vector3.right;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                //Vector3 test = new Vector3(0, -32);
                pos += Vector3.down;
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                pos += Vector3.left;
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                pos += Vector3.up;
            }
        }

        //transform.position = Vector3.MoveTowards(transform.position, pos, Time.deltaTime * 32);
        transform.position = pos;
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

    public override IEnumerator TakeTurn()
    {
        Debug.Log("Running TakeTurn");
        movePhase = true;

        //run something here to calculate and create a visual of where the player can move this turn (based on speed)

        while (movePhase)
        {
            yield return null;

            if (Input.GetKeyDown(KeyCode.Q))
            {
                //run a method to bring up the action menu here
                //within that method we'll set movePhase to false and bring up the menu to wait for user input
                //if the user selects an action we'll run that and leave movePhase false
                //if they back out we'll set movePhase back to true and return to this while loop and wait for the user to open the action menu again
                //we will remove the move area visual once the player has confirmed their action for that turn

                //for testing/before we get the action menu up and running I'm just going to have pressing y end your turn
                movePhase = false;
            }
        }
    }

    public override void DoAction()
    {
        throw new System.NotImplementedException();
    }

    ////not really ready to use yet
    //private void ActionMenu()
    //{
    //    movePhase = false;
    //    //allow user to navigate the menu and select an action
    //    if (Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        movePhase = true;
    //        return;
    //    }
    //}
}
