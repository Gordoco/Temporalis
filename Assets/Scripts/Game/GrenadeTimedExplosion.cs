using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GrenadeTimedExplosion : NetworkBehaviour
{
    [SerializeField] private GameObject ExplosionPrefab;
    [SerializeField] private float time = 5;

    [Server]
    public void Init(GameObject owner, float radius, float damage)
    {
        StartCoroutine(Explode(owner, radius, damage, time));
    }

    [Server]
    private IEnumerator Explode(GameObject owner, float radius, float damage, float time)
    {
        yield return new WaitForSeconds(time);
        GameObject explosion = Instantiate(ExplosionPrefab);
        explosion.GetComponent<ExplosionCreator>().InitializeExplosion(owner, gameObject.transform.position, radius, damage, true);
        Destroy(gameObject);
        NetworkServer.Destroy(gameObject);
    }
}
