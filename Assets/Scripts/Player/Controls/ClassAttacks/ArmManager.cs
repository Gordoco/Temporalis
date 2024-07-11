using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmManager : NetworkBehaviour
{
    private GameObject HomeLocation;

    [Server]
    public void Init(GameObject HomeLocation)
    {
        this.HomeLocation = HomeLocation;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer) return;
    }

    
}
