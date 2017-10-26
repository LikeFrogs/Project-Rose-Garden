using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon
{
    private int damage;
    private int range;

    public int Damage
    {
        get { return damage; }
    }
    public int Range
    {
        get { return range; }
    }

    
    public Weapon(int damage, int range)
    {
        this.damage = damage;
        this.range = range;
    }
}
