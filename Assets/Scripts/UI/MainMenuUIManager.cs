using Beamable;
//using Beamable.Api.Autogenerated.Models;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Experimental.Api.Lobbies;
using Hathora.Core.Scripts.Runtime.Server;
using HathoraCloud;
using HathoraCloud.Models.Operations;
using HathoraCloud.Models.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MainMenuUIManager : MonoBehaviour
{
    #region Public Events

    public UnityEvent<List<Beamable.Experimental.Api.Lobbies.Lobby>> OnLobbyListUpdated = new UnityEvent<List<Beamable.Experimental.Api.Lobbies.Lobby>>();

    #endregion

    public List<Beamable.Experimental.Api.Lobbies.Lobby> Lobbies = new List<Beamable.Experimental.Api.Lobbies.Lobby>();

    #region Object References

    public TMP_Text UsernameText;

    [SerializeField]
    private TMP_InputField _lobbyPasscodeInputField;

    [SerializeField]
    private TMP_Text _serverStatusText;

    #endregion

    #region Scriptable Object References

    [SerializeField]
    private BuildInfo _buildInfo;

    [SerializeField]
    private ServerInfo _serverInfo;

    [SerializeField]
    private UserInfo _userInfo;

    [SerializeField]
    private SimGameTypeRef _simGameTypeRef;

    [SerializeField]
    private HathoraServerConfig _serverConfig;

    #endregion

    #region Lobby Parameters

    [SerializeField]
    private TMP_InputField _lobbyName;

    [SerializeField]
    private TMP_Dropdown _publicPrivateDropdown;

    [SerializeField]
    private TMP_Dropdown _gamemodeDropdown;

    #endregion

    #region Private

    private HathoraCloudSDK _hathora;

    private BeamContext _beamContext;

    #endregion

    #region Initialization

    private void Start()
    {
        _buildInfo.IsFreeplay = false;

        UsernameText.text = "Welcome back, " + _userInfo.Username;

        SetupHathora();

        SetupBeamable();
    }

    private void SetupHathora()
    {
        // TEMP: Hardcoded dev token and app id
        var devToken = "xLSJxvOK2VCYUxaVGmV9zc_VO3KHQQOGCRZlDHOEzXXNJ";
        var appId = "app-b330c055-60e2-4bdf-9427-9c9e900eb48f";

        _hathora = new HathoraCloudSDK(
            security: new HathoraCloud.Models.Shared.Security()
            {
                //HathoraDevToken = _serverConfig.HathoraCoreOpts.DevAuthOpts.HathoraDevToken,
                HathoraDevToken = devToken,
            },
            //appId: _serverConfig.HathoraCoreOpts.AppId
            appId: appId
        );
    }

    private async void SetupBeamable()
    {
        _beamContext = BeamContext.Default;
        await _beamContext.OnReady;
        _beamContext.Lobby.OnDataUpdated += OnLobbyDataUpdated;

        OnRefreshLobbyList();
    }

    #endregion

    #region Private Events

    private void OnLobbyDataUpdated(Beamable.Experimental.Api.Lobbies.Lobby lobby)
    {
        if (lobby == null) return;

        Debug.Log($"Lobby updated: {lobby.name}, Description: {lobby.description}, Players: {lobby.players.Count}/{lobby.maxPlayers}");
        // Otherwise set the server info to the lobby's server info
    }

    #endregion

    #region Public Events

    public async void OnRefreshLobbyList()
    {
        Lobbies = (await _beamContext.Lobby.FindLobbies()).results;

        OnLobbyListUpdated.Invoke(Lobbies);
    }

    public void OnJoinLobbyByPasscode()
    {
        var passcode = _lobbyPasscodeInputField.text;

        JoinLobbyAsync("", passcode);
    }

    public async void OnCreateAndJoinLobby()
    {
        // Check that the host's input is valid
        if (string.IsNullOrEmpty(_lobbyName.text))
        {
            // TODO: Show an error message
            Debug.LogError("Lobby name is required");
            return;
        }

        SimGameType simGameType = await _simGameTypeRef.Resolve();

        // Set the rooms config based on the host's input
        CreateLobbyRecord lobbyRecord = new CreateLobbyRecord
        {
            Name = _lobbyName.text,
            Restriction = _publicPrivateDropdown.options[_publicPrivateDropdown.value].text == "Public" ? LobbyRestriction.Open : LobbyRestriction.Closed,
            GameTypeId = simGameType.Id,
            Description = "Test Lobby",
            PlayerTags = new List<Tag>(),
            MaxPlayers = 8,
            PasscodeLength = 6,
            Gamemode = _gamemodeDropdown.options[_gamemodeDropdown.value].text,
        };

        // Create the lobby and get its ID
        await CreateLobbyAsync(lobbyRecord);

        _serverStatusText.text = "Joining lobby...";

        JoinLobbyAsync(_beamContext.Lobby.Id, _beamContext.Lobby.Passcode);
    }

    public void OpenFreeplayGame()
    {
        // Set the server info to localhost and port 7770
        _serverInfo.IsFreeplay = true;
        _serverInfo.Address = "localhost";
        _serverInfo.Port = 7770;

        // Load the freeplay scene
        SceneManager.LoadScene("PreGameLobby");
    }

    public void QuitGame()
    {
        UnityEngine.Application.Quit();
    }

    #endregion

    public async Promise<CreateLobbyDetails> CreateLobbyAsync(CreateLobbyRecord lobbyRecord)
    {
        // TEMP: Create the Hathora room
        CreateRoomRequest request = new CreateRoomRequest()
        {
            AppId = _serverConfig.HathoraCoreOpts.AppId,
            CreateRoomParams = new CreateRoomParams()
            {
                Region = Region.Chicago,
                RoomConfig = "{\"name\":\"Test Room\"}", // This should contain the game mode, arena, and other settings
            },
        };

        _serverStatusText.text = "Creating room...";

        CreateRoomResponse res = await _hathora.RoomV2.CreateRoomAsync(request);

        // - Check if the room is ready
        GetConnectionInfoResponse roomRes = null;
        if (res.ConnectionInfoV2.Status == ConnectionInfoV2Status.Starting)
        {
            // Start polling for the room to be ready
            uint maxPolls = 10;
            uint polls = 0;

            bool isConnectionActive = false;

            while (true)
            {
                _serverStatusText.text = "Waiting for room to start..." + polls;

                // Poll for the room to be ready
                roomRes = await _hathora.RoomV2.GetConnectionInfoAsync(new GetConnectionInfoRequest()
                {
                    AppId = _serverConfig.HathoraCoreOpts.AppId,
                    RoomId = res.ConnectionInfoV2.RoomId,
                });

                if (roomRes.ConnectionInfoV2.Status == ConnectionInfoV2Status.Active)
                {
                    isConnectionActive = true;
                    break;
                }

                if (polls >= maxPolls)
                {
                    break;
                }

                polls++;
                await Task.Delay(1000);
            }

            if (!isConnectionActive)
            {
                // Room failed to start
                _serverStatusText.text = "Room failed to start";
                return null;
            }
        }
        else
        {
            // Room failed to start
            _serverStatusText.text = "Room failed to start";
            return null;
        }

        // TODO: Don't hardcode this
        var description = $"{roomRes.ConnectionInfoV2.ExposedPort.Host}:{roomRes.ConnectionInfoV2.ExposedPort.Port};BigMap;{lobbyRecord.Gamemode};In Lobby;0";


        // TEMP: Get the Hathora room connection details
        _serverStatusText.text = "Room is active, creating Beamable lobby...";

        // Create the lobby in Beamable and TEMP: store the Hathora room connection details in the lobby description
        await _beamContext.Lobby.Create(lobbyRecord.Name, lobbyRecord.Restriction, lobbyRecord.GameTypeId,
            description, lobbyRecord.PlayerTags, lobbyRecord.MaxPlayers, lobbyRecord.PasscodeLength);


        var createLobbyDetails = new CreateLobbyDetails()
        {
            LobbyId = _beamContext.Lobby.Id,
            LobbyPasscode = _beamContext.Lobby.Passcode
        };

        return createLobbyDetails;
    }

    public async void JoinLobbyAsync(string lobbyId, string passcode = null)
    {
        // Join the lobby in Beamable
        if (passcode == null)
        {
            await _beamContext.Lobby.Join(lobbyId);
        }
        else
        {
            await _beamContext.Lobby.JoinByPasscode(passcode.ToUpper());
        }


        var connectionDetails = _beamContext.Lobby.Description.Split(';')[0].Split(":");
        JoinLobby(connectionDetails[0], connectionDetails[1]);
    }

    private void JoinLobby(string serverAddress, string serverPort)
    {

        if (ushort.TryParse(serverPort, out ushort port) == false)
        {
            return;
        }

        // Join the room
        _serverInfo.Address = serverAddress;
        _serverInfo.Port = ushort.Parse(serverPort);

        SceneManager.LoadScene("PreGameLobby");
    }

    #region Classes and Records

    public class CreateLobbyDetails
    {
        public string LobbyId;
        public string LobbyPasscode;
    }

    public record CreateLobbyRecord
    {
        public string Name { get; set; }
        public LobbyRestriction Restriction { get; set; }
        public string GameTypeId { get; set; }
        public string Description { get; set; }
        public List<Tag> PlayerTags { get; set; }
        public int? MaxPlayers { get; set; }
        public int? PasscodeLength { get; set; }
        public string Gamemode { get; set; }
    };

    #endregion

}
