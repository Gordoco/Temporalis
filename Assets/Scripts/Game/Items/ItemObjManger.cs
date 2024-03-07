using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BaseItemComponent))]
[RequireComponent(typeof(Renderer))]
public class ItemObjManger : NetworkBehaviour
{
    private void Start()
    {
        //Do not let this object move
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    }

    public void OnTriggerEnter(Collider collision)
    {
        Debug.Log("COLLIDED");
        if (!isServer) return;
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<StatManager>().AddItem(GetComponent<BaseItemComponent>());
            NetworkServer.Destroy(gameObject);
        }
    }
}
