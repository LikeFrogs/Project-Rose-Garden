using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface ICombatChar
{
    int Health
    {
        get;
        set;
    }
    int Speed
    {
        get;
    }

    int GetInitiative();
}
