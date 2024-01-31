using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameUIManager : NetworkBehaviour
{

    #region Object References

    [SerializeField]
    private GameObject _pauseCanvas;

    [SerializeField]
    private GameObject _countdownCanvas;

    [SerializeField]
    private GameObject _scoreboardCanvas;

    #endregion

    #region Script References

    private InputManager _inputManager;

    private PlayerManager _playerManager;

    private SessionManager _sessionManager;

    #endregion

    public static GameUIManager Instance;

    #region Initialization

    private void Awake()
    {
        Instance = this;

        _pauseCanvas.SetActive(false);

        _countdownCanvas.SetActive(true);

        FirstObjectNotifier.OnFirstObjectSpawned += FirstObjectNotifier_OnFirstObjectSpawned;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        var networkManager = GameObject.Find("NetworkManager");

        if (networkManager == null)
        {
            Debug.LogError("NetworkManager not found.");
            return;
        }

        _sessionManager = networkManager.GetComponent<SessionManager>();
    }

    private void OnDestroy()
    {
        FirstObjectNotifier.OnFirstObjectSpawned -= FirstObjectNotifier_OnFirstObjectSpawned;
    }

    private void FirstObjectNotifier_OnFirstObjectSpawned(Transform obj, GameObject go)
    {
        _inputManager = obj.GetComponent<InputManager>();
        _playerManager = obj.GetComponent<PlayerManager>();

        _inputManager.TogglePause.AddListener(TogglePause);
        _inputManager.ToggleScoreboard.AddListener(ToggleScoreboard);
    }

    #endregion

    private void TogglePause()
    {
        _pauseCanvas.SetActive(!_pauseCanvas.activeSelf);
    }

    private void ToggleScoreboard()
    {
        SetShowScoreboard(!_scoreboardCanvas.activeSelf);
    }

    public void SetShowScoreboard(bool isShown)
    {
        _scoreboardCanvas.SetActive(isShown);
    }

    public void OnReturnToLobby()
    {
        _scoreboardCanvas.SetActive(false);

        // Destroy the owner's player object
        NetworkConnection playerConn = _playerManager.LocalConnection;


        DespawnPlayerServerRpc(playerConn);

        // Maybe continue loading the lobby scene after the player is despawned? (TargetRpc)


        // Load the lobby scene
        SceneLoadData preGameLobbyScene = new SceneLoadData("PreGameLobby");
        // I think this is going to tell new connections to load the PreGameLobby scene.
        preGameLobbyScene.PreferredActiveScene = new SceneLookupData("PreGameLobby");
        preGameLobbyScene.ReplaceScenes = ReplaceOption.None;

        var networkManager = InstanceFinder.NetworkManager;

        // Load the PreGameLobby scene, but don't transfer the players to it.
        networkManager.SceneManager.LoadConnectionScenes(playerConn, preGameLobbyScene);
    }

    [ServerRpc]
    private void DespawnPlayerServerRpc(NetworkConnection conn)
    {
        _sessionManager.DespawnPlayer(conn);

        // TargetRpc to the player to load the lobby scene??
    }

    public void OnLeaveGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void OnResumeGame()
    {
        _pauseCanvas.SetActive(false);
    }

}
