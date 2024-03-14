using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Responsible for managing the state of the lobby (e.g. Game, Lobby, Countdown, etc.)
// Responsible for managing the player connections too.
public class LobbyManager : NetworkBehaviour
{
    #region Events

    public UnityEvent<LobbyState> OnLobbyStateChange = new UnityEvent<LobbyState>();

    #endregion

    #region Private Fields

    [SyncVar]
    private LobbyState _lobbyState;

    #endregion

    #region Script References

    [SerializeField]
    private PreGameUIManager _preGameUIManager;

    [SerializeField]
    private GameUIManager _gameUIManager;

    // TODO: Isolate this into a separate script.
    [SerializeField]
    private MapManager _mapManager;

    #endregion

    #region Static Fields

    public static LobbyManager Instance;

    #endregion

    #region Initialization

    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        // When the server is initialized, the lobby state is set to Lobby.
        //_lobbyState = LobbyState.PreVoting;
        _lobbyState = LobbyState.Game; // TEMP: For testing purposes, we will start the game immediately.

        OnLobbyStateChange.Invoke(_lobbyState);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();


        // Set the appropriate UI based on the lobby state.
        switch (_lobbyState)
        {
            case LobbyState.PreVoting:
            case LobbyState.Voting:
            case LobbyState.PostVoting:
                _preGameUIManager.gameObject.SetActive(true);
                _gameUIManager.gameObject.SetActive(false);
                break;
            case LobbyState.PreGame:
            case LobbyState.Game:
            case LobbyState.PostGame:
                _preGameUIManager.gameObject.SetActive(false);
                _gameUIManager.gameObject.SetActive(true);
                break;
        }

        // Listen for the UI events

        // TEMP: For testing purposes, we will start the game when a player selects a map in the pre-game UI.
        // This will be replaced by the map voting system and countdown.
        _preGameUIManager.OnMapSelected.AddListener(SelectMap);
    }

    #endregion

    // TODO: Isolate this into a separate script.
    private void SelectMap(string mapName)
    {
        SelectMapServerRpc(mapName);
    }

    // TODO: Isolate this into a separate script.
    [ServerRpc]
    private void SelectMapServerRpc(string mapName)
    {
        _mapManager.SelectMap(mapName);

        _lobbyState = LobbyState.PostVoting;
        OnLobbyStateChange.Invoke(_lobbyState);
    }
}
