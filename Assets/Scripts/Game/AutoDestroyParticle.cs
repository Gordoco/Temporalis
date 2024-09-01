using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple helper script to auto-destroy particle system GameObjects when their lifespan is done
/// </summary>
public class AutoDestroyParticle : MonoBehaviour
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
        if (!GetComponent<ParticleSystem>().IsAlive())
        {
            Destroy(gameObject);
        }
    }
}
