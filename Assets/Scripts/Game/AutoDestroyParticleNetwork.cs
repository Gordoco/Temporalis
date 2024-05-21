using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AutoDestroyParticleNetwork : NetworkBehaviour
{
    private ParticleSystem p;
    // Start is called before the first frame update
    void Start()
    {
        if (!GetComponent<ParticleSystem>()) Debug.Log("ERROR: AutoDestroyParticle on a Non-Particle");
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer) return;
        if (!GetComponent<ParticleSystem>().IsAlive())
        {
            Destroy(gameObject);
            NetworkServer.Destroy(gameObject);
        }
    }
}
