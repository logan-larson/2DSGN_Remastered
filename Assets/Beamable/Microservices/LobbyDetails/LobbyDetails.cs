using Beamable.Experimental.Api.Lobbies;
using Beamable.Server;
using HathoraCloud;
using HathoraCloud.Models.Operations;
using HathoraCloud.Models.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beamable.Microservices
{
	[Microservice("LobbyDetails")]
	public class LobbyDetails : Microservice
	{
        private string _appId;
        private string _devToken;

		[ClientCallable]
		public async Task<string> CreateLobby(string lobbyId, bool isPublic, string lobbyName, string gamemode)
		{
			// Request a new room from Hathora
            var hathora = new HathoraCloudSDK(
                security: new Security()
                {
                    HathoraDevToken = _devToken,
                },
                appId: _appId
            );

            // - Create the room
            CreateRoomRequest request = new CreateRoomRequest()
            {
                AppId = _appId,
                CreateRoomParams = new CreateRoomParams()
                {
                    Region = Region.Chicago,
                    RoomConfig = "{\"name\":\"Test Room\"}",
                },
            };

            CreateRoomResponse res = await hathora.RoomV2.CreateRoomAsync(request);

            // - Check if the room is ready
            if (res.ConnectionInfoV2.Status == ConnectionInfoV2Status.Starting)
            {
                // Start polling for the room to be ready
                uint maxPolls = 10;
                uint polls = 0;

                bool isConnectionActive = false;

                while (true)
                {
                    // Poll for the room to be ready
                    GetConnectionInfoResponse roomRes = await hathora.RoomV2.GetConnectionInfoAsync(new GetConnectionInfoRequest()
                    {
                        AppId = _appId,
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
                    return null;
                }
            }
            else
            {
                // Room failed to start
                return null;
            }

            // Create a new lobby in the database with the room's connection details


            // Return the lobby id to the client
            //return lobbyId;

            // TEMP: Return the room's connection info to the client
            return res.ConnectionInfoV2.ExposedPort.Host + ":" + res.ConnectionInfoV2.ExposedPort.Port;
		}

        [ClientCallable]
        public void GetLobbyConnectionDetails(string lobbyId)
        {
            // Return the connection details for the Hathora room based on the Beamable lobby ID

        }
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
    };

}
