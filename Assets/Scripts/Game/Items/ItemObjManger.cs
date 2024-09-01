using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Simple enforcement and pickup script for ensuring spawned items have the required functionality
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(NetworkRigidbodyUnreliable))]
[RequireComponent(typeof(BaseItemComponent))]
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(SoundManager))]
public class ItemObjManger : NetworkBehaviour
{
    private void Start()
    {
        //Do not let this object move
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
    }

    public void OnTriggerEnter(Collider collision)
    {
        if (!isServer) return;
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<StatManager>().AddItem(GetComponent<BaseItemComponent>());
            NetworkServer.Destroy(gameObject);
        }
    }
}
