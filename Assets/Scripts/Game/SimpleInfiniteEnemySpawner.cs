using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class SimpleInfiniteEnemySpawner : NetworkBehaviour
{
    // Editor exposed configuration parameters
    [SerializeField] private double DifficultyScale = 0;
    [SerializeField] private Vector3[] SpawnPoints;
    [SerializeField] private float EnemySpawnInterval = 120;
    [SerializeField] private int BaseNumEnemies = 3;
    [SerializeField] private GameObject[] EnemyTypes;
    [SerializeField] private GameObject DropShipPrefab;
    [SerializeField] private GameObject DropShipEffectPrefab;
    [SerializeField] private GameObject[] DropShipSpawns;
    [SerializeField] private AudioClip EnemySpawnSound;

    // Local scaling difficulty reference
    private double difficulty;

    private void Start()
    {
        AudioCollection.RegisterAudioClip(EnemySpawnSound);
        if (!isServer) return;
        difficulty = DifficultyScale;
        StartCoroutine(ConstantEnemySpawner());
    }

    //TODO: Make generic to any gameplay level
    /// <summary>
    /// Simple coroutine for scaling and constant enemy spawning using the "Dropship" interface
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConstantEnemySpawner()
    {
        while (SceneManager.GetActiveScene().name == "PyramidLevel")
        {
            yield return new WaitForSeconds(Random.Range(EnemySpawnInterval - 5, EnemySpawnInterval + 2));
            StartCoroutine(HandleDropshipSpawner());
        }
    }

    /// <summary>
    /// Handles the spawning of all objects over the network pertaining to enemy initialization
    /// </summary>
    /// <returns></returns>
    private IEnumerator HandleDropshipSpawner()
    {
        int i = 0;
        int randSpawn = Random.Range(0, SpawnPoints.Length);
        int dropShipLocationIndex = Random.Range(0, DropShipSpawns.Length);

        // Initialize visual effect for dropship "warp"
        GameObject DropShipEffect = Instantiate(DropShipEffectPrefab);
        NetworkServer.Spawn(DropShipEffect);
        SetupDropShipSpawnEffect(DropShipEffect, DropShipSpawns[dropShipLocationIndex].transform.position, SpawnPoints[randSpawn]);

        yield return new WaitForSeconds(0.25f); //Allow dropship effect to play

        // Initialize dropship object
        GameObject DropShip = Instantiate(DropShipPrefab);
        NetworkServer.Spawn(DropShip);
        SetupDropship(DropShip, DropShipSpawns[dropShipLocationIndex].transform.position, SpawnPoints[randSpawn]);

        // Handle enemy spawning when dropship object has reached designated spawning location
        bool bStartedSpawning = false;
        while (i < (BaseNumEnemies + (int)(difficulty/20)))
        {
            // Delay for travel time
            if (!bStartedSpawning && DropShip && Vector3.Distance(DropShip.transform.position, SpawnPoints[randSpawn]) > 50)
            {
                yield return new WaitForSeconds(0.1f);
            }
            // On arrival handle enemy spawning and initialization
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
                    Debug.LogError("[ERROR - SimpleInfiniteEnemySpawner.cs: Enemy spawned with no StatManager]");
                    break;
                }

                // Contribute to total enemy "cost" budget
                i += statManager.GetEnemySpawnCost();
                NetworkServer.Spawn(newEnemy);

                PlaySpawnSound(newEnemy);

                //Allow for newly initialized statManager to complete async setup
                while (statManager.Initialized == false) yield return new WaitForSeconds(0.01f);

                // Scale enemy health with game difficulty as time progresses
                statManager.SetStat(NumericalStats.Health, statManager.GetStat(NumericalStats.Health) * (1 + (difficulty / 100)));
                statManager.ModifyCurrentHealth(statManager.GetStat(NumericalStats.Health));

                // Small delay between spawns to allow spread out enemy placement
                yield return new WaitForSeconds(Random.Range(0.3f / (BaseNumEnemies + (int)(difficulty / 30)), 0.75f / (BaseNumEnemies + (int)(difficulty / 30))));
            }
        }
        yield return 0;
    }

    /// <summary>
    /// If sound exists, play it at the Enemy's location in 3D space
    /// </summary>
    /// <param name="enemy"></param>
    private void PlaySpawnSound(GameObject enemy)
    {
        if (EnemySpawnSound)
        {
            SoundManager enemySM = enemy.GetComponent<SoundManager>();
            enemySM.PlaySoundEffect(EnemySpawnSound);
        }
    }

    /// <summary>
    /// Initializes the Dropship object with location, rotation, and goal location
    /// </summary>
    /// <param name="DropShip"></param>
    /// <param name="Location"></param>
    /// <param name="Goal"></param>
    private void SetupDropship(GameObject DropShip, Vector3 Location, Vector3 Goal)
    {
        if (DropShip == null) return;
        Vector3 dir = (Goal - Location).normalized;
        DropShip.transform.position = Location + (dir);
        Quaternion Rot = Quaternion.LookRotation(dir, Vector3.up);
        DropShip.transform.rotation = Rot;
        StartCoroutine(DropshipLocomotion(DropShip, 0.8f));
    }


    /// <summary>
    /// Initializes the visual effect for the Dropship object
    /// </summary>
    /// <param name="DropShipEffect"></param>
    /// <param name="Location"></param>
    /// <param name="Goal"></param>
    private void SetupDropShipSpawnEffect(GameObject DropShipEffect, Vector3 Location, Vector3 Goal)
    {
        if (DropShipEffect == null) return;
        Vector3 dir = (Goal - Location).normalized;
        DropShipEffect.transform.position = Location + (dir);
        Quaternion Rot = Quaternion.LookRotation(dir, Vector3.up);
        DropShipEffect.transform.rotation = Rot;
        StartCoroutine(DestroyDropshipSpawnEffect(DropShipEffect));
    }

    /// <summary>
    /// Cleans up GameObject remenants of the Dropship visual effectS
    /// </summary>
    /// <param name="Effect"></param>
    /// <returns></returns>
    private IEnumerator DestroyDropshipSpawnEffect(GameObject Effect)
    {
        yield return new WaitForSeconds(2);
        Destroy(Effect);
        NetworkServer.Destroy(Effect);
    }

    /// <summary>
    /// Simple movement coroutine for the Dropship object
    /// </summary>
    /// <param name="DropShip"></param>
    /// <param name="speed"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Cleanup check for the Dropship object when it exits the visual range of the players
    /// </summary>
    /// <param name="DropShip"></param>
    /// <returns></returns>
    private bool CheckOutOfBoundsDropShip(GameObject DropShip)
    {
        if (DropShip.transform.position.x > 1000 || DropShip.transform.position.x < -1000 || DropShip.transform.position.z > 1000 || DropShip.transform.position.z < -1000) return true;
        return false;
    }

    /// <summary>
    /// Weighted random method for retrieving a proportionally random enemy based on their parameters
    /// </summary>
    /// <param name="maxCost"></param>
    /// <returns></returns>
    public GameObject GetRandomEnemyPrefab(int maxCost)
    {
        if (EnemyTypes == null || EnemyTypes.Length == 0 || maxCost <= 0) return null;
        int sumChance = 0;
        for (int i = 0; i < EnemyTypes.Length; i++)
        {
            if (EnemyTypes[i].GetComponent<EnemyStatManager>().GetEnemySpawnCost() > maxCost) continue; //Skip if too expensive
            sumChance += EnemyTypes[i].GetComponent<EnemyStatManager>().GetEnemySpawnChance();
        }
        //Debug.Log("SUM CHANCE: " + sumChance);

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

    /// <summary>
    /// Progressively scales game difficulty with time
    /// </summary>
    private void Update()
    {
        if (!isServer) return;
        difficulty += Time.deltaTime;
    }
}
