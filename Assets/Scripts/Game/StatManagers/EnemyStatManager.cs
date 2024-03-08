using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemyStatManager : StatManager
{
    private const int BASE_ITEM_SPAWN_RATE = 10;

    /// <summary>
    /// Weight Multiplier for Special Enemies. Any value over BASE_ITEM_SPAWN_RATE will result in guarunteed item spawns
    /// </summary>
    [SerializeField] private float ItemWeightMult = 1;

    /// <summary>
    /// Weight of the enemy type when its spawned, allows fewer, stronger enemies to be spawned.
    /// </summary>
    [SerializeField] private int EnemySpawnWeight = 1;

    public static EnemyStatManager operator >(EnemyStatManager a, EnemyStatManager b)
    {
        return a.GetEnemySpawnWeight() > b.GetEnemySpawnWeight() ? a : b;
    }
    public static EnemyStatManager operator <(EnemyStatManager a, EnemyStatManager b)
    {
        return a.GetEnemySpawnWeight() < b.GetEnemySpawnWeight() ? a : b;
    }

    [Server]
    protected override void OnDeath()
    {
        int chance = Random.Range(0, Mathf.Clamp((int)(BASE_ITEM_SPAWN_RATE - ItemWeightMult), 0, BASE_ITEM_SPAWN_RATE));
        Debug.Log("YOU ROLLED: " + chance);
        if (chance == 0)
        {
            Debug.Log("SPAWNED ITEM YAYYYYYYYYYYYYY");
            GameObject ItemList = GameObject.FindGameObjectWithTag("ItemList");
            GameObject itemType = ItemList.GetComponent<WeightedItemList>().GetRandomItemPrefab();
            GameObject item = Instantiate(itemType, transform.position + (transform.up * 10), Quaternion.identity);
            NetworkServer.Spawn(item);
        }
        base.OnDeath();
    }

    [Server]
    public int GetEnemySpawnWeight() { return EnemySpawnWeight; }
}
