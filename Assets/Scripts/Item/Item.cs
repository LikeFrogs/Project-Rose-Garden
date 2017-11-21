using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Item
{
    /// <summary>
    /// Gets the specific type of this item
    /// </summary>
    public abstract Type ItemType { get; }
}
