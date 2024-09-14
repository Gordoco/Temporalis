using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Stat enum
/// </summary>
public enum NumericalStats
{
    AttackSpeed,
    SecondaryCooldown,
    Ability1Cooldown,
    Ability2Cooldown,
    Ability3Cooldown,
    Ability4Cooldown,
    Health,
    PrimaryDamage,
    SecondaryDamage,
    Ability1Damage,
    Ability2Damage,
    Ability3Damage,
    Ability4Damage,
    JumpHeight,
    MovementSpeed,
    Range,
    HealthRegenAmount,
    HealthRegenSpeed,
    NumberOfStats
}

public abstract class StatManager : NetworkBehaviour
{
    /// <summary>
    /// Class for inspector assigning of elements.
    /// Also to be utilized for "Base Stat" evaluations
    /// </summary>
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

    // Network variables
    private readonly SyncList<double> stats = new SyncList<double>();
    protected readonly SyncList<BaseItem> items = new SyncList<BaseItem>();

    [SyncVar] private bool ShouldShowHP = false;
    [SyncVar] private bool CCImmune = false;

    /// <summary>
    /// double value representing current entity health
    /// </summary>
    [SyncVar] private double Health;

    /// <summary>
    /// double value representing temporary entity shields
    /// </summary>
    [SyncVar] private double Shield;

    [SyncVar] public bool Initialized = false;

    // Editor value
    [SerializeField] private InitStatsDisplay[] initStats = new InitStatsDisplay[(int)NumericalStats.NumberOfStats];

    [Server]
    public void ToggleCCImmune(bool b)
    {
        CCImmune = b;
    }

    public bool GetCCImmune() { return CCImmune; }

    /// <summary>
    /// Copies base stats into network replicated stat list, flags the StatManager as Initialized when complete.
    /// </summary>
    private void Start()
    {
        if (isServer)
        {
            for (int i = 0; i < (int)NumericalStats.NumberOfStats; i++) stats.Add(initStats[i].val);
            Health = GetStat(NumericalStats.Health);
            Debug.Log(gameObject.name + " IS INITIALIZED");
            Initialized = true;
            StartCoroutine(HealthRegen());
        }
        else if (gameObject.tag == "Player" && isClient)
        {
            if (stats.Count < (int)NumericalStats.NumberOfStats) return;
            for (int i = 0; i < stats.Count; i++)
            {
                Debug.Log((NumericalStats)i + " = " + stats[i]);
            }
        }
    }

    /// <summary>
    /// Ensures all stats have been initially loaded
    /// </summary>
    /// <returns></returns>
    public bool CheckReady()
    {
        return stats.Count == (int)NumericalStats.NumberOfStats;
    }

    /// <summary>
    /// Network-Independant method which gets the specified stat of the player
    /// </summary>
    /// <param name="stat"></param>
    /// <returns>The double value of the specified stat</returns>
    public double GetStat(NumericalStats stat)
    {
        if ((int)stat >= (int)NumericalStats.NumberOfStats || (int)stat < 0) return Mathf.Infinity;
        return GetCombinedValueFromItems(stat);
    }

    Coroutine resetCoroutine;
    /// <summary>
    /// Adds temporary shields for a specified number of seconds. Subsequent calls will overwrite previous shields.
    /// </summary>
    /// <param name="val"></param>
    /// <param name="time"></param>
    [Server]
    public void AddShield(double val, float time)
    {
        if (resetCoroutine != null) StopCoroutine(resetCoroutine);
        Shield = val;
        resetCoroutine = StartCoroutine(ResetShields(time));
    }

    public double GetShield() { return Shield; }

    private IEnumerator ResetShields(float time)
    {
        yield return new WaitForSeconds(time);
        Shield = 0;
    }
 
    /// <summary>
    /// Will be true after recently having its health stat modified
    /// </summary>
    /// <returns></returns>
    public bool CheckIfShouldShowHP()
    {
        return ShouldShowHP;
    }

    /// <summary>
    /// Method for applying health changes outside the damage system (ie. Healing)
    /// </summary>
    /// <param name="value">Amount current health value should be changed by</param>
    [Server]
    public void ModifyCurrentHealth(double value)
    {
        Health = Mathf.Clamp((float)(value + Health), 0, (float)GetStat(NumericalStats.Health));
    }

    /// <summary>
    /// Simple server-side check of the health stat for evaluating a death
    /// </summary>
    private void Update()
    {
        if (isServer)
        {
            if (Health <= 0)
            {
                OnDeath();
            }
        }
    }

    //TODO: More elegant solution than "Destroy"
    /// <summary>
    /// Server handler for death
    /// </summary>
    [Server]
    protected virtual void OnDeath()
    {
        NetworkServer.Destroy(gameObject); //Kill Actor in all Contexts
        Destroy(gameObject);
    }

    /// <summary>
    /// Gets the Current Entity Health
    /// </summary>
    /// <returns></returns>
    public double GetHealth() { return Health; }


    private Coroutine showHPRoutine; // Coroutine identifier to ensure full 2 second window after each damage event
    /// <summary>
    /// Server-Only Method for applying damage to the entity
    /// </summary>
    /// <returns></returns>
    [Server]
    public void DealDamage(double Damage)
    {
        //Debug.Log("DAMAGED");
        Shield -= Damage;
        if (Shield >= 0) return;
        else Damage = -1 * Shield;
        Health -= Damage;
        ShouldShowHP = true;
        if (showHPRoutine != null) StopCoroutine(showHPRoutine);
        showHPRoutine = StartCoroutine(ShowHPReset());
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
    /// Server method for adding an item to the manager
    /// </summary>
    /// <param name="item"></param>
    [Server]
    public void AddItem(BaseItemComponent item)
    {
        if (item.stats.Length > 0 && item.stats.Length == item.values.Length)
        {
            BaseItem newItem = item.CreateCopy();
            item.CustomItemEffect(this);
            if (GetComponent<SoundManager>())
            {
                if (item.GetItemPickupSound())
                {
                    GetComponent<SoundManager>().PlaySoundEffect(item.GetItemPickupSound(), 2.5f);
                }
            }
            items.Add(newItem);
            AddItemChild(newItem);
        }
        else Debug.Log("[ERROR - StatManager.cs: Bad Item Insertion in " + gameObject.name + "]");
    }

    /// <summary>
    /// By default there is no generic implementation for adding an item at runtime, see PlayerStatManager.cs
    /// </summary>
    /// <param name="item"></param>
    [Server]
    protected virtual void AddItemChild(BaseItem item) {}

    /// <summary>
    /// Calculates item stat values
    /// </summary>
    /// <param name="stat"></param>
    /// <returns></returns>
    private double GetCombinedValueFromItems(NumericalStats stat)
    {
        if ((int)stat < 0 || (int)stat >= (int)NumericalStats.NumberOfStats) return 0;
        //Debug.Log("STAT: " + (int)stat);
        double val = stats[(int)stat];
        foreach (BaseItem item in items)
        {
            for (int i = 0; i < item.stats.Length; i++)
            {
                if (item.stats[i] == stat)
                {
                    if (item.percent)
                    {
                        if (val > 0) val *= item.values[i];
                    }
                    else val += item.values[i];
                }
            }
        }
        return val;
    }

    /// <summary>
    /// Coroutine for passive health regen
    /// </summary>
    /// <returns></returns>
    private IEnumerator HealthRegen()
    {
        yield return new WaitForSeconds(0.5f);
        while (true)
        {
            yield return new WaitForSeconds((float)GetStat(NumericalStats.HealthRegenSpeed));
            Health = Mathf.Clamp((float)Health + (float)GetStat(NumericalStats.HealthRegenAmount), 0, (float)GetStat(NumericalStats.Health));
        }
    }

    /// <summary>
    /// Keeps track of the recency of the statManager's HP's modification
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowHPReset()
    {
        yield return new WaitForSeconds(2);
        ShouldShowHP = false;
    }
}
