using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using HathoraCloud.Models.Operations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SessionManager : MonoBehaviour
{

    #region Public Fields

    public Dictionary<int, Player> Players = new Dictionary<int, Player>();

    public UnityEvent<PlayerListUpdateBroadcast> OnPlayerListUpdate = new UnityEvent<PlayerListUpdateBroadcast>();

    public SessionState SessionState { get; private set; } = SessionState.InLobby;

    public GameMode GameMode { get; private set; } = GameMode.SoloDeathmatch;

    public UnityEvent<GameModeUpdateBroadcast> OnGameModeUpdate = new UnityEvent<GameModeUpdateBroadcast>();

    public UnityEvent<MapChangeBroadcast> OnMapChange = new UnityEvent<MapChangeBroadcast>();

    public UnityEvent<KillLimitBroadcast> OnKillLimitChange = new UnityEvent<KillLimitBroadcast>();

    public List<GameObject> MapPrefabs = new List<GameObject>();

    public int SelectedMapIndex = 0;

    public int KillLimit = 20;

    #endregion

    #region Serialized Fields

    [SerializeField]
    private UserInfo _userInfo;

    [SerializeField]
    private AudioListener _audioListener;

    [SerializeField]
    private int _postGameWaitTime = 5;

    #endregion

    #region Private Fields

    private NetworkManager _networkManager;

    #endregion

    #region Initialization

    void Start()
    {
        _networkManager = InstanceFinder.NetworkManager;

        // Subscribe to events

        _networkManager.SceneManager.OnLoadEnd += OnLoadEnd;
        _networkManager.SceneManager.OnClientPresenceChangeEnd += OnClientPresenceChangeEnd;

        _networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;

        // Server broadcast receivers

        _networkManager.ServerManager.RegisterBroadcast<UsernameBroadcast>(OnUsernameBroadcast, false);
        _networkManager.ServerManager.RegisterBroadcast<StartGameBroadcast>(OnStartGameBroadcast, false);
        _networkManager.ServerManager.RegisterBroadcast<GameModeUpdateBroadcast>(OnGameModeUpdateBroadcastServer, false);
        _networkManager.ServerManager.RegisterBroadcast<MapChangeBroadcast>(OnMapChangeBroadcastServer, false);
        _networkManager.ServerManager.RegisterBroadcast<KillLimitBroadcast>(OnKillLimitBroadcastServer, false);

        // Client broadcast receivers

        _networkManager.ClientManager.RegisterBroadcast<PlayerListUpdateBroadcast>(OnPlayerListUpdateBroadcast);
        _networkManager.ClientManager.RegisterBroadcast<SessionStateUpdateBroadcast>(OnSessionStateUpdateBroadcast);
        _networkManager.ClientManager.RegisterBroadcast<GameModeUpdateBroadcast>(OnGameModeUpdateBroadcast);
        _networkManager.ClientManager.RegisterBroadcast<MapChangeBroadcast>(OnMapChangeBroadcast);
        _networkManager.ClientManager.RegisterBroadcast<KillLimitBroadcast>(OnKillLimitBroadcast);
    }

    #endregion

    #region Events

    #region Connection States

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            UsernameBroadcast usernameBroadcast = new UsernameBroadcast()
            {
                Username = _userInfo.Username
            };

            _networkManager.ClientManager.Broadcast(usernameBroadcast);
        }
    }

    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        PlayerListUpdateBroadcast playerListUpdateBroadcast = new PlayerListUpdateBroadcast();

        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            // Create a new player.
            Player player = new Player()
            {
                Connection = conn,
                Nob = null,
                Username = "Request Username",
                Health = 100,
                IsDead = false
            };

            // Add the player to the list.
            Players.Add(conn.ClientId, player);

            playerListUpdateBroadcast = new PlayerListUpdateBroadcast()
            {
                IsAdd = true,
                IsRemove = false,
                IsUpdate = false,
                Player = player,
                Players = Players
            };

            _networkManager.ServerManager.Broadcast(playerListUpdateBroadcast, false);

            OnPlayerListUpdate.Invoke(playerListUpdateBroadcast);

            Debug.Log($"Player {conn.ClientId} has joined the game.");
        }
        else if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            if (Players.Count == 0)
            {
                return;
            }

            // Remove the player from the list.
            var player = Players[conn.ClientId];

            Players.Remove(conn.ClientId);

            playerListUpdateBroadcast = new PlayerListUpdateBroadcast()
            {
                IsAdd = false,
                IsRemove = true,
                IsUpdate = false,
                Player = player,
                Players = Players
            };

            _networkManager.ServerManager.Broadcast(playerListUpdateBroadcast);

            OnPlayerListUpdate.Invoke(playerListUpdateBroadcast);

            Debug.Log($"Player {conn.ClientId} has left the game.");

            if (Players.Count == 0 && SessionState == SessionState.InGame)
            {
                ReturnToLobby();
            }
        }
    }

    #endregion

    private void OnLoadEnd(SceneLoadEndEventArgs args)
    {
        if (args.LoadedScenes.Length == 0) return;

        if (args.LoadedScenes[0].name == "PreGameLobby")
        {
            //SessionState = SessionState.InLobby;

            // Update the player list.
            /*
            PlayerListUpdateBroadcast playerListUpdateBroadcast = new PlayerListUpdateBroadcast()
            {
                IsAdd = false,
                IsRemove = false,
                IsUpdate = true,
                Players = Players
            };

            _networkManager.ServerManager.Broadcast(playerListUpdateBroadcast);
            */
        }
        else if (args.LoadedScenes[0].name == "OnlineGame")
        {
            // Reset the kills and deaths of all players.
            foreach (var player in Players.Values)
            {
                player.Kills = 0;
                player.Deaths = 0;
            }

            GameManager.Instance.OnGameEnd.AddListener(() => StartCoroutine(PostGameCoroutine()));

            PlayersManager.Instance.OnPlayerKilled.AddListener(OnPlayerKilled);
        }
    }

    private void OnClientPresenceChangeEnd(ClientPresenceChangeEventArgs args)
    {
        // Update the player list.
        PlayerListUpdateBroadcast playerListUpdateBroadcast = new PlayerListUpdateBroadcast()
        {
            IsAdd = false,
            IsRemove = false,
            IsUpdate = true,
            Players = Players
        };

        OnPlayerListUpdate.Invoke(playerListUpdateBroadcast);

        _networkManager.ServerManager.Broadcast(playerListUpdateBroadcast);

        // For good measure, send an update for the current game mode.
        GameModeUpdateBroadcast gameModeUpdateBroadcast = new GameModeUpdateBroadcast()
        {
            GameMode = GameMode
        };

        _networkManager.ServerManager.Broadcast(gameModeUpdateBroadcast);
    }

    #region Client Broadcast Receivers

    private void OnPlayerListUpdateBroadcast(PlayerListUpdateBroadcast broadcast)
    {
        OnPlayerListUpdate.Invoke(broadcast);
    }

    private void OnSessionStateUpdateBroadcast(SessionStateUpdateBroadcast broadcast)
    {
        SessionState = broadcast.SessionState;

        // Toggle the AudioListener on and off based on the session state.
        if (SessionState == SessionState.InLobby)
        {
            //_audioListener.enabled = true;
        }
        else if (SessionState == SessionState.InGame)
        {
            _audioListener.enabled = false;
        }
    }

    private void OnGameModeUpdateBroadcast(GameModeUpdateBroadcast broadcast)
    {
        GameMode = broadcast.GameMode;

        OnGameModeUpdate.Invoke(broadcast);
    }

    private void OnMapChangeBroadcast(MapChangeBroadcast broadcast)
    {
        SelectedMapIndex = broadcast.SelectedMapIndex;

        OnMapChange.Invoke(broadcast);
    }

    private void OnKillLimitBroadcast(KillLimitBroadcast broadcast)
    {
        KillLimit = broadcast.KillLimit;

        OnKillLimitChange.Invoke(broadcast);
    }

    #endregion

    #region Server Broadcast Receivers

    private void OnStartGameBroadcast(NetworkConnection connnection, StartGameBroadcast broadcast)
    {
        StartGame();
    }

    private void OnUsernameBroadcast(NetworkConnection connection, UsernameBroadcast broadcast)
    {
        Players[connection.ClientId].Username = broadcast.Username;

        var playerListUpdateBroadcast = new PlayerListUpdateBroadcast()
        {
            IsAdd = false,
            IsRemove = false,
            IsUpdate = true,
            Player = Players[connection.ClientId],
            Players = Players
        };

        _networkManager.ServerManager.Broadcast(playerListUpdateBroadcast);

        OnPlayerListUpdate.Invoke(playerListUpdateBroadcast);
    }

    // Broadcast received from host when the game mode is updated.
    private void OnGameModeUpdateBroadcastServer(NetworkConnection conn, GameModeUpdateBroadcast broadcast)
    {
        // Set the game mode.
        GameMode = broadcast.GameMode;

        // Let other server components know that the game mode has been updated.
        OnGameModeUpdate.Invoke(broadcast);

        // Broadcast the game mode update to all clients.
        _networkManager.ServerManager.Broadcast(broadcast);
    }

    private void OnMapChangeBroadcastServer(NetworkConnection conn, MapChangeBroadcast broadcast)
    {
        SelectedMapIndex = broadcast.SelectedMapIndex;

        _networkManager.ServerManager.Broadcast(broadcast);
    }

    private void OnKillLimitBroadcastServer(NetworkConnection connection, KillLimitBroadcast broadcast)
    {
        KillLimit = broadcast.KillLimit;

        _networkManager.ServerManager.Broadcast(broadcast);
    }

    #endregion

    #endregion

    #region Public Methods

    /// <summary>
    /// Called by the host to start the game.
    /// </summary>
    public void OnStart()
    {
        // Broadcast to the server to start the game.
        StartGameBroadcast startGameBroadcast = new StartGameBroadcast();
        _networkManager.ClientManager.Broadcast(startGameBroadcast);
    }

    public void ChangeMap(int index)
    {
        // Broadcast to the server to change the map.
        MapChangeBroadcast mapChangeBroadcast = new MapChangeBroadcast()
        {
            SelectedMapIndex = index
        };

        _networkManager.ClientManager.Broadcast(mapChangeBroadcast);

        SelectedMapIndex = index;
    } 

    public void ChangeKillLimit(int killLimit)
    {
        KillLimit = killLimit;

        // Broadcast to the server to change the kill limit.
        KillLimitBroadcast killLimitBroadcast = new KillLimitBroadcast()
        {
            KillLimit = killLimit
        };

        _networkManager.ClientManager.Broadcast(killLimitBroadcast);
    }

    /// <summary>
    /// Called by the host when the gamemode is updated.
    /// </summary>
    public void UpdateGameMode(GameMode gameMode)
    {
        GameModeUpdateBroadcast gameModeUpdateBroadcast = new GameModeUpdateBroadcast()
        {
            GameMode = gameMode
        };

        _networkManager.ClientManager.Broadcast(gameModeUpdateBroadcast);
    }

    public void DespawnPlayer(NetworkConnection conn)
    {
        // Despawn the player's nob.
        Players[conn.ClientId].Nob.Despawn();

        // Set the player's nob to null.
        Players[conn.ClientId].Nob = null;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Called by the server when receiving a StartGameBroadcast.
    /// </summary>
    private void StartGame()
    {
        // Load the game scene.
        SceneLoadData onlineGameScene = new SceneLoadData("OnlineGame");
        onlineGameScene.ReplaceScenes = ReplaceOption.All;
        _networkManager.SceneManager.LoadGlobalScenes(onlineGameScene);


        SessionState = SessionState.InGame;

        _networkManager.ServerManager.Broadcast(new SessionStateUpdateBroadcast() { SessionState = SessionState.InGame });

        _audioListener.enabled = false;
    }

    /// <summary>
    /// Called by the server after post game routine is complete.
    /// </summary>
    private void ReturnToLobby()
    {
        // For each player, despawn their nob.
        foreach (var player in Players.Values)
        {
            player.Nob.Despawn();
            player.Nob = null;
        }

        SceneLoadData preGameLobbyScene = new SceneLoadData("PreGameLobby");
        preGameLobbyScene.ReplaceScenes = ReplaceOption.All;
        _networkManager.SceneManager.LoadGlobalScenes(preGameLobbyScene);


        SessionState = SessionState.InLobby;

        _networkManager.ServerManager.Broadcast(new SessionStateUpdateBroadcast() { SessionState = SessionState.InLobby });

        //_audioListener.enabled = false;
    }

    private IEnumerator PostGameCoroutine()
    {
        yield return new WaitForSeconds(_postGameWaitTime);

        ReturnToLobby();
    }

    private void OnPlayerKilled(Player target, Player attacker, WeaponInfo weaponInfo)
    {
        if (attacker == null) return;

        // Update the target's deaths.
        Players[target.Connection.ClientId].Deaths++;

        // Update the attacker's kills.
        Players[attacker.Connection.ClientId].Kills++;

        // Broadcast a player list update to all clients.
        PlayerListUpdateBroadcast playerListUpdateBroadcast = new PlayerListUpdateBroadcast()
        {
            IsAdd = false,
            IsRemove = false,
            IsUpdate = true,
            Players = Players
        };

        _networkManager.ServerManager.Broadcast(playerListUpdateBroadcast);

        OnPlayerListUpdate.Invoke(playerListUpdateBroadcast);
    }

    #endregion
}

public struct UsernameBroadcast : IBroadcast
{
    public string Username;
}

public struct PlayerListUpdateBroadcast : IBroadcast
{
    public bool IsAdd;
    public bool IsRemove;
    public bool IsUpdate;

    public Player Player;

    public Dictionary<int, Player> Players;
}

public struct  StartGameBroadcast : IBroadcast { }

public struct SessionStateUpdateBroadcast : IBroadcast
{
    public SessionState SessionState;
}

public struct MapChangeBroadcast : IBroadcast
{
    public int SelectedMapIndex;
}

public struct KillLimitBroadcast : IBroadcast
{
    public int KillLimit;
}

public enum SessionState
{
    InLobby,
    InGame
}

public enum GameMode
{
    SoloDeathmatch, // Free for all
    DuoDeathmatch, // 2v2v2v2
    TrioDeathmatch, // 3v3v3
    TripleThreat // 3v3v3 with the combined objectives of Capture the Flag and King of the Hill
}

public struct GameModeUpdateBroadcast : IBroadcast
{
    public GameMode GameMode;
}
