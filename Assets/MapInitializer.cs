using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MapInitializer : NetworkBehaviour
{
    public UnityEvent OnMapSpawned = new UnityEvent();

    [SerializeField]
    private GameEvent _stopMusicEvent;

    private void Start()
    {
        _stopMusicEvent.Raise();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        var networkManager = InstanceFinder.NetworkManager;

        var sessionManager = networkManager.GetComponent<SessionManager>();

        var mapPrefab = sessionManager.MapPrefabs[sessionManager.SelectedMapIndex];

        var map = Instantiate(mapPrefab);

        InstanceFinder.ServerManager.Spawn(map);

        map.name = mapPrefab.name;

        OnMapSpawned.Invoke();
    }

    /*
    private void Awake()
    {
        var networkManager = InstanceFinder.NetworkManager;

        var sessionManager = networkManager.GetComponent<SessionManager>();

        var mapPrefab = sessionManager.MapPrefabs[sessionManager.SelectedMapIndex];

        var map = Instantiate(mapPrefab);

        InstanceFinder.ServerManager.Spawn(map);

        map.name = mapPrefab.name;
    }
    */
}
