using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InitializerAndTurnTest : SceneController
 {
    /// <summary>
    /// Runs the combat for this scene
    /// </summary>
    /// <param name="party">The playable party</param>
    protected override IEnumerator PlayScene(List<GameObject> party)
    {
        //start of a list that will be built up to have all turn taking characters in scene
        List<CombatChar> charList = new List<CombatChar>();


        //it would probably be a good idea to use LINQ statements here rather than a ton of foreach loops
        

        //repurpose this once we do player controlled combat set-up
        foreach(GameObject member in party)
        {
            charList.Add(Instantiate(member).GetComponent<PlayableChar>());
        }

        //adds enemies to charList
        charList.AddRange((from gameObject in GameObject.FindGameObjectsWithTag("Enemy") select gameObject.GetComponent<CombatChar>()).ToList());


        //any dialogue and such before combat goes here


        //set up party starting positions here


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
