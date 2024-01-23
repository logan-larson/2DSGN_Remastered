using FishNet;
using FishNet.Component.Spawning;
using FishNet.Connection;
using FishNet.Demo.AdditiveScenes;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using HathoraCloud.Models.Operations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayersManager : NetworkBehaviour
{
    #region Public Fields

    public static PlayersManager Instance { get; private set; }

    public Dictionary<int, Player> Players = new Dictionary<int, Player>();

    #endregion

    #region Serialized Fields

    [Header("Serialized Fields")]

    [SerializeField]
    private List<Vector3> _spawnPositions = new List<Vector3>();

    [SerializeField]
    private UserInfo _userInfo;

    #endregion

    #region Private Fields

    /// <summary>
    /// Reference to the player spawner. This is used to listen for when a player is first spawned.
    /// </summary>
    private PlayerSpawner _playerSpawner;

    #endregion

    #region Initialization

    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        var networkManager = GameObject.Find("NetworkManager");

        if (networkManager == null)
        {
            Debug.LogError("NetworkManager not found.");
            return;
        }

        _playerSpawner = networkManager.GetComponent<PlayerSpawner>();

        if (_playerSpawner == null)
        {
            Debug.LogError("PlayerSpawner not found.");
            return;
        }

        // Subscribe to the player spawner.
        _playerSpawner.OnSpawned += PlayerSpawner_OnSpawned;

        // Subscribe to the remote connection state disconnects.
        ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
    }

    private void PlayerSpawner_OnSpawned(NetworkObject nob)
    {
        // Get the connection.
        NetworkConnection conn = nob.LocalConnection;

        Players[conn.ClientId].Nob = nob;
    }

    private void ServerManager_OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
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
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// This is called by the game manager when the map is generated.
    /// </summary>
    /// <param name="spawnPositions"></param>
    public void SetSpawnPositions(List<Vector3> spawnPositions)
    {
        _spawnPositions = spawnPositions;
    }

    [Server]
    public void DamagePlayer(NetworkConnection attacker, NetworkConnection target, WeaponInfo weapon)
    {
        // TEMP: Debug log the attacker username and target username with the weapon name.
        Debug.Log($"{Players[attacker.ClientId].Username} has damaged {Players[target.ClientId].Username} with {weapon.Name}.");
    }

    public void SetUsername(NetworkConnection conn, string username)
    {
        Debug.Log($"Player {conn.ClientId} has set their username to {username}.");

        Players[conn.ClientId].Username = username;
    }

    #endregion

}
