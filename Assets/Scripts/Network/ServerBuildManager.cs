using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerBuildManager : MonoBehaviour
{
    [SerializeField]
    private BuildInfo _buildInfo;

    [SerializeField]
    private NetworkManager _networkManager;

    [SerializeField]
    private ServerInfo _serverInfo;

    [SerializeField]
    private Tugboat _tugboat;

    private void Awake()
    {
        if (_buildInfo.IsServer)
        {
            _networkManager.ServerManager.StartConnection();

            // Query the lobby ID for the initial player list.
        }
        else
        {
            // Set the connection info
            _tugboat.SetPort(_serverInfo.Port);
            _tugboat.SetClientAddress(_serverInfo.Address);

            _networkManager.ClientManager.StartConnection();
        }
    }

    // Stop connection when leaving game to lobby??
}
