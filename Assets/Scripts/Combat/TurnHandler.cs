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
        charList.Sort((x, y) => x.GetInitiative().CompareTo(y.GetInitiative()));

        return charList;
    }

    public void NextTurn(List<CombatChar> charList)
    {
        SortInitiative(charList);

    }
}