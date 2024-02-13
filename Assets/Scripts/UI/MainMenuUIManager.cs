using Hathora.Core.Scripts.Runtime.Server;
using HathoraCloud;
using HathoraCloud.Models.Operations;
using HathoraCloud.Models.Shared;
using System.Collections;
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
    private HathoraServerConfig _serverConfig;

    private HathoraCloudSDK _hathora;

    private void Start()
    {
        BaseMenu.SetActive(true);
        HostMenu.SetActive(false);
        JoinMenu.SetActive(false);

        _buildInfo.IsFreeplay = false;

        UsernameText.text = "Welcome back, " + _userInfo.Username;

        _hathora = new HathoraCloudSDK(
            security: new HathoraCloud.Models.Shared.Security()
            {
                HathoraDevToken = _serverConfig.HathoraCoreOpts.DevAuthOpts.HathoraDevToken,
            },
            appId: _serverConfig.HathoraCoreOpts.AppId
        );

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
    
    public async Task CreateAndJoinLobbyAsync()
    {
        // Set the rooms config based on the host's input

        // Create the room
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
