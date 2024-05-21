using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameLauncher : NetworkBehaviour
{
    [SerializeField] private GameObject GameManagerPrefab;

    /// <summary>
    /// Check for an instance of the game manager, and if none exists create one
    /// </summary>
    public void LaunchGame()
    {
        GameObject manager = GameObject.FindGameObjectWithTag("GameManager");

        //Initial startup case
        if (NetworkManager.singleton == null)
        {
            manager = Instantiate(GameManagerPrefab);
        }
        else
        {
            manager = NetworkManager.singleton.gameObject;
        }

        manager.GetComponent<SteamLobby>().HostLobby();
    }
}
