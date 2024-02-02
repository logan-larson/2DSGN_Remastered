using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInitializer : NetworkBehaviour
{
    private void Awake()
    {
        var networkManager = InstanceFinder.NetworkManager;

        var sessionManager = networkManager.GetComponent<SessionManager>();

        var mapPrefab = sessionManager.MapPrefabs[sessionManager.SelectedMapIndex];

        var map = Instantiate(mapPrefab);
        map.name = mapPrefab.name;
    }
}
