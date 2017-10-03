using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializerAndTurnTest : MonoBehaviour
 {
    public GameObject bluePlayer;
    public GameObject redPlayer;

    // Use this for initialization
    void Start ()
    {
        List<CombatChar> charList = new List<CombatChar>
        {
            (CombatChar)GameObject.Instantiate<GameObject>(bluePlayer).GetComponent<PlayableChar>(),
            (CombatChar)GameObject.Instantiate<GameObject>(redPlayer).GetComponent<PlayableChar>()
        };

        StartCoroutine(CombatLoop(charList));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
    public IEnumerator CombatLoop(List<CombatChar> charList)
    {
        //Iterate through charList, call DoAction for each in order of initiative
        for (int i = 0; i < charList.Count; i++)
        {
            ((PlayableChar)charList[i]).DoAction();
            while (!charList[i].FinishedTurn){ yield return null; }
            //restarts the loop at the end of the turn
            if(i == charList.Count - 1)
            {
                i = -1;
            }
        }
    }
}
