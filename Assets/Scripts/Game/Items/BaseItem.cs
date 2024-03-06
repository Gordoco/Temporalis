using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Component class which can be transfered to a StatManager object to impact entity state
/// </summary>
[RequireComponent(typeof(StatManager))]
abstract class BaseItem : NetworkBehaviour
{
    private StatManager Manager;
    private void Start()
    {
        Manager = GetComponent<StatManager>();
    }
}
