using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : CombatSceneController
{
    protected override void CheckObjective()
    {
        if(enemies.Count == 0)
        {
            state = CombatSceneState.ClosingDialogue;

            dialogueSprite.transform.parent.gameObject.SetActive(true);
            dialogueText.text = "YOU WIN!";
        }
    }
}
