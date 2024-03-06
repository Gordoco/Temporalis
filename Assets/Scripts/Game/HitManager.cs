using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(StatManager))]
public class HitManager : NetworkBehaviour
{
    private StatManager manager;

    private void Start()
    {
        if (isServer)
            manager = GetComponent<StatManager>();
    }

    /// <summary>
    /// Server-Only method which modifies the requested entity's health value
    /// </summary>
    /// <param name="damage"> Value to be subtracted from the health of the owning entity </param>
    /// <returns> True if entity died, false otherwise </returns>
    [Server]
    public virtual bool Hit(double damage)
    {
        Debug.Log(gameObject.name + " has been hit for: " + damage + " damage. Has: " + manager.GetStat(NumericalStats.Health) + " Health");
        manager.ModifyStat(NumericalStats.Health, -damage);
        if (manager.GetStat(NumericalStats.Health) <= 0) return true;
        return false;
    }
}
