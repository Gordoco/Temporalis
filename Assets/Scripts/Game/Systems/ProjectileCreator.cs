using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(NetworkTransformUnreliable))]
public class ProjectileCreator : NetworkBehaviour
{
    [SerializeField] private float projectileSpeed;
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

    /// <summary>
    /// Server-Only method to be called on Instantiated projectile prefab. Handles client spawning and awakens the projectile to start moving.
    /// </summary>
    /// <param name="startLocation">World space position for the projectile to start, overwrites instantiation transform position</param>
    /// <param name="direction">Direction of travel for the projectile, will be normalized</param>
    [Server]
    public void InitializeProjectile(GameObject owningObj, Vector3 startLocation, Vector3 direction, double damage)
    {
        direction.Normalize();
        hitObjects.Add(owningObj);
        this.direction = direction;
        this.damage = (float)damage;
        gameObject.transform.position = startLocation;
        bAlive = true;
        NetworkServer.Spawn(gameObject);

        GetComponent<Rigidbody>().AddForce(direction * projectileSpeed, ForceMode.VelocityChange);
    }

    float counter = 0;
    private void Update()
    {
        if (isServer && bAlive)
        {
            counter += Time.deltaTime;
            //transform.position += direction * projectileSpeed * Time.deltaTime;
            if (counter >= lifespan)
            {
                NetworkServer.Destroy(gameObject);
            }
        }
    }

    public void OnTriggerEnter(Collider collision)
    {
        Debug.Log("COLLIDED");
        if (isServer && bAlive)
        {
            if (!hitObjects.Contains(collision.gameObject) && collision.gameObject.GetComponent<HitManager>() != null)
            {
                if (OnHitEnemy != null) OnHitEnemy.Invoke(this, collision);
                collision.gameObject.GetComponent<HitManager>().Hit(damage);
                hitObjects.Add(collision.gameObject);
                pierceLevel--;
                if (pierceLevel <= 0) NetworkServer.Destroy(gameObject);
            }
        }
    }
}
