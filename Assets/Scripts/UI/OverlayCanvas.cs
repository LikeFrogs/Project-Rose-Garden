using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverlayCanvas : MonoBehaviour
{
    [SerializeField] private Text text;

    private void Start()
    {
        DontDestroyOnLoad(this.transform);
    }

    public void InspectCharacter(CombatChar character)
    {
        text.enabled = true;

        text.text = "Health: " + character.Health + "\n" +
               "Speed: " + character.Speed + "\n" +
               "Attack: " + character.Attack + "\n" +
               "Magic Attack: " + character.MagicAttack + "\n" +
               "Defense: " + character.Defense + "\n" +
               "Resistance: " + character.Resistance + "\n" +
               "Initiative: " + character.Initiative + "\n" +
               "Attack Range: " + character.AttackRange + "\n" +
               "Level: " + character.Level;
    }

    public void HideUI()
    {
        text.enabled = false;
    }
}
