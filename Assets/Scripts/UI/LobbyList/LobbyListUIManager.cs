using Beamable.Experimental.Api.Lobbies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyListUIManager : MonoBehaviour
{
    [SerializeField]
    private MainMenuUIManager _mainMenuUIManager;

    [SerializeField]
    private GameObject _lobbyListItemPrefab;

    private void Start()
    {
        _mainMenuUIManager.OnLobbyListUpdated.AddListener(UpdateLobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbies)
    {
        // Clear the current list but keep the first item (the header)
        for (int i = transform.childCount - 1; i > 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        // Add a new item for each lobby
        var place = 1;
        foreach (var lobby in lobbies)
        {
            var lobbyListItem = Instantiate(_lobbyListItemPrefab, transform);
            lobbyListItem.GetComponent<LobbyListItemUIManager>().SetLobby(lobby, place);

            place++;
        }

    }
}
