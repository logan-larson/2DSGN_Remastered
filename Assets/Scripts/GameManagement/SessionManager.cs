using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SessionManager : MonoBehaviour
{

    #region Public Fields

    public Dictionary<int, Player> Players = new Dictionary<int, Player>();

    public UnityEvent<PlayerListUpdateBroadcast> OnPlayerListUpdate = new UnityEvent<PlayerListUpdateBroadcast>();

    public SessionState SessionState { get; private set; } = SessionState.InLobby;

    #endregion

    #region Serialized Fields

    [SerializeField]
    private UserInfo _userInfo;

    #endregion

    #region Private Fields

    private NetworkManager _networkManager;

    #endregion

    #region Initialization

    void Start()
    {
        _networkManager = InstanceFinder.NetworkManager;

        // Subscribe to events

        _networkManager.SceneManager.OnLoadEnd += OnLoadEnd;

        _networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;

        // Server broadcast receivers

        _networkManager.ServerManager.RegisterBroadcast<UsernameBroadcast>(OnUsernameBroadcast, false);
        _networkManager.ServerManager.RegisterBroadcast<StartGameBroadcast>(OnStartGameBroadcast, false);

        // Client broadcast receivers

        _networkManager.ClientManager.RegisterBroadcast<PlayerListUpdateBroadcast>(OnPlayerListUpdateBroadcast);
        _networkManager.ClientManager.RegisterBroadcast<SessionStateUpdateBroadcast>(OnSessionStateUpdateBroadcast);
    }

    #endregion

    #region Events

    #region Connection States

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        UsernameBroadcast usernameBroadcast = new UsernameBroadcast()
        {
            Username = _userInfo.Username
        };

        _networkManager.ClientManager.Broadcast(usernameBroadcast);
    }

    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        PlayerListUpdateBroadcast playerListUpdateBroadcast = new PlayerListUpdateBroadcast();

        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            // Create a new player.
            Player player = new Player()
            {
                Connection = conn,
                Nob = null,
                Username = "Request Username",
                Health = 100,
                IsDead = false
            };

            // Add the player to the list.
            Players.Add(conn.ClientId, player);

            playerListUpdateBroadcast = new PlayerListUpdateBroadcast()
            {
                IsAdd = true,
                IsRemove = false,
                IsUpdate = false,
                Player = player,
                Players = Players
            };

            _networkManager.ServerManager.Broadcast(playerListUpdateBroadcast, false);

            OnPlayerListUpdate.Invoke(playerListUpdateBroadcast);

            Debug.Log($"Player {conn.ClientId} has joined the game.");
        }
        else if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            if (Players.Count == 0) return;

            // Remove the player from the list.
            var player = Players[conn.ClientId];

            Players.Remove(conn.ClientId);

            playerListUpdateBroadcast = new PlayerListUpdateBroadcast()
            {
                IsAdd = false,
                IsRemove = true,
                IsUpdate = false,
                Player = player,
                Players = Players
            };

            _networkManager.ServerManager.Broadcast(playerListUpdateBroadcast);

            OnPlayerListUpdate.Invoke(playerListUpdateBroadcast);

            Debug.Log($"Player {conn.ClientId} has left the game.");
        }
    }

    #endregion

    private void OnLoadEnd(SceneLoadEndEventArgs args)
    {
        /*
        // TODO: It seems like there should be a better solution than this for unloading the pre-game lobby scene
        // when a player joins a game in progress.

        //Scene[] scenes = UnityEngine.SceneManagement.SceneManager.GetAllScenes();
        var preGameLobbyIndex = -1;
        var onlineGameIndex = -1;

        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name == "PreGameLobby")
            {
                preGameLobbyIndex = i;
            }
            else if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name == "OnlineGame")
            {
                onlineGameIndex = i;
            }
        }

        if (preGameLobbyIndex != -1 && onlineGameIndex != -1)
        {
            if (SessionState == SessionState.InGame)
            {
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("PreGameLobby");
            }
            else if (SessionState == SessionState.InLobby)
            {
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("OnlineGame");
            }
        }
        */

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "PreGameLobby")
        {
            //SessionState = SessionState.InLobby;
        }
        else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "OnlineGame")
        {
            GameManager.Instance.OnGameEnd.AddListener(EndGame);
        }
    }

    #region Client Broadcast Receivers

    private void OnPlayerListUpdateBroadcast(PlayerListUpdateBroadcast broadcast)
    {
        OnPlayerListUpdate.Invoke(broadcast);
    }

    private void OnSessionStateUpdateBroadcast(SessionStateUpdateBroadcast broadcast)
    {
        SessionState = broadcast.SessionState;
    }

    #endregion

    #region Server Broadcast Receivers

    private void OnStartGameBroadcast(NetworkConnection connnection, StartGameBroadcast broadcast)
    {
        StartGame();
    }

    private void OnUsernameBroadcast(NetworkConnection connection, UsernameBroadcast broadcast)
    {
        Players[connection.ClientId].Username = broadcast.Username;

        var playerListUpdateBroadcast = new PlayerListUpdateBroadcast()
        {
            IsAdd = false,
            IsRemove = false,
            IsUpdate = true,
            Player = Players[connection.ClientId],
            Players = Players
        };

        _networkManager.ServerManager.Broadcast(playerListUpdateBroadcast);

        OnPlayerListUpdate.Invoke(playerListUpdateBroadcast);
    }

    #endregion

    #endregion

    #region Public Methods

    /// <summary>
    /// Called by the host to start the game.
    /// </summary>
    public void OnStart()
    {
        // Broadcast to the server to start the game.
        StartGameBroadcast startGameBroadcast = new StartGameBroadcast();
        _networkManager.ClientManager.Broadcast(startGameBroadcast);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Called by the server when receiving a StartGameBroadcast.
    /// </summary>
    private void StartGame()
    {
        // Load the game scene.
        SceneLoadData onlineGameScene = new SceneLoadData("OnlineGame");
        onlineGameScene.ReplaceScenes = ReplaceOption.All;
        _networkManager.SceneManager.LoadGlobalScenes(onlineGameScene);

        /*
        SceneUnloadData preGameLobbyScene = new SceneUnloadData("PreGameLobby");
        preGameLobbyScene.Options.Mode = UnloadOptions.ServerUnloadMode.UnloadUnused;
        _networkManager.SceneManager.UnloadGlobalScenes(preGameLobbyScene);
        */

        SessionState = SessionState.InGame;

        _networkManager.ServerManager.Broadcast(new SessionStateUpdateBroadcast() { SessionState = SessionState.InGame });
    }

    private void EndGame()
    {
        SceneLoadData preGameLobbyScene = new SceneLoadData("PreGameLobby");
        preGameLobbyScene.ReplaceScenes = ReplaceOption.All;
        _networkManager.SceneManager.LoadGlobalScenes(preGameLobbyScene);

        //SceneUnloadData onlineGameScene = new SceneUnloadData("OnlineGame");
        //onlineGameScene.Options.Mode = UnloadOptions.ServerUnloadMode.UnloadUnused;
        //_networkManager.SceneManager.UnloadGlobalScenes(onlineGameScene);

        SessionState = SessionState.InLobby;

        _networkManager.ServerManager.Broadcast(new SessionStateUpdateBroadcast() { SessionState = SessionState.InLobby });
    }

    #endregion
}

public struct UsernameBroadcast : IBroadcast
{
    public string Username;
}

public struct PlayerListUpdateBroadcast : IBroadcast
{
    public bool IsAdd;
    public bool IsRemove;
    public bool IsUpdate;

    public Player Player;

    public Dictionary<int, Player> Players;
}

public struct  StartGameBroadcast : IBroadcast { }

public struct SessionStateUpdateBroadcast : IBroadcast
{
    public SessionState SessionState;
}

public enum SessionState
{
    InLobby,
    InGame
}
