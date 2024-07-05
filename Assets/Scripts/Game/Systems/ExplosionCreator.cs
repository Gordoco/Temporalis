using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ExplosionCreator : NetworkBehaviour
{
    private float damage;
    private float radius;
    private GameObject owner;
    private List<GameObject> oldHits = new List<GameObject>();
    private AudioClip ExplosionSound;

    /// <summary>
    /// Server-Only method to spawn in an explosion
    /// </summary>
    /// <param name="owningObject"></param>
    /// <param name="radius"></param>
    /// <param name="damage"></param>
    [Server]
    public void InitializeExplosion(GameObject owningObject, Vector3 startLocation, float radius, float damage, bool bPlayer, AudioClip sound = null)
    {
        if (isServer) Debug.Log("SHOULD EXPLODE SERVER");
        owner = owningObject;
        this.radius = radius;
        this.damage = damage;
        gameObject.transform.position = startLocation;
        ExplosionSound = sound;
        AudioCollection.RegisterAudioClip(ExplosionSound);
        NetworkServer.Spawn(gameObject);
        PlaySound();
        RaycastHit[] hits = Physics.SphereCastAll(startLocation, radius, Vector3.up, 0);
        //Debug.Log(hits.Length);
        for (int i = 0; i < hits.Length; i++)
        {
            GameObject rootObj = hits[i].collider.gameObject.transform.root.gameObject;
            if (rootObj != owningObject && !oldHits.Contains(rootObj))
            {
                if (bPlayer && hits[i].collider.gameObject.GetComponentInParent<PlayerMove>()) continue;
                if (hits[i].collider.gameObject.GetComponentInParent<HitManager>())
                {
                    hits[i].collider.gameObject.GetComponentInParent<HitManager>().Hit(damage);
                    Debug.Log("HIT SOMONE: " + hits[i].collider.gameObject.transform.root.gameObject.name);
                    oldHits.Add(hits[i].collider.gameObject.transform.root.gameObject);
                }
            }
        }
    }

    private void PlaySound()
    {
        if (!ExplosionSound) return;
        GetComponent<SoundManager>().PlaySoundEffect(ExplosionSound);
    }
}
