using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(SoundManager))]
public class GrenadeTimedExplosion : NetworkBehaviour
{
    // Editor exposed values
    [SerializeField] private GameObject ExplosionPrefab;
    [SerializeField] private float time = 5;
    [SerializeField] private AudioClip ExplosionSound;

    // Local values
    private GameObject owner;
    private float radius;
    private float damage;
    private bool bFromServer = false;

    /// <summary>
    /// Initialization method acting as a constructor within the Component API
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="radius"></param>
    /// <param name="damage"></param>
    /// <param name="inServer"></param>
    public void Init(GameObject owner, float radius, float damage, bool inServer)
    {
        this.owner = owner;
        this.radius = radius;
        this.damage = damage;
        this.bFromServer = inServer;
        AudioCollection.RegisterAudioClip(ExplosionSound);
        GetComponent<ProjectileCreator>().OnHitEnemy += EarlyTrigger;
        StartCoroutine(Explode_Delay(owner, radius, damage, time));
    }

    public void OnTriggerEnter(Collider collision)
    {
        if (!bFromServer && collision.gameObject.GetComponent<EnemyStatManager>())
        {
            EarlyTrigger(this, collision);
        }
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

    /// <summary>
    /// Handles explosion event trigger dependant on network status
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="radius"></param>
    /// <param name="damage"></param>
    private void Explode(GameObject owner, float radius, float damage)
    {
        if (bFromServer)
        {
            GameObject explosion = Instantiate(ExplosionPrefab);
            explosion.GetComponent<ExplosionCreator>().InitializeExplosion(owner, gameObject.transform.position, radius, damage, true, ExplosionSound);
        }
        GetComponent<SoundManager>().PlaySoundEffect(ExplosionSound);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        StartCoroutine(ExplodeSoundDelay(ExplosionSound));
    }

    private IEnumerator ExplodeSoundDelay(AudioClip sound)
    {
        yield return new WaitForSeconds(sound.length);
        Destroy(gameObject);
    }
}
