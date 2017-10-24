using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SceneController : MonoBehaviour
{
    //protected List<CombatChar> charList = new List<CombatChar>();

    // Use this for initialization
    //void Start()
    //{

    //}

    // Update is called once per frame
    //void Update()
    //{

    //}


    //public void AddToCharList(List<GameObject> party)
    //{
    //    foreach(GameObject member in party)
    //    {
    //        charList.Add(member.GetComponent<PlayableChar>());
    //    }
    //}
    
    //gives management control to the CombatSceneManager
    //should only be called from GameController
    public void BeginPlay(List<PlayableChar> party)
    {
        StartCoroutine(PlayScene(party));
    }

    protected abstract IEnumerator PlayScene(List<PlayableChar> party);
}
