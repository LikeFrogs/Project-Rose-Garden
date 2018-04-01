using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for scene controllers in non combat scenes
/// </summary>
public class NonCombatSceneController : MonoBehaviour, SceneController
{
    //// Use this for initialization
    //void Start () {
    //}

    //// Update is called once per frame
    //void Update () {
    //}

    /// <summary>
    /// Calls the coroutine to begin the scene's gameplay
    /// </summary>
    /// <param name="party">The playable party</param>
    public void StartScene(List<PlayerCharacter> party)
    {
        StartCoroutine(BeginPlay(party));
    }

    /// <summary>
    /// Begins the scene
    /// </summary>
    /// <param name="party">The playable party</param>
    public IEnumerator BeginPlay(List<PlayerCharacter> party)
    {
        yield break;
    }
}
