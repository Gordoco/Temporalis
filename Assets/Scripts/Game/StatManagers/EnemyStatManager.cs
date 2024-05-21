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

    private static string ITEM_PATH = "ConcreteItems";
    private GameObject GetRandomWeightedItem()
    {
        GameObject[] itemPrefabs = Resources.LoadAll<GameObject>(ITEM_PATH);
        Debug.Log("NUM ITEMS: " + itemPrefabs.Length);
        int sum = 0;
        foreach (GameObject i in itemPrefabs)
        {
            if (i.GetComponent<BaseItemComponent>())
                sum += i.GetComponent<BaseItemComponent>().GetItemWeight();
        }
        int rand = Random.Range(0, sum);
        int index = -1;
        Debug.Log("SUM: " + sum + "RAND: " + rand);
        sum = 0;
        for (int i = 0; i < itemPrefabs.Length; i++) { sum += itemPrefabs[i].GetComponent<BaseItemComponent>().GetItemWeight(); if (sum > rand) { index = i; break; } }
        return itemPrefabs[index];
    }

    [Server]
    protected override void OnDeath()
    {
        int chance = Random.Range(0, Mathf.Clamp((int)(BASE_ITEM_SPAWN_RATE - ItemWeightMult), 0, BASE_ITEM_SPAWN_RATE));
        Debug.Log("YOU ROLLED: " + chance);
        if (chance == 0)
        {
            Debug.Log("SPAWNED ITEM YAYYYYYYYYYYYYY");
            GameObject item = Instantiate(GetRandomWeightedItem(), transform.position + (transform.up * 10), Quaternion.identity);
            NetworkServer.Spawn(item);
        }
        base.OnDeath();
    }

    [Server]
    public int GetEnemySpawnWeight() { return EnemySpawnWeight; }
}
