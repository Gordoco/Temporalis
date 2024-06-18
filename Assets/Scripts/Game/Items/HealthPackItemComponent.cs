using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPackItemComponent : BaseItemComponent
{
    public override void CustomItemEffect(StatManager manager)
    {
        base.CustomItemEffect(manager);
        manager.ModifyCurrentHealth(50);
    }
}
