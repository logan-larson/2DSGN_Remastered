using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MapManager : NetworkBehaviour
{
    public UnityEvent OnMapSpawned = new UnityEvent();
    public UnityEvent OnMapDestroyed = new UnityEvent();

    [SerializeField]
    private GameObject _mapPrefab;

    public override void OnStartServer()
    {
        base.OnStartServer();

        /*
        var networkManager = InstanceFinder.NetworkManager;

        var sessionManager = networkManager.GetComponent<SessionManager>();

        for (int i = 0; i < sessionManager.CurrentMapOptions.Count; i++)
        {
            Debug.Log($"Map Option {i}: {sessionManager.CurrentMapOptions[i].Name}");
        }

        var mapInfo = sessionManager.CurrentMapOptions[sessionManager.SelectedMapIndex];

        //var mapPrefab = Resources.Load<GameObject>(mapInfo.PrefabPath);
        var mapIndex = sessionManager.CurrentMapOptions[sessionManager.SelectedMapIndex].SessionManagerIndex;

        Debug.Log("Map Index: " + mapIndex);

        var mapPrefab = sessionManager.MapPrefabs[mapIndex];
        */

        //var map = Instantiate(mapPrefab);
        /*
        var map = Instantiate(_mapPrefab);

        InstanceFinder.ServerManager.Spawn(map);

        //map.name = mapPrefab.name;
        map.name = _mapPrefab.name;

        Debug.Log("Map Name: " + map.name);

        OnMapSpawned.Invoke();
        */

        // When the server is initialized, subscribe to the OnLobbyStateChange event.
        var networkManager = InstanceFinder.NetworkManager;

        var lobbyManager = networkManager.GetComponent<LobbyManager>();

        lobbyManager.OnLobbyStateChange.AddListener(OnLobbyStateChange);

        // TODO: Subscribe to the OnMapSelected event to know which map to spawn.
    }

    private void OnLobbyStateChange(LobbyState state)
    {
        if (state == LobbyState.Game) // If the lobby state changes to Game, spawn the map.
        {
            var map = Instantiate(_mapPrefab);

            InstanceFinder.ServerManager.Spawn(map);

            map.name = _mapPrefab.name;

            OnMapSpawned.Invoke();
        }
        else if (state == LobbyState.Lobby) // If the lobby state changes to Lobby, destroy the map.
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

}
