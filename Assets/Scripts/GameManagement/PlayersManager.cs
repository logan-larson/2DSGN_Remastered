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
using System.Linq;
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

    #region Events

    public event Action<Player, Player, WeaponInfo> OnPlayerKilled;

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
    public void DamagePlayer(NetworkConnection targetConn, NetworkConnection attackerConn = null, WeaponInfo weapon = null, bool isHeadshot = false, Vector3 hitPosition = new Vector3())
    {
        // TEMP: Debug log the attacker username and target username with the weapon name.
        //Debug.Log($"{Players[attackerConn.ClientId].Username} has damaged {Players[targetConn.ClientId].Username} with {weapon.Name}.");
        var target = Players[targetConn.ClientId];

        var attacker = attackerConn == null ? null : Players[attackerConn.ClientId];
        var damage = weapon == null ? 1000f : weapon.Damage;

        damage = isHeadshot ? damage * 1.5f : damage;

        if (target.IsDead)
            return;

        // Reduce the health of the target.
        target.Health -= damage;

        target.Nob.GetComponent<PlayerManager>().TakeDamage(damage, target.Health, isHeadshot, hitPosition);

        // If the target's health is less than or equal to 0, then they are dead.
        if (target.Health <= 0)
        {
            target.IsDead = true;

            OnPlayerKilled?.Invoke(target, attacker, weapon);

            // Initiate the respawn.

            // Get a spawn point that is furhest away from each enemy player.
            Transform spawnPoint = GetSpawnPoint();

            // Send the player to heaven.
            target.Nob.GetComponent<PlayerManager>().OnDeath(_heaven, target.Connection, attacker != null ? attacker.Nob : null);


            // Start the respawn coroutine.
            StartCoroutine(RespawnPlayer(target, spawnPoint));
        }

    }

    [Server]
    public void HealPlayer(NetworkConnection conn, float amount)
    {
        // TEMP: Debug log the target username and amount.
        //Debug.Log($"{Players[conn.ClientId].Username} has been healed for {amount}.");

        // Increase the health of the target.
        var player = Players[conn.ClientId];
        player.Health += amount;

        // TEMP: Debug log the target username and amount.
        //Debug.Log($"{Players[conn.ClientId].Username} has been healed for {amount}.");

        // If the target's health is greater than 100, then set it to 100.
        if (player.Health > 100)
        {
            player.Health = 100;
        }

        player.Nob.GetComponent<PlayerManager>().SetHealth(player.Health);
    }

    public void SetUsername(NetworkConnection conn, string username)
    {
        Debug.Log($"Player {conn.ClientId} has set their username to {username}.");

        Players[conn.ClientId].Username = username;
    }

    #endregion

    #region Private Methods

    private Transform GetSpawnPoint()
    {
        // Get a spawn point that is furthest away from each enemy player.

        Transform[] playerPositions = Players.Values.Where(p => !p.IsDead).Select(p => p.Nob.transform).ToArray();

        // Find spawn point furthest away from all players
        Transform furthestSpawnPoint = _spawnPoints[0];
        float maxDistance = 0f;

        foreach (Transform spawnPoint in _spawnPoints)
        {
            // Calculate the distance from each player to the spawn point
            float distanceSum = 0f;

            foreach (Transform playerPos in playerPositions)
            {
                distanceSum += Vector3.Distance(spawnPoint.position, playerPos.position);
            }

            // Check if the distance is greater than the previous max distance
            if (distanceSum > maxDistance)
            {
                maxDistance = distanceSum;
                furthestSpawnPoint = spawnPoint;
            }
        }

        // furthestSpawnPoint will contain the spawn point furthest from all players
        return furthestSpawnPoint;
    }

    private IEnumerator RespawnPlayer(Player player, Transform spawnPoint)
    {
        // Wait for 3 seconds.
        yield return new WaitForSeconds(3f);

        // Reset the player's health.
        player.Health = 100;

        // Reset the player's death state.
        player.IsDead = false;

        // Respawn the player.
        player.Nob.GetComponent<PlayerManager>().OnRespawn(spawnPoint, player);
    }

    #endregion

}
