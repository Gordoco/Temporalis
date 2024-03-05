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

    [System.Serializable]
    public class InitStatsDisplay
    {
        private static int nextStat = 0;
        public NumericalStats stat;
        public double val;
        public InitStatsDisplay()
        {
            stat = (NumericalStats)(nextStat%((int)NumericalStats.NumberOfStats));
            val = 1;
            nextStat++;
        }
    }

    private readonly SyncList<double> stats = new SyncList<double>();
    [SerializeField] private InitStatsDisplay[] initStats = new InitStatsDisplay[(int)NumericalStats.NumberOfStats];


    public void Start()
    {
        if (isServer)
        {
            for (int i = 0; i < (int)NumericalStats.NumberOfStats; i++) stats.Add(initStats[i].val);
        }
        else
        {
            for (int i = 0; i < (int)NumericalStats.NumberOfStats; i++)
            {
                Debug.Log((NumericalStats)i + " = " + stats[i]);
            }
        }
    }

    private void Update()
    {
        if (isServer)
        {
            if (stats[(int)NumericalStats.Health] <= 0)
            {
                NetworkServer.Destroy(gameObject); //Kill Actor in all Contexts
            }
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
        if ((int)stat >= (int)NumericalStats.NumberOfStats || (int)stat < 0) return Mathf.Infinity;
        return stats[(int)stat];
    }
}
