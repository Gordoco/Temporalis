using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Helper class for spawning explosion effects and handling their effects on gameplay actors, fully netowork compatible
/// </summary>
public class ExplosionCreator : NetworkBehaviour
{
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
        InitializeInstanceVariables(owningObject, startLocation, radius, damage, sound);

        AudioCollection.RegisterAudioClip(ExplosionSound);
        NetworkServer.Spawn(gameObject);

        PlaySound();

        RaycastHit[] hits = Physics.SphereCastAll(startLocation, radius, Vector3.up, 0);
        for (int i = 0; i < hits.Length; i++)
        {
            HandleExplosionHit(hits[i], damage, bPlayer);
        }
    }

    private void InitializeInstanceVariables(GameObject owningObject, Vector3 startLocation, float radius, float damage, AudioClip sound = null)
    {
        owner = owningObject;
        gameObject.transform.position = startLocation;
        ExplosionSound = sound;
    }

    private void HandleExplosionHit(RaycastHit hit, float damage, bool bPlayer)
    {
        GameObject obj = hit.collider.gameObject;
        GameObject rootObj = obj.transform.root.gameObject;
        if (rootObj != owner && !oldHits.Contains(rootObj))
        {
            if (bPlayer && obj.GetComponentInParent<PlayerMove>()) return;
            if (obj.GetComponentInParent<HitManager>())
            {
                obj.GetComponentInParent<HitManager>().Hit(damage);
                oldHits.Add(obj.transform.root.gameObject);
            }
        }
    }

    private void PlaySound()
    {
        if (!ExplosionSound) return;
        GetComponent<SoundManager>().PlaySoundEffect(ExplosionSound);
    }
}
