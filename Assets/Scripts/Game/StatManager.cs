using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum NumericalStats
{
    AttackSpeed,
    SecondaryCooldown,
    Ability1Cooldown,
    Ability2Cooldown,
    Ability3Cooldown,
    Ability4Cooldown,
    Health,
    Damage,
    JumpHeight,
    MovementSpeed,
    Range,
    HealthRegenAmount,
    HealthRegenSpeed,
    NumberOfStats
}

public class StatManager : NetworkBehaviour
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

    private readonly SyncList<double> stats = new SyncList<double>();
    private readonly SyncList<BaseItem> items = new SyncList<BaseItem>();
    [SerializeField] private InitStatsDisplay[] initStats = new InitStatsDisplay[(int)NumericalStats.NumberOfStats];

    [SyncVar] private double Health;


    public void Start()
    {
        if (isServer)
        {
            for (int i = 0; i < (int)NumericalStats.NumberOfStats; i++) stats.Add(initStats[i].val);
            Health = GetStat(NumericalStats.Health);
            StartCoroutine(HealthRegen());
        }
        else if (gameObject.tag == "Player" && isClient)
        {
            for (int i = 0; i < stats.Count; i++)
            {
                Debug.Log((NumericalStats)i + " = " + stats[i]);
            }
        }
    }

    private IEnumerator HealthRegen()
    {
        while (true)
        {
            yield return new WaitForSeconds((float)GetStat(NumericalStats.HealthRegenSpeed));
            Health = Mathf.Clamp((float)Health + (float)GetStat(NumericalStats.HealthRegenAmount), 0, (float)GetStat(NumericalStats.Health));
        }
    }

        private void Update()
    {
        if (isServer)
        {
            if (Health <= 0)
            {
                if (gameObject.tag == "Player")
                {
                    gameObject.transform.parent.GetComponent<PlayerObjectController>().Die();
                }
                else
                {
                    int chance = Random.Range(0, 20);
                    if (chance == 0)
                    {
                        GameObject ItemList = GameObject.FindGameObjectWithTag("ItemList");
                        GameObject itemType = ItemList.GetComponent<WeightedItemList>().GetRandomItemPrefab();
                        GameObject item = Instantiate(itemType, transform.position, Quaternion.identity);
                        NetworkServer.Spawn(item);
                    }
                }
                
                NetworkServer.Destroy(gameObject); //Kill Actor in all Contexts
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Gets the Current Entity Health
    /// </summary>
    /// <returns></returns>
    public double GetHealth() { return Health; }

    /// <summary>
    /// Server-Only Method for applying damage to the entity
    /// </summary>
    /// <returns></returns>
    [Server]
    public void DealDamage(double Damage)
    {
        Health -= Damage;
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

    [Server]
    public void AddItem(BaseItemComponent item)
    {
        if (item.stats.Length > 0 && item.stats.Length == item.values.Length)
        {
            BaseItem newItem = item.CreateCopy();
            items.Add(newItem);
        }
        else Debug.Log("ERROR: Bad Item Insertion in " + gameObject.name);
    }

    private double GetCombinedValueFromItems(NumericalStats stat)
    {
        double val = 0;
        foreach (BaseItem item in items)
        {
            for (int i = 0; i < item.stats.Length; i++)
            {
                if (item.stats[i] == stat) val += item.values[i];
            }
        }
        return val;
    }

    /// <summary>
    /// Network-Independant method which gets the specified stat of the player
    /// </summary>
    /// <param name="stat"></param>
    /// <returns>The double value of the specified stat</returns>
    public double GetStat(NumericalStats stat)
    {
        if ((int)stat >= (int)NumericalStats.NumberOfStats || (int)stat < 0) return Mathf.Infinity;
        return stats[(int)stat] + GetCombinedValueFromItems(stat);
    }
}
