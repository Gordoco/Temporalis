using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns a client-side particle effect for the local player
/// </summary>
public class PlayerDesertParticleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ParticlePrefab;

    // Update is called once per frame
    void Update()
    {
        if (!ParticlePrefab) return;
        GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");
        if (Players.Length > 0)
        {
            for (int i = 0; i < Players.Length; i++)
            {
                if (Players[i].transform.parent.name == "LocalGamePlayer")
                {
                    GameObject particle = Instantiate(ParticlePrefab);
                    particle.transform.parent = Players[i].transform;
                    particle.transform.position = Players[i].transform.position;
                    Destroy(gameObject);
                }
            }
        }
    }
}
