using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class SimpleInfiniteEnemySpawner : NetworkBehaviour
{
    [SerializeField] private double DifficultyScale = 0;
    [SerializeField] private Vector3[] SpawnPoints;
    [SerializeField] private float EnemySpawnInterval = 120;
    [SerializeField] private int BaseNumEnemies = 3;
    [SerializeField] private GameObject[] EnemyTypes;

    private double difficulty;

    private void Start()
    {
        if (!isServer) return;
        difficulty = DifficultyScale;
        StartCoroutine(ConstantEnemySpawner());
    }

    private IEnumerator ConstantEnemySpawner()
    {
        while (SceneManager.GetActiveScene().name == "SampleScene")
        {
            yield return new WaitForSeconds(Random.Range(EnemySpawnInterval - 5, EnemySpawnInterval + 5));
            int i = 0;
            while(i < (BaseNumEnemies + (int)(difficulty/30)))
            {
                GameObject randEnemy = GetRandomEnemyPrefab();
                int randSpawn = Random.Range(0, SpawnPoints.Length);
                GameObject newEnemy = Instantiate(randEnemy, SpawnPoints[randSpawn], Quaternion.identity);
                EnemyStatManager statManager = newEnemy.GetComponent<EnemyStatManager>();
                if (statManager == null)
                {
                    Destroy(newEnemy);
                    break;
                }
                i++;
                NetworkServer.Spawn(newEnemy);
            }
        }
    }

    public GameObject GetRandomEnemyPrefab()
    {
        if (EnemyTypes == null || EnemyTypes.Length == 0) return null;
        int sum = 0;
        for (int i = 0; i < EnemyTypes.Length; i++) sum += EnemyTypes[i].GetComponent<EnemyStatManager>().GetEnemySpawnWeight();
        int rand = Random.Range(0, sum);
        int index = -1;
        sum = 0;
        for (int i = 0; i < EnemyTypes.Length; i++) { sum += EnemyTypes[i].GetComponent<EnemyStatManager>().GetEnemySpawnWeight(); if (sum > rand) { index = i; break; } }
        return EnemyTypes[index];
    }

    private void Update()
    {
        if (!isServer) return;
        difficulty += Time.deltaTime;
    }
}
