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
    [SerializeField] private GameObject DropShipPrefab;
    [SerializeField] private GameObject[] DropShipSpawns;

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
            yield return new WaitForSeconds(Random.Range(EnemySpawnInterval - 5, EnemySpawnInterval + 2));
            StartCoroutine(HandleDropshipSpawner());
        }
    }

    private IEnumerator HandleDropshipSpawner()
    {
        GameObject DropShip = Instantiate(DropShipPrefab);
        NetworkServer.Spawn(DropShip);
        int i = 0;
        int randSpawn = Random.Range(0, SpawnPoints.Length);
        SetupDropship(DropShip, DropShipSpawns[Random.Range(0, DropShipSpawns.Length)].transform.position, SpawnPoints[randSpawn]);
        bool bStartedSpawning = false;
        while (i < (BaseNumEnemies + (int)(difficulty/20)))
        {
            if (!bStartedSpawning && DropShip && Vector3.Distance(DropShip.transform.position, SpawnPoints[randSpawn]) > 50)
            {
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                bStartedSpawning = true;
                GameObject randEnemy = GetRandomEnemyPrefab((BaseNumEnemies + (int)(difficulty / 10)) - i);
                GameObject newEnemy = Instantiate(randEnemy, DropShip.transform.position, Quaternion.identity);
                EnemyStatManager statManager = newEnemy.GetComponent<EnemyStatManager>();
                if (statManager == null)
                {
                    Destroy(newEnemy);
                    break;
                }
                i+=randEnemy.GetComponent<EnemyStatManager>().GetEnemySpawnCost();
                NetworkServer.Spawn(newEnemy);
                yield return new WaitForSeconds(Random.Range(0.3f/ (BaseNumEnemies + (int)(difficulty / 30)), 0.75f / (BaseNumEnemies + (int)(difficulty / 30))));
            }
        }
        yield return 0;
    }

    private void SetupDropship(GameObject DropShip, Vector3 Location, Vector3 Goal)
    {
        if (DropShip == null) return;
        DropShip.transform.position = Location;
        Quaternion Rot = Quaternion.LookRotation(Goal - Location, Vector3.up);
        DropShip.transform.rotation = Rot;
        StartCoroutine(DropshipLocomotion(DropShip, 0.8f));
    }

    private IEnumerator DropshipLocomotion(GameObject DropShip, float speed)
    {
        while (DropShip)
        {
            yield return new WaitForSeconds(0.01f);
            if (DropShip != null) DropShip.transform.position += DropShip.transform.forward * speed;
            if (DropShip != null && CheckOutOfBoundsDropShip(DropShip))
            {
                NetworkServer.Destroy(DropShip);
                if (DropShip) Destroy(DropShip);
            }
        }
    }

    private bool CheckOutOfBoundsDropShip(GameObject DropShip)
    {
        if (DropShip.transform.position.x > 1000 || DropShip.transform.position.x < -1000 || DropShip.transform.position.z > 1000 || DropShip.transform.position.z < -1000) return true;
        return false;
    }

    public GameObject GetRandomEnemyPrefab(int weight)
    {
        if (EnemyTypes == null || EnemyTypes.Length == 0 || weight <= 0) return null;
        int randIndex = Random.Range(0, EnemyTypes.Length);
        while (EnemyTypes[randIndex].GetComponent<EnemyStatManager>().GetEnemySpawnCost() > weight) randIndex = Random.Range(0, EnemyTypes.Length);
        return EnemyTypes[randIndex];
        /*int sum = 0;
        for (int i = 0; i < EnemyTypes.Length; i++) sum += EnemyTypes[i].GetComponent<EnemyStatManager>().GetEnemySpawnCost();
        int rand = Random.Range(0, sum);
        int index = -1;
        sum = 0;
        for (int i = 0; i < EnemyTypes.Length; i++) { sum += EnemyTypes[i].GetComponent<EnemyStatManager>().GetEnemySpawnCost(); if (sum > rand) { index = i; break; } }
        return EnemyTypes[index];*/
    }

    private void Update()
    {
        if (!isServer) return;
        difficulty += Time.deltaTime;
    }
}
