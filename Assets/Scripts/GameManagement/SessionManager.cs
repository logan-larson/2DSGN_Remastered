using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    private NetworkManager _networkManager;

    void Start()
    {
        _networkManager = InstanceFinder.NetworkManager;
    }

    public void OnStart()
    {
        SceneLoadData onlineGameScene = new SceneLoadData("OnlineGame");
        SceneUnloadData preGameLobbyScene = new SceneUnloadData("PreGameLobby");

        _networkManager.SceneManager.LoadGlobalScenes(onlineGameScene);

        _networkManager.SceneManager.UnloadGlobalScenes(preGameLobbyScene);
    }
}
