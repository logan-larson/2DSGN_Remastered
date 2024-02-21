using Beamable.Experimental.Api.Lobbies;
using FishNet;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreGameLobbyUIManager : MonoBehaviour
{
    private SessionManager _sessionManager;

    [SerializeField]
    private GameObject _playerListContainer;

    [SerializeField]
    private GameObject _playerListItemPrefab;

    [SerializeField]
    private TMP_Dropdown _mapDropdown;

    [SerializeField]
    private TMP_InputField _killLimitInputField;

    [SerializeField]
    private GameObject _startGameButton;

    [SerializeField]
    private TMP_Text _lobbyCodeText;

    private void Start()
    {
        var networkManager = InstanceFinder.NetworkManager;

        _sessionManager = networkManager.GetComponent<SessionManager>();

        if (_sessionManager == null)
        {
            Debug.LogError("SessionManager not found.");
            return;
        }

        _sessionManager.OnPlayerListUpdate.AddListener(OnPlayerListUpdate);
        _sessionManager.OnMapChange.AddListener(OnMapChange);
        _sessionManager.OnKillLimitChange.AddListener(OnKillLimitChange);
        _sessionManager.OnLobbyUpdate.AddListener(OnLobbyUpdate);
        _sessionManager.OnHostChange.AddListener(OnHostChange);

        _mapDropdown.onValueChanged.AddListener(OnMapDropdownValueChanged);
        _killLimitInputField.onEndEdit.AddListener(OnKillLimitInputFieldEndEdit);
    }

    private void OnMapDropdownValueChanged(int index)
    {
        _sessionManager.ChangeMap(index);
    }

    private void OnMapChange(MapChangeBroadcast broadcast)
    {
        _mapDropdown.value = broadcast.SelectedMapIndex;
    }

    private void OnKillLimitInputFieldEndEdit(string value)
    {
        if (int.TryParse(value, out int killLimit))
        {
            _sessionManager.ChangeKillLimit(killLimit);
        }
    }

    private void OnKillLimitChange(KillLimitBroadcast broadcast)
    {
        _killLimitInputField.text = broadcast.KillLimit.ToString();
    }

    private void OnPlayerListUpdate(PlayerListUpdateBroadcast broadcast)
    {
        // TODO: Utilize the broadcast to update the UI.

        for (int i = 0; i < _playerListContainer.transform.childCount; i++)
        {
            Destroy(_playerListContainer.transform.GetChild(i).gameObject);
        }

        int place = 1;
        foreach (var player in broadcast.Players.Values)
        {
            var playerListItem = Instantiate(_playerListItemPrefab, _playerListContainer.transform);

            var playerListItemUI = playerListItem.GetComponent<PlayerListItemUIManager>();

            playerListItemUI.SetPlayer(player, place);

            place++;
        }
    }

    private void OnLobbyUpdate(Lobby lobby)
    {
        var details = ParseDescription(lobby.description);

        _mapDropdown.value = _mapDropdown.options.FindIndex(option => option.text == details.Map);

        // TODO: Set the other details provided by the lobby.

        _lobbyCodeText.text = lobby.passcode;
    }

    private void OnHostChange(bool isHost)
    {
        Debug.Log("isHost: " + isHost);

        _mapDropdown.gameObject.SetActive(isHost);
        _killLimitInputField.gameObject.SetActive(isHost);
        _startGameButton.SetActive(isHost);
    }

    public void OnStartGame()
    {
        _sessionManager.OnStart();
    }

    public void OnLeaveLobby()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    private LobbyListItemDetails ParseDescription(string description)
    {
        // The description is a semicolon delimited list of strings
        // server_address:port;map;gamemode;gamestate;player_count;max_players
        var details = new LobbyListItemDetails();

        var parts = description.Split(';');

        details.Map = parts[1];
        details.Gamemode = parts[2];
        details.Gamestate = parts[3];
        //details.PlayerCount = parts[4];

        return details;
    }
}
