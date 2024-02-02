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

    // Scoreboard and Score leader canvas for different game modes

    // - Solo Deathmatch
    [SerializeField]
    private GameObject _soloScoreboardCanvas;
    [SerializeField]
    private GameObject _soloScoreLeaderCanvas;

    // - Duo Deathmatch
    [SerializeField]
    private GameObject _duoScoreboardCanvas;
    [SerializeField]
    private GameObject _duoScoreLeaderCanvas;

    // - Trio Deathmatch
    [SerializeField]
    private GameObject _trioScoreboardCanvas;
    [SerializeField]
    private GameObject _trioScoreLeaderCanvas;

    // - Triple Threat
    [SerializeField]
    private GameObject _tripleThreatScoreboardCanvas;
    [SerializeField]
    private GameObject _tripleThreatScoreLeaderCanvas;

    // Current Scoreboard and Score leader canvas

    private GameObject _currentScoreboardCanvas;
    private GameObject _currentScoreLeaderCanvas;

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

        var networkManager = InstanceFinder.NetworkManager;

        if (networkManager == null)
        {
            Debug.LogError("NetworkManager not found.");
            return;
        }

        _sessionManager = networkManager.GetComponent<SessionManager>();

        if (_sessionManager == null)
        {
            Debug.LogError("SessionManager not found.");
            return;
        }

        switch (_sessionManager.GameMode)
        {
            case GameMode.SoloDeathmatch:
                _currentScoreboardCanvas = _soloScoreboardCanvas;
                _currentScoreLeaderCanvas = _soloScoreLeaderCanvas;
                break;
            case GameMode.DuoDeathmatch:
                _currentScoreboardCanvas = _duoScoreboardCanvas;
                _currentScoreLeaderCanvas = _duoScoreLeaderCanvas;
                break;
            case GameMode.TrioDeathmatch:
                _currentScoreboardCanvas = _trioScoreboardCanvas;
                _currentScoreLeaderCanvas = _trioScoreLeaderCanvas;
                break;
            case GameMode.TripleThreat:
                _currentScoreboardCanvas = _tripleThreatScoreboardCanvas;
                _currentScoreLeaderCanvas = _tripleThreatScoreLeaderCanvas;
                break;
        }

        if (_currentScoreboardCanvas != null)
            _currentScoreboardCanvas.SetActive(false);

        if (_currentScoreLeaderCanvas != null)
            _currentScoreLeaderCanvas.SetActive(true);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        var networkManager = InstanceFinder.NetworkManager;

        if (networkManager == null)
        {
            Debug.LogError("NetworkManager not found.");
            return;
        }

        _sessionManager = networkManager.GetComponent<SessionManager>();

        if (_sessionManager == null)
        {
            Debug.LogError("SessionManager not found.");
            return;
        }

        switch (_sessionManager.GameMode)
        {
            case GameMode.SoloDeathmatch:
                _currentScoreboardCanvas = _soloScoreboardCanvas;
                _currentScoreLeaderCanvas = _soloScoreLeaderCanvas;
                break;
            case GameMode.DuoDeathmatch:
                _currentScoreboardCanvas = _duoScoreboardCanvas;
                _currentScoreLeaderCanvas = _duoScoreLeaderCanvas;
                break;
            case GameMode.TrioDeathmatch:
                _currentScoreboardCanvas = _trioScoreboardCanvas;
                _currentScoreLeaderCanvas = _trioScoreLeaderCanvas;
                break;
            case GameMode.TripleThreat:
                _currentScoreboardCanvas = _tripleThreatScoreboardCanvas;
                _currentScoreLeaderCanvas = _tripleThreatScoreLeaderCanvas;
                break;
        }

        if (_currentScoreboardCanvas != null)
            _currentScoreboardCanvas.SetActive(false);

        if (_currentScoreLeaderCanvas != null)
            _currentScoreLeaderCanvas.SetActive(true);
    }

    private void OnDestroy()
    {
        FirstObjectNotifier.OnFirstObjectSpawned -= FirstObjectNotifier_OnFirstObjectSpawned;
    }

    private void FirstObjectNotifier_OnFirstObjectSpawned(Transform obj, GameObject go)
    {
        _inputManager = obj.GetComponent<InputManager>();
        _playerManager = obj.GetComponent<PlayerManager>();

        _inputManager.TogglePause.AddListener(SetShowPause);
        _inputManager.ToggleScoreboard.AddListener(SetShowScoreboard);
    }

    #endregion

    #region Scoreboard Methods

    public void SetShowScoreboard(bool isShown)
    {
        _currentScoreboardCanvas.SetActive(isShown);
    }

    #endregion

    // DEPRECATED: The player does not have control over returning to the lobby now.
    // The game will automatically return to the lobby after the game ends.
    public void OnReturnToLobby()
    {
        _currentScoreboardCanvas.SetActive(false);

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

    #region Pause Menu Methods

    private void SetShowPause(bool isShown)
    {
        _pauseCanvas.SetActive(isShown);
    }

    public void OnLeaveGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void OnResumeGame()
    {
        _inputManager.SetPause(false);
        _pauseCanvas.SetActive(false);
    }

    #endregion

}
