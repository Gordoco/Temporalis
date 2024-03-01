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
        for (int i = 0; i < Clients.Count; i++)
        {
            if (Clients[i].connectionId == ConnectionID) return Clients[i];
        }
        return null;
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
            GamePlayers[i].SpawnPlayerPrefab();
        }
    }
}
