using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The game manager is responsible for managing the state of the game.
/// From directly after all the players are transported to the game scene,
/// to the win/lose conditions and everything in between.
/// Recieves signal from SessionManager when the game starts.
/// Notifies SessionManager when the game ends.
/// </summary>
public class GameManager : NetworkBehaviour
{
    #region Events 

    public UnityEvent<int> OnCountdown = new UnityEvent<int>();

    public UnityEvent<GameState> OnGameStateChange = new UnityEvent<GameState>();

    #endregion

    #region Public Fields

    [SyncVar (OnChange = nameof(OnGameStateChanged))]
    public GameState GameState;

    private void OnGameStateChanged(GameState oldState, GameState newState, bool asServer)
    {
        OnGameStateChange.Invoke(newState);
    }

    #endregion

    #region Private Fields

    [SerializeField]
    private int _countdownDuration = 5;

    [SerializeField]
    private int _killsToWin = 5;

    [SerializeField]
    private int _gameEndOverrideTime = -1;

    //private int _currentKills = 0;

    private NetworkManager _networkManager;

    private SessionManager _sessionManager;

    #endregion

    #region Static Fields

    public static GameManager Instance;

    #endregion

    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        // For now, we'll just start the game when the server loads the scene with 10 second countdown.
        StartCoroutine(CountdownCoroutine());

        _networkManager = InstanceFinder.NetworkManager;
        _sessionManager = _networkManager.GetComponent<SessionManager>();

        _sessionManager.OnPlayerListUpdate.AddListener(OnPlayerListUpdate);

        _killsToWin = _sessionManager.KillLimit;

        PlayersManager.Instance.OnPlayerKilled.AddListener(OnPlayerKilled);

        // Later, we'll want to wait for all the players in the pre game lobby before starting.
    }

    private IEnumerator CountdownCoroutine()
    {
        GameState = GameState.PreGame;

        int countdown = _countdownDuration;
        while (countdown > 0)
        {
            OnCountdown.Invoke(countdown);
            OnCountdownObserversRpc(countdown);
            yield return new WaitForSeconds(1);
            countdown--;
        }

        OnCountdownObserversRpc(countdown);
        OnCountdown.Invoke(0);

        GameState = GameState.InGame;

        // TEMP: For now, we'll just end the game after 5 seconds.
        if (_gameEndOverrideTime != -1)
        {
            yield return new WaitForSeconds(_gameEndOverrideTime);

            GameState = GameState.PostGame;
        }
    }

    [ObserversRpc]
    private void OnCountdownObserversRpc(int countdown)
    {
        OnCountdown.Invoke(countdown);
    }

    private void OnPlayerKilled(Player arg0, Player arg1, WeaponInfo arg2)
    {
        /*
        _currentKills++;

        if (_currentKills >= _killsToWin)
        {
            // Game over
            OnGameEnd.Invoke();

            GameState = GameState.PostGame;
        }
        */
    }

    private void OnPlayerListUpdate(PlayerListUpdateBroadcast broadcast)
    {
        foreach (var player in broadcast.Players.Values)
        {
            if (player.Kills >= _killsToWin && GameState == GameState.InGame)
            {
                // Game over
                GameState = GameState.PostGame;
            }
        }
    }
}
