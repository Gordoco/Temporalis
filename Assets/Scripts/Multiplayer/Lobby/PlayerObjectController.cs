using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class PlayerObjectController : NetworkBehaviour
{
    [SerializeField] private LookAround lookComp;

    //Player Data
    [SyncVar] public int ConnectionID;
    [SyncVar] public int PlayerIdNumber;
    [SyncVar] public ulong PlayerSteamID;
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string PlayerName;
    [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool bReady;
    [SyncVar(hook = nameof(PlayerCharacterUpdate))] public int PlayerCharacterChoice = 0;
    public GameObject[] GamePrefabs;
    [SerializeField] private GameObject SpectatorPrefab;

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
    public void RpcSetParent(GameObject obj, GameObject parent, bool b)
    {
        obj.transform.SetParent(parent.transform, b);
    }

    public void StartGameMap()
    {
        if (!isServer) return;
        if (GameObject.FindGameObjectWithTag("AudioManager"))
        {
            GameObject.FindGameObjectWithTag("AudioManager").GetComponent<SoundManager>().ChangeBackgroundSound(1);
        }
    } 

    [Server]
    public void Die()
    {
        //if (GetComponent<Camera>() == null) gameObject.AddComponent<Camera>();
        DieRpc(isServer);
        Manager.PlayerDied(this);
        //lookComp.enabled = true;
    }

    [ClientRpc]
    public void DieRpc(bool server)
    {
        if (!isOwned) return;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
        GameObject spectator = Instantiate(SpectatorPrefab, transform.position, Quaternion.identity);
        spectator.GetComponent<SpectatorMove>().isServer = server;
        if (gameObject.name == "LocalGamePlayer") GetComponent<AudioListener>().enabled = true;
        //if (GetComponent<Camera>() == null) gameObject.AddComponent<Camera>();
        //lookComp.enabled = true;
    }

    [ClientRpc]
    public void DisableCameraMove()
    {
        GetComponent<LookAround>().enabled = false;
        if (gameObject.name == "LocalGamePlayer") GetComponent<AudioListener>().enabled = false;
        Cursor.lockState = CursorLockMode.None;
    }

    [ClientRpc]
    public void Disconnect()
    {
        if (!isOwned) return;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (gameObject.name == "LocalGamePlayer") GetComponent<AudioListener>().enabled = true;
        Manager.Disconnect(false);
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

    public void SelectCharacter(int character)
    {
        if (isOwned)
        {
            CmdSetPlayerCharacter(character);
        }
    }

    [Command]
    private void CmdSetPlayerCharacter(int character)
    {
        this.PlayerCharacterUpdate(this.PlayerCharacterChoice, character);
    }

    public void PlayerCharacterUpdate(int OldValue, int NewValue)
    {
        if (isServer)
        {
            this.PlayerCharacterChoice = NewValue;
        }
        if (isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
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
