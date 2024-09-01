using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

/// <summary>
/// Lightweight object for holding item information
/// </summary>
public class BaseItem
{
    public string ItemName;
    public NumericalStats[] stats;
    public double[] values;
    public bool percent;

    public BaseItem()
    {
    }
}
