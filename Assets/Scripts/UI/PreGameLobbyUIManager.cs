using FishNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreGameLobbyUIManager : MonoBehaviour
{
    private SessionManager _sessionManager;

    [SerializeField]
    private GameObject _playerListContainer;

    [SerializeField]
    private GameObject _playerListItemPrefab;

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
    }

    private void OnPlayerListUpdate(PlayerListUpdateBroadcast broadcast)
    {
        // TODO: Utilize the broadcast to update the UI.

        for (int i = 0; i < _playerListContainer.transform.childCount; i++)
        {
            Destroy(_playerListContainer.transform.GetChild(i).gameObject);
        }

        foreach (var player in broadcast.Players.Values)
        {
            var playerListItem = Instantiate(_playerListItemPrefab, _playerListContainer.transform);

            var playerListItemUI = playerListItem.GetComponent<PlayerListItemUIManager>();

            playerListItemUI.SetPlayer(player);
        }
    }

    public void OnStartGame()
    {
        _sessionManager.OnStart();
    }
}
