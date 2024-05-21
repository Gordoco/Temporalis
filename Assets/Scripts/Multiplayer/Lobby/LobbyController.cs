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

    //Character Selection
    public TMP_Dropdown CharacterSelector;

    //Manager
    private LoopbreakerNetworkManager manager;

    /// <summary>
    /// Singleton for the LoopbreakerNetworkManager
    /// </summary>
    private LoopbreakerNetworkManager Manager
    {
        get
        {
            if (manager != null) return manager;
            return manager = LoopbreakerNetworkManager.singleton as LoopbreakerNetworkManager;
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    /// <summary>
    /// Public Server-Only method for launching the main game once all players have readied.
    /// </summary>
    /// <param name="SceneName"></param>
    [Server]
    public virtual void LaunchGame(string SceneName)
    {
        LocalPlayerController.ServerStartGame(SceneName);
    }

    /// <summary>
    /// Method called when a player clicks their "Ready" button in the lobby menu
    /// Relies on UI hooks set in editor.
    /// </summary>
    public void ReadyPlayer()
    {
        LocalPlayerController.ChangeReady();
    }

    /// <summary>
    /// Method called when a player selects a character in the lobby menu
    /// Relies on UI hooks set in editor.
    /// </summary>
    public void SelectCharacter()
    {
        LocalPlayerController.SelectCharacter(CharacterSelector.value);
    }

    /// <summary>
    /// Method to update ReadyButton text appropriately based on the Player's ready status.
    /// </summary>
    public void UpdateButton()
    {
        ReadyButtonText.text = LocalPlayerController.bReady ? "Not Ready" : "Ready";
    }

    /// <summary>
    /// Method for evaluating when all players have readied and allowing the match to be started.
    /// </summary>
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

    /// <summary>
    /// Update the Lobby Name text to correspond to the owner's name.
    /// </summary>
    public void UpdateLobbyName()
    {
        CurrentLobbyID = Manager.GetComponent<SteamLobby>().CurrentLobbyID;
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name");
    }

    /// <summary>
    /// Update method to syncronize the UI of all Player's in the Lobby
    /// </summary>
    public void UpdatePlayerList()
    {
        if (!PlayerItemCreated) { CreateHostPlayerItem(); }
        if (PlayerListItems.Count < Manager.GamePlayers.Count) { CreateClientPlayerItem(); }
        if (PlayerListItems.Count > Manager.GamePlayers.Count) { RemovePlayerItem(); }
        if (PlayerListItems.Count == Manager.GamePlayers.Count) { UpdatePlayerItem(); }
    }

    /// <summary>
    /// Method to find the local player object by its designated name in the Scene hierarchy.
    /// See PlayerObjectController for name reference.
    /// </summary>
    public void FindLocalPlayer()
    {
        LocalPlayerObject = GameObject.Find("LocalGamePlayer");
        LocalPlayerController = LocalPlayerObject.GetComponent<PlayerObjectController>();
    }

    /// <summary>
    /// Server-Side Player UI element creation method
    /// </summary>
    public void CreateHostPlayerItem()
    {
        foreach(PlayerObjectController player in Manager.GamePlayers)
        {
            CreatePlayerItem(player);
        }
        PlayerItemCreated = true;
    }

    /// <summary>
    /// Client-Side Player UI element creation method
    /// </summary>
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

    /// <summary>
    /// Implementation for creating a UI element for a designated player entity, runs on both Server and Client.
    /// </summary>
    /// <param name="player"></param>
    private void CreatePlayerItem(PlayerObjectController player)
    {
        GameObject NewPlayerItem = Instantiate(PlayerListItemPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        PlayerListItem NewPlayerItemScript = NewPlayerItem.GetComponent<PlayerListItem>();

        NewPlayerItemScript.PlayerName = player.PlayerName;
        NewPlayerItemScript.ConnectionID = player.ConnectionID;
        NewPlayerItemScript.PlayerSteamID = player.PlayerSteamID;
        NewPlayerItemScript.bReady = player.bReady;
        NewPlayerItemScript.ChangePlayerCharacterSelection(player.PlayerCharacterChoice);
        NewPlayerItemScript.SetPlayerValues();

        NewPlayerItem.transform.SetParent(PlayerListViewContents.transform, false);
        NewPlayerItem.transform.localScale = Vector3.one;

        PlayerListItems.Add(NewPlayerItemScript);
    }

    /// <summary>
    /// Updates a Player lobby UI element to match current values in the Player object
    /// </summary>
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
                    PlayerListItemScript.ChangePlayerCharacterSelection(player.PlayerCharacterChoice);
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

    /// <summary>
    /// Removes a player from the lobby menu upon them leaving the game
    /// </summary>
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
