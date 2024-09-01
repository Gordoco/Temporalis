using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple healing item implementation using CustomItemEffect interface
/// </summary>
public class HealthPackItemComponent : BaseItemComponent
{
    public override void CustomItemEffect(StatManager manager)
    {
        base.CustomItemEffect(manager);
        manager.ModifyCurrentHealth(50);
    }
}
