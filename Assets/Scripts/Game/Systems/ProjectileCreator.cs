using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.VisualScripting;
using System.Security.Cryptography.X509Certificates;

/// <summary>
/// Helper class for spawning and evaluating projectiles on gameplay actors, fully netowrk compatible
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(NetworkTransformUnreliable))]
[RequireComponent(typeof(Rigidbody))]
public class ProjectileCreator : NetworkBehaviour
{
    // Event callback
    public event System.EventHandler<Collider> OnHitEnemy;

    // Configurable values
    [SerializeField] private float projectileSpeed;
    [SerializeField] private bool bInteractWithTerrain = true;

    /// <summary>
    /// Number of distinct entities that can have damage applied from this projectile.
    /// </summary>
    [SerializeField] private int pierceLevel = 1;

    /// <summary>
    /// In seconds.
    /// </summary>
    [SerializeField] private float lifespan = 5.0f;

    private float damage;
    private Vector3 direction;
    private bool bAlive = false;
    private List<GameObject> hitObjects = new List<GameObject>();

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
        InitializeInstanceVariables(owningObj, startLocation, direction, damage, inServer);

        HandleAudio(sound);

        GetComponent<Rigidbody>().AddForce(direction * projectileSpeed, ForceMode.VelocityChange);
    }

    private void InitializeInstanceVariables(GameObject owningObj, Vector3 startLocation, Vector3 direction, double damage, bool inServer)
    {
        bEnemy = owningObj.tag == "Enemy";
        direction.Normalize();
        hitObjects.Add(owningObj);
        this.direction = direction;
        this.damage = (float)damage;
        bFromServer = inServer;
        gameObject.transform.position = startLocation;
        gameObject.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        bAlive = true;
    }

    /// <summary>
    /// Provided that sounds are setup, handles playing them over the network using AudioCollection and SoundManager
    /// </summary>
    /// <param name="sound"></param>
    private void HandleAudio(AudioClip sound)
    {
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
    }

    float counter = 0;
    /// <summary>
    /// Gives every projectile a finite lifespan to avoid GameObject memory leaks
    /// </summary>
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
        if (bAlive)
        {
            if (CheckForValidCollision(collision))
            {
                // Handles enemies shooting one another
                if (bEnemy && collision.transform.root.CompareTag("Enemy"))
                {
                    hitObjects.Add(collision.transform.root.gameObject);
                    return;
                }

                // On player hitting enemy or enemy hitting player
                if (OnHitEnemy != null) OnHitEnemy.Invoke(this, collision); // Allows seperate handling of hit event for visual effects, etc.
                
                // Damage application through HitManager interface
                collision.GetComponentInParent<HitManager>().Hit(damage);

                // Handles projectile cleanup through pierce tracking
                hitObjects.Add(collision.transform.root.gameObject);
                pierceLevel--;
                if (pierceLevel <= 0)
                {
                    if (bFromServer) NetworkServer.Destroy(gameObject);
                    else Destroy(gameObject);
                }
            }
            //Handles projectile collisions with non-actors based on pre-determined parameters
            else if (collision.GetComponentInParent<HitManager>() == null && bInteractWithTerrain)
            {
                if (bFromServer) NetworkServer.Destroy(gameObject);
                else Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Checks if object has not already been collided with and has a valid HitManager, Queries the root of the GameObject Hierarchy
    /// </summary>
    /// <param name="collision"></param>
    /// <returns></returns>
    private bool CheckForValidCollision(Collider collision)
    {
        Transform rootOfCollider = collision.gameObject.transform.root;
        return !hitObjects.Contains(rootOfCollider.gameObject) && collision.gameObject.GetComponentInParent<HitManager>() != null;
    }

    [ClientRpc]
    private void Client_PlaySound()
    {
        PlaySound();
    }

    /// <summary>
    /// Uses the SoundManager to play networked audio
    /// </summary>
    private void PlaySound()
    {
        if (!ProjSound) return;
        GameObject.FindGameObjectWithTag("AudioManager").GetComponent<SoundManager>().PlaySoundEffect(ProjSound);
    }
}
