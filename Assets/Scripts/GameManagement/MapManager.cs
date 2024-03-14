using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Responsible for managing the spawning and destruction of the map.

// Also responsible for managing the map voting system. Might be moved to a different script.
public class MapManager : NetworkBehaviour
{

    #region Events

    public UnityEvent OnMapSpawned = new UnityEvent();

    public UnityEvent OnMapDestroyed = new UnityEvent();

    #endregion

    #region Serialized Fields

    [SerializeField]
    private GameObject _mapPrefab;

    [SerializeField]
    private List<GameObject> _maps = new List<GameObject>();

    #endregion

    #region Script References

    [SerializeField]
    private LobbyManager _lobbyManager;

    #endregion

    #region Public Fields

    [SyncVar]
    public bool IsMapSpawned;

    #endregion

    #region Initialization

    public override void OnStartServer()
    {
        base.OnStartServer();

        // Subscribe to the OnLobbyStateChange event to know when to spawn or destroy the map.
        _lobbyManager.OnLobbyStateChange.AddListener(OnLobbyStateChange);

        // Spawn the map.
        SpawnMap();
    }

    #endregion

    private void OnLobbyStateChange(LobbyState state)
    {
        if (state == LobbyState.PostVoting) // If the lobby state changes to PostVoting, this means a map has been selected and we can spawn it.
        {
            // If a previous map exists, destroy it.
            //var map = GameObject.Find(_mapPrefab.name);

            // TODO: Get the selected map from the (MapVotingManager or LobbyManager) and spawn it.

            // Spawn the map.
            var map = Instantiate(_mapPrefab);

            InstanceFinder.ServerManager.Spawn(map);

            map.name = _mapPrefab.name;

            OnMapSpawned.Invoke();
        }
        else if (state == LobbyState.PreVoting) // If the lobby state changes to Lobby, destroy the map.
        {
            var map = GameObject.Find(_mapPrefab.name);

            if (map != null)
            {
                InstanceFinder.ServerManager.Despawn(map);
                Destroy(map);
            }

            OnMapDestroyed.Invoke();
        }
    }


    #region Public Methods

    [Server]
    public void SelectMap(string mapName)
    {
        switch (mapName)
        {
            case "Forest":
                _mapPrefab = _maps[0];
                break;
            case "Arctic":
                _mapPrefab = _maps[1];
                break;
            case "Cave":
                _mapPrefab = _maps[2];
                break;
        }
    }

    [Server]
    public void SpawnMap()
    {
        // Spawn the map.
        var map = Instantiate(_mapPrefab);

        InstanceFinder.ServerManager.Spawn(map);

        map.name = _mapPrefab.name;

        OnMapSpawned.Invoke();
    }

    #endregion
}
