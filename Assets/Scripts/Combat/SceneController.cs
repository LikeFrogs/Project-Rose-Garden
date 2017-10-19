using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SceneController : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    
    //gives management control to the CombatSceneManager
    //should only be called from GameController
    public void BeginPlay(List<GameObject> party)
    {
        StartCoroutine(PlayScene(party));
    }

    protected abstract IEnumerator PlayScene(List<GameObject> party);
}
