using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class SimpleInfiniteEnemySpawner : NetworkBehaviour
{
    [SerializeField] private double DifficultyScale = 0;
    [SerializeField] private Vector3[] SpawnPoints;
    [SerializeField] private float EnemySpawnInterval = 5;
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
            yield return new WaitForSeconds(EnemySpawnInterval);
            for (int i = 0; i < (BaseNumEnemies + (int)(difficulty/5)); i++)
            {
                int randEnemy = Random.Range(0, EnemyTypes.Length);
                int randSpawn = Random.Range(0, SpawnPoints.Length);
                GameObject newEnemy = Instantiate(EnemyTypes[randEnemy], SpawnPoints[randSpawn], Quaternion.identity);
                NetworkServer.Spawn(newEnemy);
            }
        }
    }

    private void Update()
    {
        if (!isServer) return;
        difficulty += Time.deltaTime;
    }
}
