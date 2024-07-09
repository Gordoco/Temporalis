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
    [SerializeField] private GameObject DropShipEffectPrefab;
    [SerializeField] private GameObject[] DropShipSpawns;
    [SerializeField] private AudioClip EnemySpawnSound;

    private double difficulty;

    private void Start()
    {
        AudioCollection.RegisterAudioClip(EnemySpawnSound);
        if (!isServer) return;
        difficulty = DifficultyScale;
        StartCoroutine(ConstantEnemySpawner());
    }

    private IEnumerator ConstantEnemySpawner()
    {
        while (SceneManager.GetActiveScene().name == "PyramidLevel")
        {
            yield return new WaitForSeconds(Random.Range(EnemySpawnInterval - 5, EnemySpawnInterval + 2));
            StartCoroutine(HandleDropshipSpawner());
        }
    }

    private IEnumerator HandleDropshipSpawner()
    {
        int i = 0;
        int randSpawn = Random.Range(0, SpawnPoints.Length);
        int dropShipLocationIndex = Random.Range(0, DropShipSpawns.Length);
        GameObject DropShipEffect = Instantiate(DropShipEffectPrefab);
        NetworkServer.Spawn(DropShipEffect);
        SetupDropShipSpawnEffect(DropShipEffect, DropShipSpawns[dropShipLocationIndex].transform.position, SpawnPoints[randSpawn]);
        yield return new WaitForSeconds(0.25f);
        GameObject DropShip = Instantiate(DropShipPrefab);
        NetworkServer.Spawn(DropShip);
        SetupDropship(DropShip, DropShipSpawns[dropShipLocationIndex].transform.position, SpawnPoints[randSpawn]);
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
                RaycastHit hit;
                Physics.Raycast(DropShip.transform.position, Vector3.down, out hit, int.MaxValue);
                GameObject newEnemy = Instantiate(randEnemy, hit.point, Quaternion.identity);
                EnemyStatManager statManager = newEnemy.GetComponent<EnemyStatManager>();
                if (statManager == null)
                {
                    Destroy(newEnemy);
                    break;
                }

                EnemyStatManager newEnemyManager = newEnemy.GetComponent<EnemyStatManager>();
                i += randEnemy.GetComponent<EnemyStatManager>().GetEnemySpawnCost();
                NetworkServer.Spawn(newEnemy);
                SoundManager enemySM = newEnemy.GetComponent<SoundManager>();
                if (EnemySpawnSound) enemySM.PlaySoundEffect(EnemySpawnSound);
                while (newEnemyManager.Initialized == false) yield return new WaitForSeconds(0.01f);
                newEnemyManager.SetStat(NumericalStats.Health, newEnemyManager.GetStat(NumericalStats.Health) * (1 + (difficulty / 100)));
                newEnemyManager.ModifyCurrentHealth(newEnemyManager.GetStat(NumericalStats.Health));
                yield return new WaitForSeconds(Random.Range(0.3f/ (BaseNumEnemies + (int)(difficulty / 30)), 0.75f / (BaseNumEnemies + (int)(difficulty / 30))));
            }
        }
        yield return 0;
    }

    private void SetupDropship(GameObject DropShip, Vector3 Location, Vector3 Goal)
    {
        if (DropShip == null) return;
        Vector3 dir = (Goal - Location).normalized;
        DropShip.transform.position = Location + (dir);
        Quaternion Rot = Quaternion.LookRotation(dir, Vector3.up);
        DropShip.transform.rotation = Rot;
        StartCoroutine(DropshipLocomotion(DropShip, 0.8f));
    }

    private void SetupDropShipSpawnEffect(GameObject DropShipEffect, Vector3 Location, Vector3 Goal)
    {
        if (DropShipEffect == null) return;
        Vector3 dir = (Goal - Location).normalized;
        DropShipEffect.transform.position = Location + (dir);
        Quaternion Rot = Quaternion.LookRotation(dir, Vector3.up);
        DropShipEffect.transform.rotation = Rot;
        StartCoroutine(DestroyDropshipSpawnEffect(DropShipEffect));
    }

    private IEnumerator DestroyDropshipSpawnEffect(GameObject Effect)
    {
        yield return new WaitForSeconds(2);
        Destroy(Effect);
        NetworkServer.Destroy(Effect);
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

    public GameObject GetRandomEnemyPrefab(int maxCost)
    {
        if (EnemyTypes == null || EnemyTypes.Length == 0 || maxCost <= 0) return null;
        int sumChance = 0;
        for (int i = 0; i < EnemyTypes.Length; i++)
        {
            if (EnemyTypes[i].GetComponent<EnemyStatManager>().GetEnemySpawnCost() > maxCost) continue; //Skip if too expensive
            sumChance += EnemyTypes[i].GetComponent<EnemyStatManager>().GetEnemySpawnChance();
        }
        Debug.Log("SUM CHANCE: " + sumChance);
        int randomNum = Random.Range(0, sumChance);
        int count = 0;
        for (int i = 0; i < EnemyTypes.Length; i++)
        {
            if (EnemyTypes[i].GetComponent<EnemyStatManager>().GetEnemySpawnCost() > maxCost) continue; //Skip if too expensive
            count += EnemyTypes[i].GetComponent<EnemyStatManager>().GetEnemySpawnChance();
            if (count > randomNum) return EnemyTypes[i];
        }
        return null;
    }

    private void Update()
    {
        if (!isServer) return;
        difficulty += Time.deltaTime;
    }
}
