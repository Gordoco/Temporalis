using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.VisualScripting;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(NetworkTransformUnreliable))]
public class ProjectileCreator : NetworkBehaviour
{
    [SerializeField] private float projectileSpeed;
    [SerializeField] private bool bInteractWithTerrain = true;
    private float damage;

    /// <summary>
    /// Number of entities that can have damage applied from this projectile.
    /// </summary>
    [SerializeField] private int pierceLevel = 1;

    /// <summary>
    /// In seconds.
    /// </summary>
    [SerializeField] private float lifespan = 5.0f;

    private Vector3 direction;
    private bool bAlive = false;
    private List<GameObject> hitObjects = new List<GameObject>();

    public event System.EventHandler<Collider> OnHitEnemy;

    private bool bEnemy = false;
    private bool bFromServer = false;

    private AudioClip ProjSound = null;

    /// <summary>
    /// Universal method to be called on Instantiated projectile prefab. Handles client spawning and awakens the projectile to start moving.
    /// </summary>
    /// <param name="startLocation">World space position for the projectile to start, overwrites instantiation transform position</param>
    /// <param name="direction">Direction of travel for the projectile, will be normalized</param>
    public void InitializeProjectile(GameObject owningObj, Vector3 startLocation, Vector3 direction, double damage, bool inServer = false, AudioClip sound = null)
    {
        bEnemy = owningObj.tag == "Enemy";
        direction.Normalize();
        hitObjects.Add(owningObj);
        this.direction = direction;
        this.damage = (float)damage;
        this.bFromServer = inServer;
        gameObject.transform.position = startLocation;
        gameObject.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        bAlive = true;
        if (sound != null)
        {
            ProjSound = sound;
            AudioCollection.RegisterAudioClip(ProjSound);
        }
        PlaySound();
        if (bFromServer)
        {
            NetworkServer.Spawn(gameObject);
            Client_PlaySound();
        }

        GetComponent<Rigidbody>().AddForce(direction * projectileSpeed, ForceMode.VelocityChange);
    }

    float counter = 0;
    private void Update()
    {
        if (isServer && bAlive)
        {
            counter += Time.deltaTime;
            if (counter >= lifespan)
            {
                if(bFromServer) NetworkServer.Destroy(gameObject);
                else Destroy(gameObject);
            }
        }
    }

    public void OnTriggerEnter(Collider collision)
    {
        Debug.Log("COLLIDED WITH: " + collision.gameObject.transform.root.gameObject.name);
        if (bAlive)
        {
            if (!(hitObjects.Contains(collision.gameObject.transform.root.gameObject)) && collision.gameObject.GetComponentInParent<HitManager>() != null)
            {
                if (bEnemy && collision.gameObject.transform.root.CompareTag("Enemy"))
                {
                    hitObjects.Add(collision.gameObject.transform.root.gameObject);
                    return;
                }
                if (OnHitEnemy != null) OnHitEnemy.Invoke(this, collision);
                collision.gameObject.GetComponentInParent<HitManager>().Hit(damage);
                hitObjects.Add(collision.gameObject.transform.root.gameObject);
                pierceLevel--;
                if (pierceLevel <= 0)
                {
                    if (bFromServer) NetworkServer.Destroy(gameObject);
                    else Destroy(gameObject);
                }
            }
            else if (collision.gameObject.GetComponentInParent<HitManager>() == null && bInteractWithTerrain)
            {
                if (bFromServer) NetworkServer.Destroy(gameObject);
                else Destroy(gameObject);
            }
        }
    }

    [ClientRpc]
    private void Client_PlaySound()
    {
        PlaySound();
    }

    private void PlaySound()
    {
        if (!ProjSound) return;
        GameObject.FindGameObjectWithTag("AudioManager").GetComponent<SoundManager>().PlaySoundEffect(ProjSound);
    }
}
