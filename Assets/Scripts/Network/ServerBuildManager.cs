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
    private GameObject _networkManagerUI;

    [SerializeField]
    private ServerInfo _serverInfo;

    [SerializeField]
    private Tugboat _tugboat;

    private void Awake()
    {
        if (_networkManager == null || _networkManager.TransportManager == null) return;

        Multipass multipass = _networkManager.TransportManager.GetTransport<Multipass>();
        
        if (_buildInfo.IsServer)
        {
            _networkManager.ServerManager.StartConnection();
            _networkManagerUI.SetActive(false);

            // Query the lobby ID for the initial player list.
        }
        else if (_serverInfo.IsFreeplay)
        {
            multipass.SetClientTransport<Yak>();

            _networkManagerUI.SetActive(false);

            _networkManager.ServerManager.StartConnection();
            _networkManager.ClientManager.StartConnection();
        }
        else
        {
            multipass.SetClientTransport<Tugboat>();

            if (_buildInfo.IsProduction)
            {
                _networkManagerUI.SetActive(false);
            }

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
