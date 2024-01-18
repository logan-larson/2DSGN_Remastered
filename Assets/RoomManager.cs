using HathoraCloud;
using HathoraCloud.Models.Shared;
using HathoraCloud.Models.Operations;
using UnityEngine;
using Hathora.Core.Scripts.Runtime.Server;

public class RoomManager : MonoBehaviour
{
    [SerializeField]
    private HathoraServerConfig _serverConfig;

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

    public void OnHost()
    {
        // Create the room
        CreateRoomRequest request = new CreateRoomRequest()
        {
            CreateRoomParams = new CreateRoomParams()
            {
                Region = Region.Chicago,
                RoomConfig = "{\"name\":\"Test Room\"}",

            },
            RoomId = "test-room",
        };

        using (var res = hathora.RoomV2.CreateRoomAsync(request))
        {
            // TODO: Handle response
            Debug.Log(res.Result.ConnectionInfoV2.Status);
        }
    }
}
