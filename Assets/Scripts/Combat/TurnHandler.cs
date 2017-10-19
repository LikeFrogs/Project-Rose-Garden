using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TurnHandler : ScriptableObject
{
    /// <summary>
    /// Sorts list of combat characters by their "GetIniative" value
    /// </summary>
    /// <param name="charList">List of combat characters</param>
    /// <returns>Sorted list of combat characters</returns>
    public List<CombatChar> SortInitiative(List<CombatChar> charList)
    {
        charList.Sort((x, y) => -1*x.GetInitiative().CompareTo(y.GetInitiative()));

        return charList;
    }

    /// <summary>
    /// Updates scene objects
    /// </summary>
    public void UpdateScene()
    {
        //Do scene stuff here
        //Parameters?
    }

    /// <summary>
    /// Main method for TurnHandler, Handles the beginning of a new turn
    /// </summary>
    /// <param name="charList">List of characters</param>
    public void NextTurn(List<CombatChar> charList)
    {
        //Sort charList by initiative
        SortInitiative(charList);

        //Update scene stuff
        UpdateScene();

        //Iterate through charList, call DoAction for each in order of initiative
        for (int i = 0; i < charList.Count; i++)
        {
            charList[i].BeginTurn();
        }
    }
}