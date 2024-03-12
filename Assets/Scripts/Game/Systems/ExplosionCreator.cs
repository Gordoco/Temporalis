using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ExplosionCreator : NetworkBehaviour
{
    private float damage;
    private float radius;
    private GameObject owner;

    /// <summary>
    /// Server-Only method to spawn in an explosion
    /// </summary>
    /// <param name="owningObject"></param>
    /// <param name="radius"></param>
    /// <param name="damage"></param>
    [Server]
    public void InitializeExplosion(GameObject owningObject, Vector3 startLocation, float radius, float damage, bool bPlayer)
    {
        owner = owningObject;
        this.radius = radius;
        this.damage = damage;
        gameObject.transform.position = startLocation;
        NetworkServer.Spawn(gameObject);
        RaycastHit[] hits = Physics.SphereCastAll(startLocation, radius, Vector3.up, 0);
        Debug.Log(hits.Length);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject != owningObject)
            {
                if (bPlayer && hits[i].collider.gameObject.GetComponent<PlayerMove>()) continue;
                if (hits[i].collider.gameObject.GetComponent<HitManager>())
                {
                    hits[i].collider.gameObject.GetComponent<HitManager>().Hit(damage);
                }
            }
        }
    }
}
