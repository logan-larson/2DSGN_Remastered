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

    public void OnStartGame()
    {
        _sessionManager.OnStart();
    }

    public void OnLeaveLobby()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
