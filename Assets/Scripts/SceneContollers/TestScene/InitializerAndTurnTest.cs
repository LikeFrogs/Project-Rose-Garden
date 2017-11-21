using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SceneController for TestScene
/// </summary>
public class InitializerAndTurnTest : CombatSceneController
 {
    /// <summary>
    /// Runs all pre-combat dialogue and then calls the main combat coroutine
    /// </summary>
    /// <param name="party">The playable party</param>
    public override IEnumerator BeginPlay(List<PlayableChar> party)
    {
        //Any before combat dialogue/scene specific story stuff goes here

        StartCoroutine(PlayScene(party));
        yield break;
    }

    /// <summary>
    /// Runs after combat dialogue and loads the next scene
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator EndScene()
    {
        //end of scene dialogue and such
        yield break;
    }

    /// <summary>
    /// Checks to see if the combat's objective has been completed
    /// </summary>
    protected override void CheckObjective()
    {
        //check for objective
    }
}
