using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using Steamworks;

public class LoopbreakerNetworkManager : NetworkManager
{
    [SerializeField] private PlayerObjectController GamePlayerPrefab;
    [SerializeField] private Vector3 StartLocation = Vector3.zero;

    public List<PlayerObjectController> GamePlayers { get; } = new List<PlayerObjectController>();
    private List<NetworkConnectionToClient> Clients = new List<NetworkConnectionToClient>();

    public override void Awake()
    {
        base.Awake();
        if (StartLocation == Vector3.zero)
            StartLocation = new Vector3(Random.Range(-20f, 20f), 100, Random.Range(-20.0f, 20.0f));
    }

    [Server]
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            Clients.Add(conn);

            PlayerObjectController GamePlayerInstance = Instantiate(GamePlayerPrefab);

            GamePlayerInstance.ConnectionID = conn.connectionId;
            GamePlayerInstance.PlayerIdNumber = GamePlayers.Count + 1;
            GamePlayerInstance.PlayerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.Instance.CurrentLobbyID, GamePlayers.Count);

            NetworkServer.AddPlayerForConnection(conn, GamePlayerInstance.gameObject);
        }
    }

    [Server]
    private NetworkConnectionToClient GetConnectionFromID(int ConnectionID)
    {
        NetworkConnectionToClient connection;
        NetworkServer.connections.TryGetValue(ConnectionID, out connection);
        return connection;
    }

    [Server]
    public void RemoveClient(int index) { Debug.Log("Client Number: " + index + " Removed"); Clients.RemoveAt(index); }

    [Server]
    public void ServerStartGame(string SceneName)
    {
        ServerSpawnAllPlayers();
        ServerChangeScene(SceneName);
    }

    [Server]
    private void ServerSpawnAllPlayers()
    {
        foreach (PlayerObjectController GamePlayer in GamePlayers)
        {
            GameObject gamePrefab = Instantiate(GamePlayer.GamePrefab, GamePlayer.transform);
            gamePrefab.transform.SetParent(GamePlayer.transform);
            NetworkServer.Spawn(gamePrefab, GetConnectionFromID(GamePlayer.ConnectionID));
            GamePlayer.RpcSetParent(gamePrefab, GamePlayer.gameObject);
        }
    }

    [Server]
    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);

        Debug.Log("Player Ready And Ported");

        PlayerObjectController Player = null;
        foreach (PlayerObjectController GamePlayer in GamePlayers)
        {
            if (GamePlayer.ConnectionID == conn.connectionId) Player = GamePlayer;
        }
        if (Player == null) return;
        Player.gameObject.transform.position = StartLocation;
    }
}
