using HathoraCloud;
using HathoraCloud.Models.Shared;
using HathoraCloud.Models.Operations;
using UnityEngine;
using Hathora.Core.Scripts.Runtime.Server;
using TMPro;
using System.Collections;
using System.Threading.Tasks;

public class RoomManager : MonoBehaviour
{
    [SerializeField]
    private HathoraServerConfig _serverConfig;

    [SerializeField]
    private TMP_InputField _serverAddressInput;

    [SerializeField]
    private TMP_InputField _serverPortInput;

    [SerializeField]
    private TMP_Text _serverAddressText;

    [SerializeField]
    private TMP_Text _serverPortText;

    [SerializeField]
    private TMP_Text _serverStatusText;

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

    }

    public async void OnHost()
    {
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
        if (string.IsNullOrEmpty(_serverAddressInput.text) || string.IsNullOrEmpty(_serverPortInput.text))
        {
            Debug.LogError("Server address or port is empty");
            return;
        }

        // Join the room

    }

    public void OnMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
