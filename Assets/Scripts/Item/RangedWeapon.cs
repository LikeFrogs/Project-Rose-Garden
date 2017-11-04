using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RangedWeapon : Item
{
    /// <summary>
    /// The damage of this ranged weapon
    /// </summary>
    public abstract int Damage { get; }
    /// <summary>
    /// The range of this ranged weapon
    /// </summary>
    public abstract int Range { get; }
    /// <summary>
    /// The amount of ammo remaining in this ranged weapon
    /// </summary>
    public abstract int Ammo { get; set; }

    public override abstract Type ItemType { get; }
}
