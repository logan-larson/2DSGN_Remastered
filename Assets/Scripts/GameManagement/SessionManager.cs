using Beamable;
using Beamable.Common.Content;
using Beamable.Experimental.Api.Lobbies;
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

    public SessionState SessionState { get; private set; } = SessionState.InLobbyWaitingForVote;

    public GameMode GameMode { get; private set; } = GameMode.SoloDeathmatch;

    public UnityEvent<SessionStateUpdateBroadcast> OnSessionStateUpdate = new UnityEvent<SessionStateUpdateBroadcast>();

    public UnityEvent<OptionSelectedBroadcast> OnOptionSelected = new UnityEvent<OptionSelectedBroadcast>();

    public UnityEvent<GameModeUpdateBroadcast> OnGameModeUpdate = new UnityEvent<GameModeUpdateBroadcast>();

    public UnityEvent<MapChangeBroadcast> OnMapChange = new UnityEvent<MapChangeBroadcast>();

    public UnityEvent<KillLimitBroadcast> OnKillLimitChange = new UnityEvent<KillLimitBroadcast>();

    public UnityEvent<OptionVoteChangeBroadcast> OnOptionVoteChange = new UnityEvent<OptionVoteChangeBroadcast>();

    //public UnityEvent<int> OnOptionSelected = new UnityEvent<int>();

    public List<MapInfo> AvailableMaps = new List<MapInfo>();

    public List<MapInfo> CurrentMapOptions = new List<MapInfo>();

    public List<GameObject> MapPrefabs = new List<GameObject>();

    public UnityEvent<MapOptionsBroadcast> OnMapOptionsChange = new UnityEvent<MapOptionsBroadcast>();

    public int SelectedMapIndex = 0;

    public int KillLimit = 20;

    public Lobby Lobby { get; private set; }

    public UnityEvent<Lobby> OnLobbyUpdate = new UnityEvent<Lobby>();

    public UnityEvent<bool> OnHostChange = new UnityEvent<bool>();

    #endregion

    #region Serialized Fields

    [SerializeField]
    private UserInfo _userInfo;

    [SerializeField]
    private ServerInfo _serverInfo;

    [SerializeField]
    private AudioListener _audioListener;

    [SerializeField]
    private int _votingCountdownDuration = 20;

    [SerializeField]
    private int _gameStartCountdownDuration = 5;


    [SerializeField]
    private int _postGameWaitTime = 5;

    [SerializeField]
    private SimGameTypeRef _simGameTypeRef;

    #endregion

    #region Private Fields

    private NetworkManager _networkManager;

    private BeamContext _beamContext;

    private Coroutine _votingCountdownCoroutine;

    private Coroutine _gameStartCountdownCoroutine;

    private bool _isOffline = false;

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
        _networkManager.ServerManager.RegisterBroadcast<OptionVoteBroadcast>(OnOptionVoteBroadcastServer, false);
        _networkManager.ServerManager.RegisterBroadcast<ReturnToLobbyBroadcast>(OnReturnToLobbyServer, false);

        // Client broadcast receivers

        _networkManager.ClientManager.RegisterBroadcast<PlayerListUpdateBroadcast>(OnPlayerListUpdateBroadcast);
        _networkManager.ClientManager.RegisterBroadcast<SessionStateUpdateBroadcast>(OnSessionStateUpdateBroadcast);
        _networkManager.ClientManager.RegisterBroadcast<GameModeUpdateBroadcast>(OnGameModeUpdateBroadcast);
        _networkManager.ClientManager.RegisterBroadcast<MapChangeBroadcast>(OnMapChangeBroadcast);
        _networkManager.ClientManager.RegisterBroadcast<KillLimitBroadcast>(OnKillLimitBroadcast);

        _networkManager.ClientManager.RegisterBroadcast<OptionVoteChangeBroadcast>(OnOptionVoteChangeBroadcast);
        _networkManager.ClientManager.RegisterBroadcast<OptionSelectedBroadcast>(OnOptionSelectedBroadcast);
        _networkManager.ClientManager.RegisterBroadcast<MapOptionsBroadcast>(OnMapOptionsBroadcast);

        SetMapPrefabOptions();
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

            SetupBeamable();
        }
        else if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            if (_beamContext != null && _beamContext.Lobby != null)
            {
                _beamContext.Lobby.Leave();
            }
        }
    }

    private async void SetupBeamable()
    {
        _beamContext = BeamContext.Default;
        await _beamContext.OnReady;
        _beamContext.Lobby.OnDataUpdated += OnLobbyDataUpdated;
        _beamContext.Lobby.OnUpdated += Lobby_OnUpdated;

        Lobby = _beamContext.Lobby.Value;

        OnLobbyUpdate.Invoke(Lobby);

        Lobby_OnUpdated();

        UpdateLobbyDetails();

    }

    private void Lobby_OnUpdated()
    {
        //Debug.Log($"Host: {_beamContext.Lobby.Host}, playerId: {_beamContext.PlayerId}");

        //OnHostChange.Invoke(_beamContext.PlayerId.ToString() == _beamContext.Lobby.Host);

    }

    private void OnLobbyDataUpdated(Lobby lobby)
    {
        //Debug.Log($"Host: {lobby.host}, playerId: {_beamContext.PlayerId}");

        //OnHostChange.Invoke(_beamContext.PlayerId.ToString() == lobby.host);

        /*
        Lobby = lobby;

        OnLobbyUpdate.Invoke(Lobby);

        UpdateLobbyDetails();
        */
    }


    /// <summary>
    /// Called when a client connects to the server.
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="args"></param>
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
                IsDead = false,
                Status = PlayerStatus.Alive,
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

            UpdateLobbyDetails();
            Debug.Log($"Player {conn.ClientId} has joined the game.");

            MapOptionsBroadcast mapOptionsBroadcast = new MapOptionsBroadcast()
            {
                MapOptions = CurrentMapOptions
            };

            _networkManager.ServerManager.Broadcast(mapOptionsBroadcast, false);
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

            UpdateLobbyDetails();

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
            OnLobbyUpdate.Invoke(Lobby);
        }
        //else if (args.LoadedScenes[0].name == "OnlineGame")
        else
        {
            // Reset the kills and deaths of all players.
            foreach (var player in Players.Values)
            {
                player.Kills = 0;
                player.Deaths = 0;
            }

            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChange.AddListener(OnGameStateChange);

            if (PlayersManager.Instance != null)
                PlayersManager.Instance.OnPlayerKilled.AddListener(OnPlayerKilled);
        }
    }

    private void OnGameStateChange(GameState gameState)
    {
        if (gameState == GameState.PostGame)
        {
            StartCoroutine(PostGameCoroutine());
        }
        /*
        else if (gameState == GameState.PreGame)
        {
            // Moved back to lobby
            ResetLobby();
        }
        */
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

    /// <summary>
    /// When the session state is updated on the server, update the session state on the client.
    /// </summary>
    /// <param name="broadcast"></param>
    private void OnSessionStateUpdateBroadcast(SessionStateUpdateBroadcast broadcast)
    {
        SessionState = broadcast.SessionState;

        switch (SessionState)
        {
            case SessionState.InLobbyWaitingForVote:
                //SetMapPrefabOptions();
                break;
            case SessionState.InLobbyCountdown:
                break;
            case SessionState.InGame:
                _audioListener.enabled = false;
                break;
        }

        OnSessionStateUpdate.Invoke(broadcast);
    }

    private void SetMapPrefabOptions()
    {
        // Select two random maps from the list of available maps.
        CurrentMapOptions.Clear();

        // Get two unique random indices within the number of available maps.
        while (CurrentMapOptions.Count < 2)
        {
            int randomIndex = UnityEngine.Random.Range(0, AvailableMaps.Count);

            if (CurrentMapOptions.Contains(AvailableMaps[randomIndex]))
            {
                continue;
            }

            CurrentMapOptions.Add(AvailableMaps[randomIndex]);
        }

        MapOptionsBroadcast mapPrefabOptionsBroadcast = new MapOptionsBroadcast()
        {
            MapOptions = CurrentMapOptions
        };

        if (_networkManager.ServerManager.AnyServerStarted())
            _networkManager.ServerManager.Broadcast(mapPrefabOptionsBroadcast);
        
        OnMapOptionsChange.Invoke(mapPrefabOptionsBroadcast);
    }


    // TODO: Combine all these into one broadcast 'GameSettingsUpdateBroadcast' or something.

    /// <summary>
    /// When the game mode is updated on the server, update the game mode on the client.
    /// </summary>
    /// <param name="broadcast"></param>
    private void OnGameModeUpdateBroadcast(GameModeUpdateBroadcast broadcast)
    {
        GameMode = broadcast.GameMode;

        OnGameModeUpdate.Invoke(broadcast);
    }

    /// <summary>
    /// When the selected map index is updated on the server, update the selected map index on the client.
    /// </summary>
    /// <param name="broadcast"></param>
    private void OnMapChangeBroadcast(MapChangeBroadcast broadcast)
    {
        SelectedMapIndex = broadcast.SelectedMapIndex;

        OnMapChange.Invoke(broadcast);
    }

    /// <summary>
    /// When the kill limit is updated on the server, update the kill limit on the client.
    /// </summary>
    /// <param name="broadcast"></param>
    private void OnKillLimitBroadcast(KillLimitBroadcast broadcast)
    {
        KillLimit = broadcast.KillLimit;

        OnKillLimitChange.Invoke(broadcast);
    }

    /// <summary>
    /// When a player votes for an option, update the vote count on the clients.
    /// </summary>
    /// <param name="broadcast"></param>
    private void OnOptionVoteChangeBroadcast(OptionVoteChangeBroadcast broadcast)
    {
        // Update the vote count for the option.
        OnOptionVoteChange.Invoke(broadcast);
    }

    private void OnOptionSelectedBroadcast(OptionSelectedBroadcast broadcast)
    {
        OnOptionSelected.Invoke(broadcast);
    }

    private void OnMapOptionsBroadcast(MapOptionsBroadcast broadcast)
    {
        CurrentMapOptions = broadcast.MapOptions;

        OnMapOptionsChange.Invoke(broadcast);
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


    /// <summary>
    /// When a client votes for a map, set the clients choice for their player and broadcast the vote to all clients.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="broadcast"></param>
    private void OnOptionVoteBroadcastServer(NetworkConnection connection, OptionVoteBroadcast broadcast)
    {
        Debug.Log("Client " + connection.ClientId + " voted for option " + broadcast.OptionVoteIndex + ".");

        if (SessionState == SessionState.InLobbyWaitingForVote)
        {
            // Voting hasn't started and this is the first vote
            if (!_isOffline)
                Players[connection.ClientId].OptionVoteIndex = broadcast.OptionVoteIndex;

            // Initate the voting countdown.
            StartLobbyVotingCountdown();
        }
        else if (SessionState == SessionState.InLobbyVoting)
        {
            // Update the players vote.
            if (!_isOffline)
                Players[connection.ClientId].OptionVoteIndex = broadcast.OptionVoteIndex;
        }
        else
        {
            return;
        }

        if (_isOffline)
        {
            SelectedMapIndex = broadcast.OptionVoteIndex;

            // If everyone has voted, end the voting and start the game.
            StartLobbyCountdown();

            return;
        }


        // Check if everyone has voted.
        List<int> voteCounts = new List<int>() { 0, 0, 0 };
        bool everyoneHasVoted = true;
        foreach (var player in Players.Values)
        {
            if (player.OptionVoteIndex == -1)
            {
                everyoneHasVoted = false;
            }
            else
            {
                voteCounts[player.OptionVoteIndex]++;
            }
        }

        // Broadcast the vote to all clients.
        var optionVoteChangeBroadcast = new OptionVoteChangeBroadcast()
        {
            OptionVotes = voteCounts
        };

        _networkManager.ServerManager.Broadcast(optionVoteChangeBroadcast, false);

        if (!everyoneHasVoted)
        {
            return;
        }

        int highestVoteCount = 0;
        int highestVoteIndex = 0;
        for (int i = 0; i < voteCounts.Count; i++)
        {
            if (voteCounts[i] > highestVoteCount)
            {
                highestVoteCount = voteCounts[i];
                highestVoteIndex = i;
            }
        }

        // Broadcast the highest voted map to all clients.  
        var optionSelectedBroadcast = new OptionSelectedBroadcast()
        {
            OptionIndex = highestVoteIndex
        };
        OnOptionSelected.Invoke(optionSelectedBroadcast);

        SelectedMapIndex = highestVoteIndex;

        _networkManager.ServerManager.Broadcast(optionSelectedBroadcast, false);

        // If everyone has voted, end the voting and start the game.
        StartLobbyCountdown();
    }

    private void OnReturnToLobbyServer(NetworkConnection connection, ReturnToLobbyBroadcast broadcast)
    {
        ReturnToLobby();
    }

    #endregion

    #endregion

    #region Public Methods

    public void SetIsOffline(bool isOffline)
    {
        _isOffline = isOffline;
    }

    /// <summary>
    /// Called by the host to start the game.
    /// </summary>
    public void OnStart()
    {
        // Broadcast to the server to start the game.
        StartGameBroadcast startGameBroadcast = new StartGameBroadcast();
        _networkManager.ClientManager.Broadcast(startGameBroadcast);
    }

    public void OnSelectOption(int index)
    {
        // Broadcast to the server to vote for the selected option.
        OptionVoteBroadcast optionVoteBroadcast = new OptionVoteBroadcast()
        {
            OptionVoteIndex = index,
        };

        _networkManager.ClientManager.Broadcast(optionVoteBroadcast);
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

    public void OnReturnToLobby()
    {
        _networkManager.ClientManager.Broadcast(new ReturnToLobbyBroadcast());
    }

    #endregion

    #region Private Methods

    private void StartLobbyVotingCountdown()
    {
        // Set the session state to InLobbyVoting.
        SessionState = SessionState.InLobbyVoting;

        // Broadcast the session state update to all clients.
        _networkManager.ServerManager.Broadcast(new SessionStateUpdateBroadcast() { SessionState = SessionState.InLobbyVoting, CountdownNumber = _votingCountdownDuration });

        if (_gameStartCountdownCoroutine != null)
        {
            StopCoroutine(_gameStartCountdownCoroutine);
        }

        if (_votingCountdownCoroutine != null)
        {
            StopCoroutine(_votingCountdownCoroutine);
        }

        // Start the countdown.
        _votingCountdownCoroutine = StartCoroutine(StartLobbyVotingCountdownCoroutine());
    }

    private IEnumerator StartLobbyVotingCountdownCoroutine()
    {
        Debug.Log("Voting has started!");
        int countdown = _votingCountdownDuration;
        while (countdown > 0)
        {
            yield return new WaitForSeconds(1);

            countdown--;

            // Broadcast the countdown to all clients.
            _networkManager.ServerManager.Broadcast(new SessionStateUpdateBroadcast() { SessionState = SessionState.InLobbyVoting, CountdownNumber = countdown });
        }

        StartLobbyCountdown();
    }

    private void StartLobbyCountdown()
    {
        Debug.Log("Starting lobby countdown.");

        // Set the session state to InLobbyCountdown.
        SessionState = SessionState.InLobbyCountdown;

        // Broadcast the session state update to all clients.
        _networkManager.ServerManager.Broadcast(new SessionStateUpdateBroadcast() { SessionState = SessionState.InLobbyCountdown, CountdownNumber = _gameStartCountdownDuration });

        if (_votingCountdownCoroutine != null)
        {
            StopCoroutine(_votingCountdownCoroutine);
        }

        // Start the countdown.
        StartCoroutine(LobbyCountdownCoroutine());
    }

    private IEnumerator LobbyCountdownCoroutine()
    {
        int countdown = _gameStartCountdownDuration;

        while (countdown > 0)
        {
            yield return new WaitForSeconds(1);

            countdown--;

            // Broadcast the countdown to all clients.
            _networkManager.ServerManager.Broadcast(new SessionStateUpdateBroadcast() { SessionState = SessionState.InLobbyCountdown, CountdownNumber = countdown });
        }

        StartGame();
    }

    /// <summary>
    /// Called by the server when receiving a StartGameBroadcast.
    /// </summary>
    private void StartGame()
    {
        SceneLoadData gameScene = new SceneLoadData("OnlineGame");

        // Load the game scene based on the selected map.
        //SceneLoadData gameScene = new SceneLoadData(CurrentMapOptions[SelectedMapIndex].Name);

        Debug.Log(CurrentMapOptions[SelectedMapIndex].Name);

        gameScene.ReplaceScenes = ReplaceOption.All;
        _networkManager.SceneManager.LoadGlobalScenes(gameScene);


        SessionState = SessionState.InGame;

        _networkManager.ServerManager.Broadcast(new SessionStateUpdateBroadcast() { SessionState = SessionState.InGame });

        _audioListener.enabled = false;

        UpdateLobbyDetails();
    }

    private async void UpdateLobbyDetails()
    {
        // If the player isn't in a lobby or isn't the host, return.
<<<<<<< HEAD
        if (_beamContext == null || _beamContext.Lobby == null || _beamContext.Lobby.Value == null || _beamContext.Lobby.Host != _beamContext.PlayerId.ToString()) return;
=======
        if (_beamContext.Lobby == null || _beamContext.Lobby.Value == null || _beamContext.Lobby.Host != _beamContext.PlayerId.ToString()) return;
>>>>>>> the-revert-pt2

        var sessionState = SessionState == SessionState.InLobbyWaitingForVote ? "In Lobby" : "In Game";

        var description = _serverInfo.Address + ":" + _serverInfo.Port + ";" +  CurrentMapOptions[SelectedMapIndex].name + ";FFA;" + sessionState;

        SimGameType simGameType = await _simGameTypeRef.Resolve();

        await _beamContext.Lobby.Update(Lobby.lobbyId, Lobby.Restriction, Lobby.host, Lobby.name, description, simGameType.Id, 8);

        await _beamContext.Lobby.Refresh();
    }

    /// <summary>
    /// Called by the server after post game routine is complete.
    /// </summary>
    private void ReturnToLobby()
    {
        // For each player, despawn their nob.
        foreach (var player in Players.Values)
        {
            if (player.Nob == null) continue;

            player.Nob.Despawn();
            player.Nob = null;

            // Reset the player's voting
            player.OptionVoteIndex = -1;
        }

        SceneLoadData preGameLobbyScene = new SceneLoadData("PreGameLobby");
        preGameLobbyScene.ReplaceScenes = ReplaceOption.All;
        _networkManager.SceneManager.LoadGlobalScenes(preGameLobbyScene);


        SessionState = SessionState.InLobbyWaitingForVote;

        _networkManager.ServerManager.Broadcast(new SessionStateUpdateBroadcast() { SessionState = SessionState.InLobbyWaitingForVote });

        SetMapPrefabOptions();

        UpdateLobbyDetails();
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

// TODO: Switch this to a PlayerUpdateBroadcast. It will be used for both updating usernames and voting, and maybe other things.
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
    public int CountdownNumber;
}

public struct OptionVoteChangeBroadcast : IBroadcast
{
    public List<int> OptionVotes;
}

public struct OptionSelectedBroadcast : IBroadcast
{
    public int OptionIndex; 
}

public struct MapChangeBroadcast : IBroadcast
{
    public int SelectedMapIndex;
}

public struct KillLimitBroadcast : IBroadcast
{
    public int KillLimit;
}

public struct OptionVoteBroadcast : IBroadcast
{
    public int OptionVoteIndex;
    public int PlayerId;
}

public struct OptionsVoteUpdateBroadcast : IBroadcast
{
    public List<int> OptionVotes;
}

public struct MapOptionsBroadcast : IBroadcast
{
    public List<MapInfo> MapOptions;
}

public struct ReturnToLobbyBroadcast : IBroadcast { }

public enum SessionState
{
    InLobbyWaitingForVote,
    InLobbyVoting,
    InLobbyCountdown,
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
