using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum NumericalStats
{
    AttackSpeed,
    Health,
    Damage,
    JumpHeight,
    MovementSpeed,
    Range,
    NumberOfStats
}
public class StatManager : NetworkBehaviour
{
    private readonly SyncList<double> stats = new SyncList<double>();
    [SerializeField] private double[] initStats = new double[(int)NumericalStats.NumberOfStats];

    public void Start()
    {
        if (isServer)
        {
            for (int i = 0; i < (int)NumericalStats.NumberOfStats; i++) stats.Add(initStats[i]);
        }
    }

    /// <summary>
    /// Server-Only method which modifies the specified stat of the player
    /// </summary>
    /// <param name="stat"></param>
    /// <param name="value"></param>
    [Server]
    public void ModifyStat(NumericalStats stat, double value)
    {
        if (stat == NumericalStats.NumberOfStats) return;
        stats[(int)stat] += value;
    }

    /// <summary>
    /// Server-Only method which sets the specified stat of the player
    /// </summary>
    /// <param name="stat"></param>
    /// <param name="value"></param>
    [Server]
    public void SetStat(NumericalStats stat, double value)
    {
        if (stat == NumericalStats.NumberOfStats) return;
        stats[(int)stat] = value;
    }

    /// <summary>
    /// Network-Independant method which gets the specified stat of the player
    /// </summary>
    /// <param name="stat"></param>
    /// <returns>The double value of the specified stat</returns>
    public double GetStat(NumericalStats stat)
    {
        if (stat == NumericalStats.NumberOfStats) return Mathf.Infinity;
        return stats[(int)stat];
    }
}
