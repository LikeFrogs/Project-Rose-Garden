using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Base class for scene controllers in combat scenes
/// </summary>
public abstract class CombatSceneController : MonoBehaviour, SceneController
{
    [SerializeField] Vector3 bottomLeftCorner;
    [SerializeField] Vector3 topRightCorner;

    /// <summary>
    /// Gets the bottom left corner of the play area
    /// </summary>
    public Vector3 BottomLeftCorner { get { return bottomLeftCorner; } }
    /// <summary>
    /// Gets the top right corner of the play area
    /// </summary>
    public Vector3 TopRightCorner { get { return topRightCorner; } }

    /// <summary>
    /// Begins play
    /// </summary>
    /// <param name="party">The playable party</param>
    public abstract IEnumerator BeginPlay(List<PlayableChar> party);
    /// <summary>
    /// Finishes and ends the scene
    /// </summary>
    /// <returns></returns>
    protected abstract IEnumerator EndScene();
    /// <summary>
    /// Checks to see if the combat's objective has been completed
    /// </summary>
    protected abstract void CheckObjective();

    /// <summary>
    /// Calls the coroutine to begin the scene's gameplay
    /// </summary>
    /// <param name="party">The playable party</param>
    public void StartScene(List<PlayableChar> party)
    {
        StartCoroutine(BeginPlay(party));
    }

    /// <summary>
    /// Runs combat scenes
    /// </summary>
    protected IEnumerator PlayScene(List<PlayableChar> party)
    {
        //any dialogue and such before combat goes here






        //it would probably be a good idea to use LINQ statements here rather than a ton of foreach loops
        List<CombatChar> charList = new List<CombatChar>();

        //GameController gameController = Camera.main.GetComponent<GameController>();

        foreach (PlayableChar character in party)
        {
            charList.Add(character);
        }

        charList[0].transform.position = new Vector3(2, 2);
        charList[1].transform.position = new Vector3(0, 0);

        //doing this would essentially remove the player from the scene -- useful for non combat scenes
        //charList[0].gameObject.SetActive(false);

        //adds enemies to charList
        charList.AddRange((from gameObject in GameObject.FindGameObjectsWithTag("Enemy") select gameObject.GetComponent<CombatChar>()).ToList());




        //set up party starting positions here


        //ensures that all characters are active before battle starts
        for (int i = 0; i < charList.Count; i++)
        {
            charList[i].gameObject.SetActive(true);
        }



        //splits charList into 3 groups: those that have finished their turn in a round, those that have yet to go and are of the same type, and those that have yet to go but are not of the same type
        List<CombatChar> finishedList = new List<CombatChar>();
        List<CombatChar> currentTurnBlock = new List<CombatChar>();
        List<CombatChar> nextList = new List<CombatChar>();
        nextList.AddRange(charList);

        //runs combat until the combat's objective is completed
        //could be kill all enemies or a battle specific objective
        bool objectiveComplete = false;
        while (!objectiveComplete)
        {
            //sets up currentTurnBlock
            //this is all of the characters of the same type that are "adjacent" in the turn order
            if (nextList.Count > 0 && nextList[0] is PlayableChar)
            {
                while (nextList.Count > 0 && nextList[0] is PlayableChar)
                {
                    currentTurnBlock.Add(nextList[0]);
                    nextList.RemoveAt(0);
                }

            }
            else if (nextList.Count > 0 && nextList[0] is Enemy)
            {
                while (nextList.Count > 0 && nextList[0] is Enemy)
                {
                    currentTurnBlock.Add(nextList[0]);
                    nextList.RemoveAt(0);
                }
            }
            else
            {
                nextList.AddRange(finishedList);
                finishedList.Clear();
            }

            //loops through currentTurnBlock until all characters from that block have taken their turns
            while (currentTurnBlock.Count > 0)
            {
                int index = 0;
                currentTurnBlock[index].BeginTurn();
                //allows the player to switch between playable characters whose turns are directly in sequence
                while (!currentTurnBlock[index].FinishedTurn)
                {
                    if (Input.GetKeyDown(KeyCode.Tab) && currentTurnBlock.Count > 1 && currentTurnBlock[index] is PlayableChar) //*********************************convert this to GetButtonDown
                    {
                        currentTurnBlock[index].StopAllCoroutines();
                        currentTurnBlock[index].FinishedTurn = true;
                        index++;
                        if (index >= currentTurnBlock.Count) { index = 0; }
                        currentTurnBlock[index].BeginTurn();
                    }
                    yield return null;
                }

                yield return null;

                //when a character finishes its turn it is removed from currentTurnBlock and added to finishedList
                finishedList.Add(currentTurnBlock[index]);
                currentTurnBlock.RemoveAt(index);

                //checks for and removes all destroyed characaters
                for (int i = 0; i < finishedList.Count; i++)
                {
                    if (finishedList[i] == null) { finishedList.RemoveAt(i); }
                }
                for (int i = 0; i < currentTurnBlock.Count; i++)
                {
                    if (currentTurnBlock[i] == null) { currentTurnBlock.RemoveAt(i); }
                }
                for (int i = 0; i < nextList.Count; i++)
                {
                    if (nextList[i] == null) { nextList.RemoveAt(i); }
                }


                //checks for objective completion, special events, etc. here
            }
        }


        //after combat story and EXP stuff
        StartCoroutine("EndScene");
    }
}
