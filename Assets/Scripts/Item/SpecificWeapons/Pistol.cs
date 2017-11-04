using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pistol : RangedWeapon
{
    private int damage;
    private int range;
    private int ammo;

    /// <summary>
    /// The damage of this ranged weapon
    /// </summary>
    public override int Damage { get { return damage; } }
    /// <summary>
    /// The range of this ranged weapon
    /// </summary>
    public override int Range { get { return range; } }
    /// <summary>
    /// The amount of ammo remaining in this ranged weapon
    /// </summary>
    public override int Ammo
    {
        get { return ammo; }
        set
        {
            ammo = value;
            if(ammo < 0) { ammo = 0; }
        }
    }

    public override Type ItemType { get { return GetType(); } }


    public Pistol(int damage, int range, int ammo)
    {
        this.damage = damage;
        this.range = range;
        this.ammo = ammo;
    }

}
