using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
        

        //repurpose this once we do player controlled combat set-up
        foreach(GameObject member in party)
        {
            charList.Add(Instantiate(member).GetComponent<PlayableChar>());
        }

        //adds enemies to charList
        GameObject[] enemyList = GameObject.FindGameObjectsWithTag("Enemy");
        foreach(GameObject enemy in enemyList)
        {
            //uncomment this once enemies are real
            //charList.Add(enemy.GetComponent<CombatChar>());
        }


        //any dialogue and such before combat goes here


        //set up party starting positions here

        //runs combat until the combat's objective is completed
        //could be kill all enemies or a battle specific objective
        bool objectiveComplete = false;
        while (!objectiveComplete)
        {
            Debug.Log(charList.ToString());
            //for(int i = 0; i < charList.Count; i++)
            foreach(CombatChar character in charList)
            {
                character.BeginTurn();
                //waits until the character's turn ends to pregress to the next object in the list
                while (!character.FinishedTurn) { yield return null; }

                //checks for death, objective completion, special events, etc. here
                //if combat ends here modify i and objectiveComplete
            }
        }


        //after combat story and EXP stuff
    }
}
