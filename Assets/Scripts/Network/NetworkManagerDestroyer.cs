using FishNet;
using FishNet.Managing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerDestroyer : MonoBehaviour
{
    void Start()
    {
        // Destroy the NetworkManager object.
        var networkManager = InstanceFinder.NetworkManager;

        if (networkManager != null)
        {
            Destroy(networkManager.gameObject);
        }
    }
}
