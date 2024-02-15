using Beamable;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Experimental.Api.Lobbies;
using Beamable.Server.Clients;
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
using UnityEngine.SceneManagement;

public class MainMenuUIManager : MonoBehaviour
{
    public GameObject BaseMenu;
    public GameObject HostMenu;
    public GameObject JoinMenu;

    public TMP_Text UsernameText;

    [SerializeField]
    private TMP_Text _serverStatusText;

    [SerializeField]
    private BuildInfo _buildInfo;

    [SerializeField]
    private ServerInfo _serverInfo;

    [SerializeField]
    private UserInfo _userInfo;

    [SerializeField]
    private SimGameTypeRef _simGameTypeRef;

    #region Lobby Parameters

    [SerializeField]
    private TMP_InputField _lobbyName;

    [SerializeField]
    private TMP_Dropdown _publicPrivateDropdown;

    [SerializeField]
    private TMP_Dropdown _gamemodeDropdown;

    #endregion

    [SerializeField]
    private HathoraServerConfig _serverConfig;

    private HathoraCloudSDK _hathora;

    private BeamContext _beamContext;

    private LobbyDetailsClient _lobbyDetailsClient;

    public string PlayerId { get; private set; }

    private void Start()
    {
        BaseMenu.SetActive(true);
        HostMenu.SetActive(true);
        JoinMenu.SetActive(true);

        _buildInfo.IsFreeplay = false;

        UsernameText.text = "Welcome back, " + _userInfo.Username;

        _hathora = new HathoraCloudSDK(
            security: new HathoraCloud.Models.Shared.Security()
            {
                HathoraDevToken = _serverConfig.HathoraCoreOpts.DevAuthOpts.HathoraDevToken,
            },
            appId: _serverConfig.HathoraCoreOpts.AppId
        );

        SetupBeamable();
    }

    private async void SetupBeamable()
    {
        _beamContext = BeamContext.Default;
        await _beamContext.OnReady;
        _beamContext.Lobby.OnDataUpdated += OnLobbyDataUpdated;

        _lobbyDetailsClient = _beamContext.Microservices().LobbyDetails();

        PlayerId = _beamContext.PlayerId.ToString();
    }

    private void OnLobbyDataUpdated(Beamable.Experimental.Api.Lobbies.Lobby lobby)
    {
        if (lobby == null) return;

        // Otherwise set the server info to the lobby's server info
    }

    public void OpenHostMenu()
    {
        HostMenu.SetActive(true);
    }

    public void CloseHostMenu()
    {
        HostMenu.SetActive(false);
    }

    public void OpenJoinMenu()
    {
        JoinMenu.SetActive(true);
    }

    public void CloseJoinMenu()
    {
        JoinMenu.SetActive(false);
    }

    public void OpenLobbyShell()
    {
        // Load the lobby scene
        SceneManager.LoadScene("LobbyShell");
    }

    public void OpenFreeplayGame()
    {
        // Set the server info to localhost and port 7770
        _buildInfo.IsFreeplay = true;

        // Load the freeplay scene
        SceneManager.LoadScene("PreGameLobby");
    }

    public void QuitGame()
    {
        UnityEngine.Application.Quit();
    }

    public void OnCreateAndJoinLobby()
    {
        CreateAndJoinLobbyAsync();
    }
    
    public async Promise<string> CreateLobbyAsync(CreateLobbyRecord lobbyRecord)
    {
        // TEMP: Create the Hathora room
        CreateRoomRequest request = new CreateRoomRequest()
        {
            AppId = _serverConfig.HathoraCoreOpts.AppId,
            CreateRoomParams = new CreateRoomParams()
            {
                Region = Region.Chicago,
                RoomConfig = "{\"name\":\"Test Room\"}",
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

        var description = roomRes.ConnectionInfoV2.ExposedPort.Host + ":" + roomRes.ConnectionInfoV2.ExposedPort.Port;
        

        // TEMP: Get the Hathora room connection details
        _serverStatusText.text = "Room is active, creating Beamable lobby...";

        // Create the lobby in Beamable and TEMP: store the Hathora room connection details in the lobby description
        await _beamContext.Lobby.Create(lobbyRecord.Name, lobbyRecord.Restriction, lobbyRecord.GameTypeId,
            description, lobbyRecord.PlayerTags, lobbyRecord.MaxPlayers, lobbyRecord.PasscodeLength);

        // Get the lobby ID and call the create lobby microservice
        string lobbyId = _beamContext.Lobby.Id;

        // Call the lobby microservice to create the Hathora room and store the lobby details
        //var lobbyDetails = await _lobbyDetailsClient.CreateLobby(lobbyId, lobbyRecord.Restriction == LobbyRestriction.Open, lobbyRecord.Name, lobbyRecord.Gamemode);

        return lobbyId;
    }

    public async Task JoinLobbyAsync(string lobbyId)
    {
        // Get the connection details for the Hathora room based on the Beamable lobby ID

        // Call the lobby microservice to get the room details


        // TEMP: Get the connection details for the Hathora room from the lobby description
        var lqr = await _beamContext.Lobby.FindLobbies();

        // Join the lobby in Beamable
        // TODO: Move this to the PreGameLobby scene
        await _beamContext.Lobby.Join(lobbyId);

        lqr.results.ForEach(lobby =>
        {
            if (lobby.lobbyId == lobbyId)
            {
                var connectionDetails = lobby.description.Split(':');
                JoinLobby(connectionDetails[0], connectionDetails[1]);
            }
        });
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

    public async void CreateAndJoinLobbyAsync()
    {
        // Check that the host's input is valid
        if (string.IsNullOrEmpty(_lobbyName.text))
        {
            // TODO: Show an error message
            Debug.LogError("Lobby name is required");
            return;
        }
        
        Debug.Log("Creating and joining lobby: " + _lobbyName.text);

        SimGameType simGameType = await _simGameTypeRef.Resolve();

        // Set the rooms config based on the host's input
        CreateLobbyRecord lobbyRecord = new CreateLobbyRecord
        {
            Name = _lobbyName.text,
            Restriction = _publicPrivateDropdown.options[_publicPrivateDropdown.value].text == "Public" ? LobbyRestriction.Open : LobbyRestriction.Closed,
            GameTypeId = simGameType.Id,
            Description = "Test Lobby",
            PlayerTags = new List<Tag>(),
            MaxPlayers = 9,
            PasscodeLength = 6,
            Gamemode = _gamemodeDropdown.options[_gamemodeDropdown.value].text,
        };

        // Create the lobby and get its ID
        var lobbyId = await CreateLobbyAsync(lobbyRecord);

        _serverStatusText.text = "Joining lobby...";

        // Join the lobby by its ID
        await JoinLobbyAsync(lobbyId);

        // This is now getting handled by the LobbyDetails microservice
        // Create the room
        /*
        CreateRoomRequest request = new CreateRoomRequest()
        {
            AppId = _serverConfig.HathoraCoreOpts.AppId,
            CreateRoomParams = new CreateRoomParams()
            {
                Region = Region.Chicago,
                RoomConfig = "{\"name\":\"Test Room\"}",
            },
        };

        _serverStatusText.text = "Creating room...";

        CreateRoomResponse res = await _hathora.RoomV2.CreateRoomAsync(request);

        if (res.ConnectionInfoV2.Status == ConnectionInfoV2Status.Starting)
        {
            _serverStatusText.text = "Room is starting...";

            // Start polling for the room to be ready
            StartCoroutine(PollConnectionInfoCoroutine(res.ConnectionInfoV2.RoomId));
        }
        */
    }

    private IEnumerator PollConnectionInfoCoroutine(string roomId)
    {
        uint maxPolls = 10;
        uint polls = 0;
        while (true)
        {
            Task<bool> isConnectionActiveTask = IsConnectionActive(roomId);

            yield return new WaitUntil(() => isConnectionActiveTask.IsCompleted);

            if (isConnectionActiveTask.Result)
            {
                break;
            }

            if (polls >= maxPolls)
            {
                _serverStatusText.text = "Room failed to start";
                Debug.LogError($"Room '{roomId}' failed to start");
                break;
            }

            polls++;
            yield return new WaitForSeconds(1);
        }
    }

    private async Task<bool> IsConnectionActive(string roomId)
    {
        // Poll for the room to be ready
        GetConnectionInfoResponse roomRes = await _hathora.RoomV2.GetConnectionInfoAsync(new GetConnectionInfoRequest()
        {
            AppId = _serverConfig.HathoraCoreOpts.AppId,
            RoomId = roomId,
        });

        if (roomRes.ConnectionInfoV2.Status == ConnectionInfoV2Status.Active)
        {
            // Update UI
            _serverStatusText.text = "Room is ready";
            var address = roomRes.ConnectionInfoV2.ExposedPort.Host;
            var port = roomRes.ConnectionInfoV2.ExposedPort.Port.ToString();

            JoinLobby(address, port);

            return true;
        }
        else
        {
            return false;
        }
    }

    private void JoinLobby(string serverAddress, string serverPort)
    {
        //_joinInputErrorText.gameObject.SetActive(false);

        /*
        if (string.IsNullOrEmpty(_serverAddressInput.text) || string.IsNullOrEmpty(_serverPortInput.text))
        {
            Debug.LogError("Server address or port is empty");
            _joinInputErrorText.gameObject.SetActive(true);
            return;
        }
        */

        if (ushort.TryParse(serverPort, out ushort port) == false)
        {
            //Debug.LogError("Server port is not a number");
            //_joinInputErrorText.gameObject.SetActive(true);
            return;
        }

        // Join the room
        _serverInfo.Address = serverAddress;
        _serverInfo.Port = ushort.Parse(serverPort);


        // TEMP: Switching to test
        SceneManager.LoadScene("PreGameLobby");
        //UnityEngine.SceneManagement.SceneManager.LoadScene("OnlineGame");
    }
}
