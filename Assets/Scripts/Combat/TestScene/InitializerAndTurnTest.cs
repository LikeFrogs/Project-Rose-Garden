using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InitializerAndTurnTest : SceneController
 {
    /// <summary>
    /// Runs the combat for this scene
    /// </summary>
    protected override IEnumerator PlayScene(List<PlayableChar> party)
    {
        //it would probably be a good idea to use LINQ statements here rather than a ton of foreach loops
        List<CombatChar> charList = new List<CombatChar>();

        //GameController gameController = Camera.main.GetComponent<GameController>();

        foreach(PlayableChar character in party)
        {
            charList.Add(character);
        }

        charList[0].transform.position = new Vector3(2, 2);
        charList[1].transform.position = new Vector3(0, 0);

        //doing this would essentially remove the player from the scene -- useful for non combat scenes
        //charList[0].gameObject.SetActive(false);

        //adds enemies to charList
        charList.AddRange((from gameObject in GameObject.FindGameObjectsWithTag("Enemy") select gameObject.GetComponent<CombatChar>()).ToList());


        //any dialogue and such before combat goes here


        //set up party starting positions here



        for(int i = 0; i < charList.Count; i++)
        {
            charList[i].gameObject.SetActive(true);
        }



        //runs combat until the combat's objective is completed
        //could be kill all enemies or a battle specific objective
        bool objectiveComplete = false;
        while (!objectiveComplete)
        {
            for(int i = 0; i < charList.Count; i++)
            {
                charList[i].BeginTurn();
                //waits until the character's turn ends to pregress to the next object in the list
                while (!charList[i].FinishedTurn) { yield return null; }

                yield return null;

                for (int j = 0; j < charList.Count; j++)
                {
                    if(charList[j] == null){ charList.RemoveAt(j); }
                }
                //checks for death, objective completion, special events, etc. here
                //if combat ends here modify i and objectiveComplete
            }
        }


        //after combat story and EXP stuff
    }
}
