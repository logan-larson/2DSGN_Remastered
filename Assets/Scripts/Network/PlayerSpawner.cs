using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerSpawner : MonoBehaviour
{
    #region Public.
    /// <summary>
    /// Called on the server when a player is spawned.
    /// </summary>
    public event Action<NetworkObject> OnSpawned;
    #endregion

    #region Serialized.
    /// <summary>
    /// Prefab to spawn for the player.
    /// </summary>
    [Tooltip("Prefab to spawn for the player.")]
    [SerializeField]
    private NetworkObject _playerPrefab;
    /// <summary>
    /// True to add player to the active scene when no global scenes are specified through the SceneManager.
    /// </summary>
    [Tooltip("True to add player to the active scene when no global scenes are specified through the SceneManager.")]
    [SerializeField]
    private bool _addToDefaultScene = true;
    /// <summary>
    /// Areas in which players may spawn.
    /// </summary>
    [Tooltip("Areas in which players may spawn.")]
    [FormerlySerializedAs("_spawns")]//Remove on 2024/01/01
    public Transform[] Spawns = new Transform[0];
    #endregion

    #region Private Fields

    /// <summary>
    /// NetworkManager on this object or within this objects parents.
    /// </summary>
    private NetworkManager _networkManager;

    private SessionManager _sessionManager;

    /// <summary>
    /// Next spawns to use.
    /// </summary>
    private int _nextSpawn;

    #endregion

    private void Start()
    {
        InitializeOnce();
    }

    private void OnDestroy()
    {
        if (_networkManager != null)
            _networkManager.SceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
    }


    /// <summary>
    /// Initializes this script for use.
    /// </summary>
    private void InitializeOnce()
    {
        _networkManager = InstanceFinder.NetworkManager;
        if (_networkManager == null)
        {
            Debug.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
            return;
        }

        _sessionManager = _networkManager.GetComponent<SessionManager>();

        if (_sessionManager == null)
        {
            Debug.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as SessionManager wasn't found on this object or within parent objects.");
            return;
        }

        _networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;

        _networkManager.SceneManager.OnClientPresenceChangeEnd += SceneManager_OnClientPresenceChangeEnd;
    }

    /// <summary>
    /// Called when a client loads initial scenes after connecting. Typically this is when the players
    /// join the game scene directly from the lobby search. Join in progress.
    /// </summary>
    private void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
    {
        if (!asServer)
            return;
        if (_playerPrefab == null)
        {
            Debug.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
            return;
        }

        // If the current scene is OnlineGame then spawn the player.
        var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName == "OnlineGame" || sceneName == "OfflineGame")
            SpawnPlayer(conn);
    }

    /// <summary>
    /// Called when a client changes scenes. Typically this is when the players join the game scene from the lobby.
    /// </summary>
    /// <param name="args"></param>
    private void SceneManager_OnClientPresenceChangeEnd(ClientPresenceChangeEventArgs args)
    {
        if (_playerPrefab == null)
        {
            Debug.LogWarning($"Player prefab is empty and cannot be spawned for connection {args.Connection.ClientId}.");
            return;
        }

        // If the current scene is OnlineGame then spawn the player.
        var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName == "OnlineGame" || sceneName == "OfflineGame")
            SpawnPlayer(args.Connection);
    }

    private void SpawnPlayer(NetworkConnection conn)
    {
        if (_sessionManager.Players.TryGetValue(conn.ClientId, out Player player) && player.Nob != null)
        {
            Debug.LogWarning($"Player {player.Username} is already spawned.");
            return;
        }

        GameObject spawnPoints = GameObject.Find("SpawnPoints");

        if (spawnPoints == null)
        {
            Debug.LogError("SpawnPoints not found.");
            return;
        }

        List<Transform> spawns = new List<Transform>();

        foreach (Transform child in spawnPoints.transform)
        {
            spawns.Add(child);
        }

        Spawns = spawns.ToArray();

        Vector3 position;
        Quaternion rotation;
        SetSpawn(_playerPrefab.transform, out position, out rotation);

        NetworkObject nob = _networkManager.GetPooledInstantiated(_playerPrefab, position, rotation, true);
        _networkManager.ServerManager.Spawn(nob, conn);

        _sessionManager.Players[conn.ClientId].Nob = nob;

        //If there are no global scenes 
        if (_addToDefaultScene)
            _networkManager.SceneManager.AddOwnerToDefaultScene(nob);

        OnSpawned?.Invoke(nob);
    }


    /// <summary>
    /// Sets a spawn position and rotation.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    private void SetSpawn(Transform prefab, out Vector3 pos, out Quaternion rot)
    {
        //Increase next spawn and reset if needed.
        _nextSpawn++;
        if (_nextSpawn >= Spawns.Length)
            _nextSpawn = 0;

        //No spawns specified.
        if (Spawns.Length == 0)
        {
            SetSpawnUsingPrefab(prefab, out pos, out rot);
            return;
        }

        Transform result = Spawns[_nextSpawn];
        if (result == null)
        {
            SetSpawnUsingPrefab(prefab, out pos, out rot);
        }
        else
        {
            pos = result.position;
            rot = result.rotation;
        }
    }

    /// <summary>
    /// Sets spawn using values from prefab.
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    private void SetSpawnUsingPrefab(Transform prefab, out Vector3 pos, out Quaternion rot)
    {
        pos = prefab.position;
        rot = prefab.rotation;
    }

}