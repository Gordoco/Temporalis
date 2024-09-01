using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Simple helper script to auto-destroy particle system GameObjects when their lifespan is done. Adapted from AutoDestroyParticle.cs for network usage.
/// </summary>
public class AutoDestroyParticleNetwork : NetworkBehaviour
{
    private ParticleSystem p;
    // Start is called before the first frame update
    void Start()
    {
        if (!GetComponent<ParticleSystem>()) Debug.Log("ERROR: AutoDestroyParticle on a Non-Particle");
    }

    // Update is called once per frame
    float count = 0;
    void Update()
    {
        if (!GetComponent<ParticleSystem>().IsAlive())
        {
            Destroy(gameObject);
            if (isServer) NetworkServer.Destroy(gameObject);
        }
        if (count > 1)
        {
            Destroy(gameObject);
            if (isServer) NetworkServer.Destroy(gameObject);
        }
        count += Time.deltaTime;
    }
}
