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

    public UnityEvent OnPlayerListUpdated = new UnityEvent();

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

        _networkManager.SceneManager.OnLoadEnd += OnLoadEnd;

        _networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;

        _networkManager.ServerManager.RegisterBroadcast<UsernameBroadcast>(OnUsernameBroadcast);

    }

    #endregion

    #region Events

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        UsernameBroadcast usernameBroadcast = new UsernameBroadcast()
        {
            Username = _userInfo.Username
        };

        _networkManager.ClientManager.Broadcast(usernameBroadcast);
    }

    private void OnLoadEnd(SceneLoadEndEventArgs args)
    {
    }

    private void OnUsernameBroadcast(NetworkConnection connection, UsernameBroadcast broadcast)
    {
        Players[connection.ClientId].Username = broadcast.Username;

        OnPlayerListUpdated.Invoke();
    }

    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
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

            Debug.Log($"Player {conn.ClientId} has joined the game.");
        }
        else if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            if (Players.Count == 0) return;

            // Remove the player from the list.
            Players.Remove(conn.ClientId);

            Debug.Log($"Player {conn.ClientId} has left the game.");
        }

        OnPlayerListUpdated.Invoke();
    }

    #endregion

    #region Public Methods

    public void OnStart()
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
