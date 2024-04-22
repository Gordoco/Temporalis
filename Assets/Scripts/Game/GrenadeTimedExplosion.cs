using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GrenadeTimedExplosion : NetworkBehaviour
{
    [SerializeField] private GameObject ExplosionPrefab;
    [SerializeField] private float time = 5;

    private GameObject owner;
    private float radius;
    private float damage;

    public void Init(GameObject owner, float radius, float damage)
    {
        this.owner = owner;
        this.radius = radius;
        this.damage = damage;
        GetComponent<ProjectileCreator>().OnHitEnemy += EarlyTrigger;
        StartCoroutine(Explode_Delay(owner, radius, damage, time));
    }

    private void EarlyTrigger(object sender, Collider collision)
    {
        StopAllCoroutines();
        Explode(owner, radius, damage);
    }

    private IEnumerator Explode_Delay(GameObject owner, float radius, float damage, float time)
    {
        yield return new WaitForSeconds(time);
        Explode(owner, radius, damage);
    }

    private void Explode(GameObject owner, float radius, float damage)
    {
        if (isServer)
        {
            GameObject explosion = Instantiate(ExplosionPrefab);
            explosion.GetComponent<ExplosionCreator>().InitializeExplosion(owner, gameObject.transform.position, radius, damage, true);
        }
        Destroy(gameObject);
        //NetworkServer.Destroy(gameObject);
    }
}
