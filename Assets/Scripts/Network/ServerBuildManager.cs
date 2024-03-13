using FishNet.Managing;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.Tugboat;
using FishNet.Transporting.Yak;
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

    [SerializeField]
    private Yak _yak;

    private void Start()
    {
        if (_networkManager == null || _networkManager.TransportManager == null) return;

        Multipass multipass = _networkManager.TransportManager.GetTransport<Multipass>();
        
        if (_buildInfo.IsServer)
        {
            _networkManager.ServerManager.StartConnection();

            // Query the lobby ID for the initial player list.
        }
        else if (_buildInfo.IsLocalTest)
        {
            // Connect to the local server
            multipass.SetClientTransport<Tugboat>();

            // Set the connection info
            _tugboat.SetPort(7770);
            _tugboat.SetClientAddress("localhost");

            if (_networkManager.ClientManager == null)
            {
                //Debug.LogError("ClientManager is null.");
                return;
            }

            _networkManager.ClientManager.StartConnection();
        }
        else if (_serverInfo.IsFreeplay)
        {
            multipass.SetClientTransport<Yak>();

            _networkManager.ServerManager.StartConnection();
            _networkManager.ClientManager.StartConnection();
        }
        else
        {
            multipass.SetClientTransport<Tugboat>();

            // Set the connection info
            _tugboat.SetPort(_serverInfo.Port);
            _tugboat.SetClientAddress(_serverInfo.Address);

            if (_networkManager.ClientManager == null)
            {
                //Debug.LogError("ClientManager is null.");
                return;
            }

            _networkManager.ClientManager.StartConnection();
        }
    }

    // Stop connection when leaving game to lobby??
}
