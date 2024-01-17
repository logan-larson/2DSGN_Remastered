using FishNet.Managing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerBuildManager : MonoBehaviour
{
    [SerializeField]
    private BuildInfo _buildInfo;

    [SerializeField]
    private NetworkManager _networkManager;

    private void Awake()
    {
        if (_buildInfo.IsServer)
        {
            _networkManager.ServerManager.StartConnection();
        }
        else
        {
            _networkManager.ClientManager.StartConnection();
        }
    }

    // Stop connection when leaving game to lobby??
}
