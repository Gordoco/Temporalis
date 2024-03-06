using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class PlayerObjectController : NetworkBehaviour
{
    //Player Data
    [SyncVar] public int ConnectionID;
    [SyncVar] public int PlayerIdNumber;
    [SyncVar] public ulong PlayerSteamID;
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string PlayerName;
    [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool bReady;
    public GameObject GamePrefab;


    private LoopbreakerNetworkManager manager;

    private LoopbreakerNetworkManager Manager
    {
        get
        {
            if (manager != null) return manager;
            return manager = LoopbreakerNetworkManager.singleton as LoopbreakerNetworkManager;
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    [ClientRpc]
    public void RpcSetParent(GameObject obj, GameObject parent)
    {
        obj.transform.SetParent(parent.transform, false);
        obj.transform.localPosition = Vector3.zero;
    }

    [ClientRpc]
    public void RpcSetPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    private void PlayerReadyUpdate(bool OldValue, bool NewValue)
    {
        if (isServer)
        {
            this.bReady = NewValue;
        }
        if (isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }

    [Command]
    private void CmdSetPlayerReady()
    {
        this.PlayerReadyUpdate(this.bReady, !this.bReady);
    }

    public void ServerStartGame(string SceneName)
    {
        if (isServer)
            Manager.ServerStartGame(SceneName);
        else Debug.Log("ERROR: Trying to load scene from client");
    }

    public void ChangeReady()
    {
        if (isOwned)
        {
            CmdSetPlayerReady();
        }
    }

    public override void OnStartAuthority()
    {
        CmdSetPlayerName(SteamFriends.GetPersonaName().ToString());
        gameObject.name = "LocalGamePlayer";
        LobbyController.Instance.FindLocalPlayer();
        LobbyController.Instance.UpdateLobbyName();
    }

    public override void OnStartClient()
    {
        Manager.GamePlayers.Add(this);
        LobbyController.Instance.UpdateLobbyName();
        LobbyController.Instance.UpdatePlayerList();
    }

    public override void OnStopClient()
    {
        Manager.GamePlayers.Remove(this);
        if (Manager.GamePlayers.IndexOf(this) != -1) Manager.RemoveClient(Manager.GamePlayers.IndexOf(this));
        LobbyController.Instance.UpdatePlayerList();
    }

    [Command]
    private void CmdSetPlayerName(string PlayerName)
    {
        this.PlayerNameUpdate(this.PlayerName, PlayerName);
    }

    public void PlayerNameUpdate(string OldValue, string NewValue)
    {
        if (isServer)
        {
            this.PlayerName = NewValue;
        }
        if (isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }
}
