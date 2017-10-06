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
    
    bool isMoving;
    List<Vector3> moveRange;
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
        isMoving = false;
        finishedTurn = false;

        moveRange = new List<Vector3>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        //only move/look for move input during the move phase of a player's turn
        if (movePhase)
        {
            //when the player is not actively moving looks for input in x and y directions and calls the move coroutine
            Vector2 input;
            if (!isMoving)
            {
                input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

                ////the following code disables diagonal movement
                //if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                //{
                //    input.y = 0;
                //}
                //else
                //{
                //    input.x = 0;
                //}

                if (input != Vector2.zero)
                {
                    StartCoroutine(Move(input));
                }
            }
        }
    }

    //Smoothly moves the player from their current position to a position one tile in the direction of input
    private IEnumerator Move(Vector2 input)
    {
        isMoving = true; //while running this routine no new input is accepted
        Vector3 startPos = transform.position;
        float t = 0; //time
        //this vector equals the player's original position + 1 in the direction they are moving
        Vector3 endPos = new Vector3(startPos.x + System.Math.Sign(input.x), startPos.y + System.Math.Sign(input.y), startPos.z);

        int speed = 5;
        if(input.x != 0  && input.y != 0)//diagonal movement needs to take longer
        {
            speed = 4;
        }

        //will eventually prevent player from moving into unreachable squares
        //if (!moveRange.Contains(input))
        //{
        //    t = 1f;
        //}

        //smoothly moves the player across the distance with lerp
        while(t < 1f)
        {
            t += Time.deltaTime * speed;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        //when done moving allow more input to be received
        isMoving = false;
        yield return null;
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

    /// <summary>
    /// Handles the entire turn for playable characters
    /// </summary>
    /// <returns></returns>
    public override IEnumerator TakeTurn()
    {
        //the finishedTurn variable tells the turn handler to wait until TakeTurn() completes before starting the next turn
        finishedTurn = false;

        //this for loop runs the inner functions on every square that would be within a character's unimpeded movement range
        //the calculations in the middle determine if the square can actually be reached and if so adds it to moveRange
        for (int x = (int)transform.position.x - speed; x <= (int)transform.position.x + speed; x++)
        {
            for (int y = (int)transform.position.y - (speed - System.Math.Abs((int)transform.position.x - x)); System.Math.Abs((int)transform.position.x - x) + System.Math.Abs((int)transform.position.y - y) <= speed; y++)
            {
                Vector3 test = new Vector3(x, y);
                if (Node.CheckSquare(transform.position, test, speed))
                {
                    moveRange.Add(test);
                }
            }
        }

        //turns on player movement
        movePhase = true;

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

                //for testing/before we get the action menu up and running I'm just going to have pressing q end your turn
                movePhase = false;
            }
        }
        finishedTurn = true;
    }

    //starts turn
    public override void DoAction()
    {
        StartCoroutine("TakeTurn");
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
