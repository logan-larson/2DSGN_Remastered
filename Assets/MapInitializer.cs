using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MapInitializer : NetworkBehaviour
{
    public UnityEvent OnMapSpawned = new UnityEvent();

    public override void OnStartServer()
    {
        base.OnStartServer();

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

        var map = Instantiate(mapPrefab);

        InstanceFinder.ServerManager.Spawn(map);

        map.name = mapPrefab.name;

        Debug.Log("Map Name: " + map.name);

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
