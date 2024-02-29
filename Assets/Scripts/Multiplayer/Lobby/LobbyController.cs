using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyController : MonoBehaviour
{
    public static LobbyController Instance;

    //UI Elements
    public TMP_Text LobbyNameText;

    //Player Data
    public GameObject PlayerListViewContents;
    public GameObject PlayerListItemPrefab;
    public GameObject LocalPlayerObject;
    public GameObject PlayerGamePrefab;

    //Other Data
    public ulong CurrentLobbyID;
    public bool PlayerItemCreated = false;
    private List<PlayerListItem> PlayerListItems = new List<PlayerListItem>();
    public PlayerObjectController LocalPlayerController;

    //Ready
    public Button StartGameButton;
    public TMP_Text ReadyButtonText;

    //Manager
    private TemporalisNetworkManager manager;

    private TemporalisNetworkManager Manager
    {
        get
        {
            if (manager != null) return manager;
            return manager = TemporalisNetworkManager.singleton as TemporalisNetworkManager;
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public virtual void LaunchGame(string SceneName)
    {
        LocalPlayerController.CanStartGame(SceneName);
    }

    public void ReadyPlayer()
    {
        LocalPlayerController.ChangeReady();
    }

    public void UpdateButton()
    {
        ReadyButtonText.text = LocalPlayerController.bReady ? "Not Ready" : "Ready";
    }

    public void CheckIfAllReady()
    {
        bool bAllReady = false;

        foreach(PlayerObjectController player in Manager.GamePlayers)
        {
            if (player.bReady) bAllReady = true;
            else
            {
                bAllReady = false;
                break;
            }
        }
        if (bAllReady)
        {
            if (LocalPlayerController.PlayerIdNumber == 1) StartGameButton.interactable = true;
            else StartGameButton.interactable = false;
        }
        else StartGameButton.interactable = false;
    }

    public void UpdateLobbyName()
    {
        CurrentLobbyID = Manager.GetComponent<SteamLobby>().CurrentLobbyID;
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name");
    }

    public void UpdatePlayerList()
    {
        if (!PlayerItemCreated) { CreateHostPlayerItem(); }
        if (PlayerListItems.Count < Manager.GamePlayers.Count) { CreateClientPlayerItem(); }
        if (PlayerListItems.Count > Manager.GamePlayers.Count) { RemovePlayerItem(); }
        if (PlayerListItems.Count == Manager.GamePlayers.Count) { UpdatePlayerItem(); }
    }

    public void FindLocalPlayer()
    {
        LocalPlayerObject = GameObject.Find("LocalGamePlayer");
        LocalPlayerController = LocalPlayerObject.GetComponent<PlayerObjectController>();
    }

    public void CreateHostPlayerItem()
    {
        foreach(PlayerObjectController player in Manager.GamePlayers)
        {
            CreatePlayerItem(player);
        }
        PlayerItemCreated = true;
    }

    public void CreateClientPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if (!PlayerListItems.Any(b => b.ConnectionID == player.ConnectionID))
            {
                CreatePlayerItem(player);
            }
        }
    }

    private void CreatePlayerItem(PlayerObjectController player)
    {
        GameObject NewPlayerItem = Instantiate(PlayerListItemPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        PlayerListItem NewPlayerItemScript = NewPlayerItem.GetComponent<PlayerListItem>();

        NewPlayerItemScript.PlayerName = player.PlayerName;
        NewPlayerItemScript.ConnectionID = player.ConnectionID;
        NewPlayerItemScript.PlayerSteamID = player.PlayerSteamID;
        NewPlayerItemScript.bReady = player.bReady;
        NewPlayerItemScript.SetPlayerValues();

        NewPlayerItem.transform.SetParent(PlayerListViewContents.transform, false);
        NewPlayerItem.transform.localScale = Vector3.one;

        PlayerListItems.Add(NewPlayerItemScript);
    }

    public void UpdatePlayerItem()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            foreach(PlayerListItem PlayerListItemScript in PlayerListItems)
            {
                if (PlayerListItemScript.ConnectionID == player.ConnectionID)
                {
                    PlayerListItemScript.PlayerName = player.PlayerName;
                    PlayerListItemScript.bReady = player.bReady;
                    PlayerListItemScript.SetPlayerValues();
                    if (player == LocalPlayerController)
                    {
                        UpdateButton();
                    }
                }
            }
        }
        CheckIfAllReady();
    }

    public void RemovePlayerItem()
    {
        List<PlayerListItem> playerListItemsToRemove = new List<PlayerListItem>();

        foreach (PlayerListItem playerListItem in PlayerListItems)
        {
            if (!Manager.GamePlayers.Any(b => b.ConnectionID == playerListItem.ConnectionID))
            {
                playerListItemsToRemove.Add(playerListItem);
            }
        }
        if (playerListItemsToRemove.Count > 0)
        {
            foreach(PlayerListItem playerListItemToRemove in playerListItemsToRemove)
            {
                GameObject ObjectToRemove = playerListItemToRemove.gameObject;
                PlayerListItems.Remove(playerListItemToRemove);
                Destroy(ObjectToRemove);
                ObjectToRemove = null;
            }
        }
    }
}
