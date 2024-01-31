using FishNet;
using FishNet.Managing;
using FishNet.Object;
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
    public UnityEvent OnGameStart = new UnityEvent();
    public UnityEvent OnGameEnd = new UnityEvent();

    #endregion

    #region Public Fields

    public GameState GameState;

    #endregion

    [SerializeField]
    private int _countdownDuration = 5;

    [SerializeField]
    private int _killsToWin = 5;

    private int _currentKills = 0;

    private NetworkManager _networkManager;

    //private SessionManager _sessionManager;

    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _networkManager = InstanceFinder.NetworkManager;
        //_sessionManager = _networkManager.GetComponent<SessionManager>();

        PlayersManager.Instance.OnPlayerKilled.AddListener(OnPlayerKilled);

        // For now, we'll just start the game when the server loads the scene with 10 second countdown.
        StartCoroutine(CountdownCoroutine());

        // Later, we'll want to wait for all the players in the pre game lobby before starting.
    }

    private IEnumerator CountdownCoroutine()
    {
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

        // Start the game
        OnGameStart.Invoke();
    }

    [ObserversRpc]
    private void OnCountdownObserversRpc(int countdown)
    {
        OnCountdown.Invoke(countdown);
    }

    private void OnPlayerKilled(Player arg0, Player arg1, WeaponInfo arg2)
    {
        _currentKills++;

        if (_currentKills >= _killsToWin)
        {
            // Game over
            OnGameEnd.Invoke();
        }
    }

}