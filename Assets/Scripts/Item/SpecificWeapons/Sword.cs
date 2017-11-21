using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MeleeWeapon
{
    private int damage;

    /// <summary>
    /// The damage of this sword
    /// </summary>
    public override int Damage { get { return damage; } }

    public override Type ItemType { get { return GetType(); } }

    public Sword(int damage)
    {
        this.damage = damage;
    }
}
