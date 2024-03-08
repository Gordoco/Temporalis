using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseItemComponent : MonoBehaviour
{
    public string ItemName;
    public NumericalStats[] stats;
    public double[] values;
    public bool percent;

    public BaseItem CreateCopy()
    {
        BaseItem item = new BaseItem();
        item.ItemName = ItemName;
        item.stats = (NumericalStats[])stats.Clone();
        item.values = (double[])values.Clone();
        item.percent = percent;
        return item;
    }

    /// <summary>
    /// Server-Only Event (Un-enforced) to add custom functionality to custom items
    /// </summary>
    /// <param name="manager">Stat manager this item is added to</param>
    public virtual void CustomItemEffect(StatManager manager) { }
}
