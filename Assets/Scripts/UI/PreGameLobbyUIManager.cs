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

        _mapDropdown.onValueChanged.AddListener(OnMapDropdownValueChanged);
    }

    private void OnMapDropdownValueChanged(int index)
    {
        _sessionManager.OnMapChange(index);
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

    public void OnStartGame()
    {
        _sessionManager.OnStart();
    }

    public void OnLeaveLobby()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
