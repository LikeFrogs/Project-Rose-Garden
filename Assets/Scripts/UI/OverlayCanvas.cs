using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverlayCanvas : MonoBehaviour
{
    private static Text text;

    private void Start()
    {
        text = gameObject.GetComponent<Text>();
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

    public static void CombatForecast(CombatChar target, CombatChar attacker, int expectedTargetDamage, int expectedAttackerDamage, int etc)
    {
        text.enabled = true;

        text.text = "Displaying combat forecast";
    }

    public void HideUI()
    {
        text.enabled = false;
    }

    public static void StaticHideUI()
    {
        text.enabled = false;
    }
}
