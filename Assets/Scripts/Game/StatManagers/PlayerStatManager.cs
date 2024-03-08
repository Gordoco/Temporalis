using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatManager : StatManager
{
    protected override void OnDeath()
    {
        gameObject.transform.parent.GetComponent<PlayerObjectController>().Die();
        base.OnDeath();
    }
}
