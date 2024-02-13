using HathoraCloud;
using HathoraCloud.Models.Shared;
using HathoraCloud.Models.Operations;
using UnityEngine;
using Hathora.Core.Scripts.Runtime.Server;
using TMPro;
using System.Collections;
using System.Threading.Tasks;
using System.Text;
using UnityEngine.Networking;

public class RoomUIManager : MonoBehaviour
{
    [SerializeField]
    private HathoraServerConfig _serverConfig;

    [SerializeField]
    private TMP_InputField _serverAddressInput;

    [SerializeField]
    private TMP_InputField _serverPortInput;

    [SerializeField]
    private TMP_Text _joinInputErrorText;

    [SerializeField]
    private TMP_Text _serverAddressText;

    [SerializeField]
    private TMP_Text _serverPortText;

    [SerializeField]
    private TMP_Text _serverStatusText;

    [SerializeField]
    private ServerInfo _serverInfo;

    [SerializeField]
    private TMP_Text _usernameText;

    [SerializeField]
    private UserInfo _userInfo;

    private HathoraCloudSDK hathora;

    private void Start()
    {
        hathora = new HathoraCloudSDK(
            security: new HathoraCloud.Models.Shared.Security()
            {
                HathoraDevToken = _serverConfig.HathoraCoreOpts.DevAuthOpts.HathoraDevToken,
            },
            appId: _serverConfig.HathoraCoreOpts.AppId
        );

        _joinInputErrorText.gameObject.SetActive(false);

        _usernameText.text = _userInfo.Username;
    }

    public void OnHost()
    {
        // Request a room from my Go server
        StartCoroutine(CreateRoomCoroutine("{\"name\":\"Test Room\", \"hostUsername\":\"\"}", "Chicago"));


        /*
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

        CreateRoomResponse res = await hathora.RoomV2.CreateRoomAsync(request);

        if (res.ConnectionInfoV2.Status == ConnectionInfoV2Status.Starting)
        {
            _serverStatusText.text = "Room is starting...";

            // Start polling for the room to be ready
            StartCoroutine(PollConnectionInfoCoroutine(res.ConnectionInfoV2.RoomId));
        }
        */
    }

    private IEnumerator CreateRoomCoroutine(string roomConfig, string region)
    {
        var requestBody = new
        {
            roomConfig,
            region,
        };

        string jsonBody = JsonUtility.ToJson(requestBody);
        byte[] jsonBodyBytes = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest www = new UnityWebRequest("http://localhost:8080/lobbies", "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonBodyBytes);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                Debug.Log("Room created");
            }
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
        GetConnectionInfoResponse roomRes = await hathora.RoomV2.GetConnectionInfoAsync(new GetConnectionInfoRequest()
        {
            AppId = _serverConfig.HathoraCoreOpts.AppId,
            RoomId = roomId,
        });

        if (roomRes.ConnectionInfoV2.Status == ConnectionInfoV2Status.Active)
        {
            // Update UI
            _serverStatusText.text = "Room is ready";
            _serverAddressText.text = roomRes.ConnectionInfoV2.ExposedPort.Host;
            _serverPortText.text = roomRes.ConnectionInfoV2.ExposedPort.Port.ToString();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void OnJoin()
    {
        _joinInputErrorText.gameObject.SetActive(false);

        if (string.IsNullOrEmpty(_serverAddressInput.text) || string.IsNullOrEmpty(_serverPortInput.text))
        {
            Debug.LogError("Server address or port is empty");
            _joinInputErrorText.gameObject.SetActive(true);
            return;
        }

        if (ushort.TryParse(_serverPortInput.text, out ushort port) == false)
        {
            Debug.LogError("Server port is not a number");
            _joinInputErrorText.gameObject.SetActive(true);
            return;
        }

        // Join the room
        _serverInfo.Address = _serverAddressInput.text;
        _serverInfo.Port = ushort.Parse(_serverPortInput.text);


        // TEMP: Switching to test
        UnityEngine.SceneManagement.SceneManager.LoadScene("PreGameLobby");
        //UnityEngine.SceneManagement.SceneManager.LoadScene("OnlineGame");
    }

    public void OnMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
