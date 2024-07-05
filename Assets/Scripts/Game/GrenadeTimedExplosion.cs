using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GrenadeTimedExplosion : NetworkBehaviour
{
    [SerializeField] private GameObject ExplosionPrefab;
    [SerializeField] private float time = 5;
    [SerializeField] private AudioClip ExplosionSound;

    private GameObject owner;
    private float radius;
    private float damage;
    private bool bFromServer = false;

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

    private void Explode(GameObject owner, float radius, float damage)
    {
        if (bFromServer)
        {
            GameObject explosion = Instantiate(ExplosionPrefab);
            explosion.GetComponent<ExplosionCreator>().InitializeExplosion(owner, gameObject.transform.position, radius, damage, true, ExplosionSound);
        }
        GetComponent<SoundManager>().PlaySoundEffect(ExplosionSound);
        StartCoroutine(ExplodeSoundDelay(ExplosionSound));
    }

    private IEnumerator ExplodeSoundDelay(AudioClip sound)
    {
        yield return new WaitForSeconds(sound.length);
        Destroy(gameObject);
    }
}
