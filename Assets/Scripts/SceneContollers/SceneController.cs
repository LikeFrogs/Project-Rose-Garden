using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Requires implementing classes to have a coroutine for beginning the scene
/// </summary>
public interface SceneController
{
    /// <summary>
    /// Gives game management control to the SceneController.
    /// Should only be called from GameController
    /// </summary>
    /// <param name="party">The playable party</param>
    void StartScene(List<PlayableChar> party);

    /// <summary>
    /// Starts the scene. Should be called from StartScene
    /// </summary>
    /// <param name="party">The playable party</param>
    IEnumerator BeginPlay(List<PlayableChar> party);
}
