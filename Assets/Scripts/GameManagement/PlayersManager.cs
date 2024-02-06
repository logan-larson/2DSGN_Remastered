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
using UnityEngine.Events;

public class PlayersManager : NetworkBehaviour
{
    #region Public Fields

    public static PlayersManager Instance { get; private set; }

    #endregion

    #region Serialized Fields

    [Header("Serialized Fields")]

    [SerializeField]
    private List<Transform> _spawnPoints = new List<Transform>();

    private Transform _heaven;

    [SerializeField]
    private UserInfo _userInfo;

    [SerializeField]
    private MapInitializer _mapInitializer;

    #endregion

    #region Events

    public UnityEvent<Player, Player, WeaponInfo> OnPlayerKilled;

    #endregion

    #region Private Fields

    /// <summary>
    /// Reference to the player spawner. This is used to listen for when a player is first spawned.
    /// </summary>
    private PlayerSpawner _playerSpawner;

    /// <summary>
    /// Reference to the session manager.
    /// </summary>
    private SessionManager _sessionManager;

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
        _sessionManager = networkManager.GetComponent<SessionManager>();

        if (_playerSpawner == null)
        {
            Debug.LogError("PlayerSpawner not found.");
            return;
        }
        else if (_sessionManager == null)
        {
            Debug.LogError("SessionManager not found.");
            return;
        }

        // Subscribe to the player spawner.
        _playerSpawner.OnSpawned += PlayerSpawner_OnSpawned;

        // Subscribe to the remote connection state disconnects.
        ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;

        _mapInitializer.OnMapSpawned.AddListener(OnMapSpawned);
    }

    private void OnMapSpawned()
    {
        var map = GameObject.FindWithTag("Map");

        Transform spawnPoints = map.transform.GetChild(2);

        if (spawnPoints == null)
        {
            Debug.LogError("SpawnPoints not found.");
            return;
        }

        foreach (Transform child in spawnPoints)
        {
            _spawnPoints.Add(child);
        }

        _heaven = GameObject.Find("Heaven").transform;

        if (_heaven == null)
        {
            Debug.LogError("Heaven not found.");
            return;
        }
    }

    private void PlayerSpawner_OnSpawned(NetworkObject nob)
    {
        // Get the connection.
        NetworkConnection conn = nob.Owner;

        _sessionManager.Players[conn.ClientId].Nob = nob;
    }

    private void ServerManager_OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
    }

    #endregion

    #region Public Methods

    [Server]
    public void DamagePlayer(NetworkConnection targetConn, NetworkConnection attackerConn = null, WeaponInfo weapon = null, bool isHeadshot = false, Vector3 hitPosition = new Vector3())
    {
        // TEMP: Debug log the attacker username and target username with the weapon name.
        //Debug.Log($"{Players[attackerConn.ClientId].Username} has damaged {Players[targetConn.ClientId].Username} with {weapon.Name}.");
        var target = _sessionManager.Players[targetConn.ClientId];

        var attacker = attackerConn == null ? null : _sessionManager.Players[attackerConn.ClientId];
        var damage = weapon == null ? 1000f : weapon.Damage;

        damage = isHeadshot ? damage * weapon.HeadshotMultiplier : damage;

        damage = Mathf.Floor(damage);

        //if (target.IsDead)
        if (target.Status == PlayerStatus.Dead)
            return;

        // Reduce the health of the target.
        target.Health -= damage;

        target.Nob.GetComponent<PlayerManager>().TakeDamage(damage, target.Health, isHeadshot, hitPosition);

        // If the target's health is less than or equal to 0, then they are dead.
        if (target.Health <= 0)
        {
            target.Status = PlayerStatus.Dead;

            OnPlayerKilled?.Invoke(target, attacker, weapon);

            // Initiate the respawn.

            // Send the player to heaven.
            target.Nob.GetComponent<PlayerManager>().OnDeath(_heaven, target.Connection, attacker != null ? attacker.Nob : null);


            // Start the respawn coroutine.
            StartCoroutine(RespawnPlayer(target));
        }

    }

    [Server]
    public void HealPlayer(NetworkConnection conn, float amount)
    {
        // TEMP: Debug log the target username and amount.
        //Debug.Log($"{Players[conn.ClientId].Username} has been healed for {amount}.");

        // Increase the health of the target.
        var player = _sessionManager.Players[conn.ClientId];
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

        _sessionManager.Players[conn.ClientId].Username = username;
    }

    #endregion

    #region Private Methods

    private Transform GetSpawnPoint()
    {
        // -- Get a spawn point that is furthest away from each enemy player --

        // Add the alive player positions to the list of player positions.
        List<Transform> playerPositions = _sessionManager.Players.Values.Where(p => p.Status == PlayerStatus.Alive).Select(p => p.Nob.transform).ToList();

        // Add the respawning player positions to the list of player positions.
        playerPositions.AddRange(_sessionManager.Players.Values.Where(p => p.Status == PlayerStatus.Respawning).Select(p => p.RespawnPoint));

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

    private IEnumerator RespawnPlayer(Player player)
    {
        // Wait for 2 seconds.
        yield return new WaitForSeconds(2.5f);

        // Get a spawn point that is furhest away from each enemy player that is alive or currently respawning.
        player.RespawnPoint = GetSpawnPoint();
        
        // Set the player's status to respawning.
        player.Status = PlayerStatus.Respawning;

        player.Nob.GetComponent<PlayerManager>().OnSetRespawnPoint(player.Connection, player.RespawnPoint);

        // Wait for 1 second.
        yield return new WaitForSeconds(0.5f);

        // Reset the player's health.
        player.Health = 100;

        // Reset the player's death state.
        player.Status = PlayerStatus.Alive;

        // Respawn the player.
        player.Nob.GetComponent<PlayerManager>().OnRespawn(player.RespawnPoint, player);

        player.RespawnPoint = null;
    }

    #endregion

}
