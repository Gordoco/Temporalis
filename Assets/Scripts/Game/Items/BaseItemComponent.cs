using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseItemComponent : MonoBehaviour
{
    public string ItemName;
    public NumericalStats[] stats;
    public double[] values;

    public BaseItem CreateCopy()
    {
        BaseItem item = new BaseItem();
        item.ItemName = ItemName;
        item.stats = (NumericalStats[])stats.Clone();
        item.values = (double[])values.Clone();
        return item;
    }
}
