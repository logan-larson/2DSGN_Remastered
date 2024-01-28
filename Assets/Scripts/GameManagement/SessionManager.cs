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

public class SessionManager : MonoBehaviour
{

    #region Public Fields

    public Dictionary<int, Player> Players = new Dictionary<int, Player>();

    public UnityEvent<PlayerListUpdateBroadcast> OnPlayerListUpdate = new UnityEvent<PlayerListUpdateBroadcast>();

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

        // Register broadcasts

        _networkManager.ServerManager.RegisterBroadcast<UsernameBroadcast>(OnUsernameBroadcast, false);
        _networkManager.ClientManager.RegisterBroadcast<PlayerListUpdateBroadcast>(OnPlayerListUpdateBroadcast);
        _networkManager.ServerManager.RegisterBroadcast<StartGameBroadcast>(OnStartGameBroadcast, false);
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
    }

    #region Client Broadcast Receivers

    private void OnPlayerListUpdateBroadcast(PlayerListUpdateBroadcast broadcast)
    {
        OnPlayerListUpdate.Invoke(broadcast);
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

    public void OnStart()
    {
        StartGameBroadcast startGameBroadcast = new StartGameBroadcast();

        _networkManager.ClientManager.Broadcast(startGameBroadcast);
    }

    #endregion

    #region Private Methods

    private void StartGame()
    {
        SceneLoadData onlineGameScene = new SceneLoadData("OnlineGame");
        SceneUnloadData preGameLobbyScene = new SceneUnloadData("PreGameLobby");

        _networkManager.SceneManager.LoadGlobalScenes(onlineGameScene);
        _networkManager.SceneManager.UnloadGlobalScenes(preGameLobbyScene);
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
