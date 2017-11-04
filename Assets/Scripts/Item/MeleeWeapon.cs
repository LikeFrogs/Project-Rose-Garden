using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MeleeWeapon : Item
{
    /// <summary>
    /// The damage of this melee weapon
    /// </summary>
    public abstract int Damage { get; }

    public override abstract Type ItemType { get; }
}
