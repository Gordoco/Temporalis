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
    [SerializeField] private GameObject AudioManager;

    public List<PlayerObjectController> GamePlayers { get; } = new List<PlayerObjectController>();
    private List<NetworkConnectionToClient> Clients = new List<NetworkConnectionToClient>();

    private bool bLoadingRealMap = false;
    private int playersLoaded = 0;

    public override void Awake()
    {
        base.Awake();
        if (StartLocation == Vector3.zero)
            StartLocation = new Vector3(Random.Range(-20f, 20f), 100, Random.Range(-20.0f, 20.0f));
    }

    public override void Start()
    {
        base.Start();
        if (AudioManager)
        {
            AudioManager.SetActive(true);
        }
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
        //ServerSpawnAllPlayers();
        bLoadingRealMap = true;
        ServerChangeScene(SceneName);
    }

    [Server]
    private void ServerSpawnAllPlayers()
    {
        foreach (PlayerObjectController GamePlayer in GamePlayers)
        {
            GamePlayer.gameObject.transform.position = StartLocation;

            GameObject gamePrefab = Instantiate(GamePlayer.GamePrefabs[GamePlayer.PlayerCharacterChoice], StartLocation, Quaternion.identity);
            gamePrefab.transform.SetParent(GamePlayer.transform, false);
            NetworkServer.Spawn(gamePrefab, GetConnectionFromID(GamePlayer.ConnectionID));

            GamePlayer.RpcSetParent(gamePrefab, GamePlayer.gameObject, false);
            GamePlayer.StartGameMap();
            //gamePrefab.GetComponent<PlayerMove>().SetStart();
        }
    }

    [Server]
    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);

        Debug.Log("Player Ready And Ported");
        if (bLoadingRealMap)
        {
            playersLoaded++;
            if (playersLoaded == GamePlayers.Count) StartCoroutine(SpawnPlayersDelay());
        }
    }

    [Server]
    public void PlayerDied(PlayerObjectController player)
    {
        playersLoaded--;
        if (playersLoaded <= 0)
        {
            Debug.Log("EVERYONE DEAD");
            GameObject AM = GameObject.FindGameObjectWithTag("AudioManager");
            if (AM)
            {
                Destroy(AM);
            }
            for (int i = 0; i < GamePlayers.Count; i++)
            {
                GamePlayers[i].DisableCameraMove();
                GamePlayers[i].Disconnect();
            }
            StartCoroutine(CheckForAllClientsDisconnected());
            //SceneManager.LoadScene("MainMenu");
        }
    }

    private IEnumerator CheckForAllClientsDisconnected()
    {
        while (GamePlayers.Count > 0)
        {
            yield return new WaitForSeconds(0.02f);
        }
        Disconnect(true);
    }

    /// <summary>
    /// Way for clients or host to leave the game
    /// </summary>
    public void Disconnect(bool isServer)
    {
        GameObject AM = GameObject.FindGameObjectWithTag("AudioManager");
        if (AM)
        {
            Destroy(AM);
        }
        if (isServer)
        {
            Debug.Log("STOPPING SERVER");
            StopHost();
        }
        else
        {
            Debug.Log("STOPPING CLIENT");
            StopClient();
        }
    }

    public override void OnStopHost()
    {
        base.OnStopHost();
        Debug.Log("STOPPED");
    }

    IEnumerator SpawnPlayersDelay()
    {
        yield return new WaitForSeconds(5);
        ServerSpawnAllPlayers();
    }
}
