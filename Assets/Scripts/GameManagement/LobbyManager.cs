using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Responsible for managing the state of the lobby (e.g. Game, Lobby, Countdown, etc.)
// Responsible for managing the player connections too.
public class LobbyManager : MonoBehaviour
{
    public UnityEvent<LobbyState> OnLobbyStateChange = new UnityEvent<LobbyState>();

    private LobbyState _lobbyState;

    private void Start()
    {
        // When the server is initialized, the lobby state is set to Lobby.
        _lobbyState = LobbyState.Lobby;
        OnLobbyStateChange.Invoke(_lobbyState);
    }
}
