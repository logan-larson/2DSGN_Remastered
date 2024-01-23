using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{

    [SerializeField]
    private UserInfo _userInfo;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner)
            return;

        SetUserNameServerRpc(base.LocalConnection, _userInfo.Username);
    }

    [ServerRpc]
    private void SetUserNameServerRpc(NetworkConnection conn, string username)
    {
        // Set the name of the player in the PlayersManager.
        PlayersManager.Instance.SetUsername(conn, username);
    }

}
