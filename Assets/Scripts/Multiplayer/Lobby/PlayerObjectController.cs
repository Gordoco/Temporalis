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

    public void ChangeReady()
    {
        if (authority)
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

    public void CanStartGame(string SceneName)
    {
        if (authority) CmdCanStartGame(SceneName);
    }

    [Command]
    public void CmdCanStartGame(string SceneName)
    {
        Manager.ServerStartGame(SceneName);
    }

    public virtual void SpawnPlayerPrefab()
    {
        if (authority)
        {
            GameObject newPrefab = Instantiate(GamePrefab);
            newPrefab.transform.parent = transform;
        }
    }
}
