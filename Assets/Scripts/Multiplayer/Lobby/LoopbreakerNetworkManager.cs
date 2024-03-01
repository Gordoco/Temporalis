using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using Steamworks;

public class LoopbreakerNetworkManager : NetworkManager
{
    [SerializeField] private PlayerObjectController GamePlayerPrefab;
    public List<PlayerObjectController> GamePlayers { get; } = new List<PlayerObjectController>();
    private List<NetworkConnectionToClient> Clients = new List<NetworkConnectionToClient>();

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

    private NetworkConnectionToClient GetConnectionFromID(int ConnectionID)
    {
        NetworkConnectionToClient connection;
        NetworkServer.connections.TryGetValue(ConnectionID, out connection);
        return connection;
    }

    public void RemoveClient(int index) { Debug.Log("Client Number: " + index + " Removed"); Clients.RemoveAt(index); }

    public void ServerStartGame(string SceneName)
    {
        ServerChangeScene(SceneName);
        ServerSpawnPlayerGamePrefabs();
    }

    private void ServerSpawnPlayerGamePrefabs()
    {
        for (int i = 0; i < GamePlayers.Count; i++)
        {
            GameObject gamePrefab = Instantiate(GamePlayers[i].GamePrefab);
            gamePrefab.transform.SetParent(GamePlayers[i].transform);
            NetworkServer.ReplacePlayerForConnection(GetConnectionFromID(GamePlayers[i].ConnectionID), gamePrefab);
        }
    }
}
