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
    private List<Transform> _spawnPoints = new List<Transform>();

    private Transform _heaven;

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

        GameObject spawnPoints = GameObject.Find("SpawnPoints");

        if (spawnPoints == null)
        {
            Debug.LogError("SpawnPoints not found.");
            return;
        }

        foreach (Transform child in spawnPoints.transform)
        {
            _spawnPoints.Add(child);
        }

        _heaven = GameObject.Find("Heaven").transform;
    }

    private void PlayerSpawner_OnSpawned(NetworkObject nob)
    {
        // Get the connection.
        NetworkConnection conn = nob.Owner;

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

    [Server]
    public void DamagePlayer(NetworkConnection attackerConn, NetworkConnection targetConn, WeaponInfo weapon)
    {
        // TEMP: Debug log the attacker username and target username with the weapon name.
        //Debug.Log($"{Players[attackerConn.ClientId].Username} has damaged {Players[targetConn.ClientId].Username} with {weapon.Name}.");

        var attacker = Players[attackerConn.ClientId];
        var target = Players[targetConn.ClientId];

        if (target.IsDead)
            return;

        // Reduce the health of the target.
        target.Health -= weapon.Damage;

        target.Nob.GetComponent<PlayerManager>().TakeDamage(weapon.Damage, target.Health);

        // If the target's health is less than or equal to 0, then they are dead.
        if (target.Health <= 0)
        {
            target.IsDead = true;

            // Initiate the respawn.

            // Get a random spawn point.
            Transform spawnPoint = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Count)];

            // Send the player to heaven.
            target.Nob.GetComponent<PlayerController>().OnDeath(_heaven);

            // Start the respawn coroutine.
            StartCoroutine(RespawnPlayer(target, spawnPoint));


            // TEMP: Debug log the attacker username and target username with the weapon name.
            Debug.Log($"{Players[attackerConn.ClientId].Username} has killed {Players[targetConn.ClientId].Username} with {weapon.Name}.");
        }

    }

    public void SetUsername(NetworkConnection conn, string username)
    {
        Debug.Log($"Player {conn.ClientId} has set their username to {username}.");

        Players[conn.ClientId].Username = username;
    }

    #endregion

    #region Private Methods

    private IEnumerator RespawnPlayer(Player player, Transform spawnPoint)
    {
        // Wait for 3 seconds.
        yield return new WaitForSeconds(3f);

        // Reset the player's health.
        player.Health = 100;

        // Reset the player's death state.
        player.IsDead = false;

        // Respawn the player.
        player.Nob.GetComponent<PlayerController>().OnRespawn(spawnPoint);
    }

    #endregion

}
