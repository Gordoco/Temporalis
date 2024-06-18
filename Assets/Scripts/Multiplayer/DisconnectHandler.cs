using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisconnectHandler : MonoBehaviour
{
    public bool isServer = false;

    /// <summary>
    /// Crude, unsafe way of calling NetworkManager Disconnect
    /// </summary>
    public void Disconnect()
    {
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<LoopbreakerNetworkManager>().Disconnect(isServer);
    }
}
