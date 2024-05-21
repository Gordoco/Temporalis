using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(NetworkRigidbodyUnreliable))]
[RequireComponent(typeof(BaseItemComponent))]
[RequireComponent(typeof(Renderer))]
public class ItemObjManger : NetworkBehaviour
{
    private void Start()
    {
        //Do not let this object move
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        //if (isServer) StartCoroutine(ItemDespawn());
    }

    //If a despawn time is needed
    /*private IEnumerator ItemDespawn()
    {
        yield return new WaitForSeconds(10);
        NetworkServer.Destroy(gameObject);
        Destroy(gameObject);
    }*/

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
