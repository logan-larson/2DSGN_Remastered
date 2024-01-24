using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UsernameUI : NetworkBehaviour
{

    [SyncVar (OnChange = nameof(OnUsernameChanged))]
    private string _username;

    private void OnUsernameChanged(string oldValue, string newValue, bool asServer)
    {
        _usernameText.text = newValue;
    }

    #region Serialized Fields

    [Header("Serialized Fields")]

    [SerializeField]
    private TMP_Text _usernameText;

    [SerializeField]
    private UserInfo _userInfo;

    #endregion

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner)
        {
            _usernameText.color = Color.red;
            return;
        }

        _usernameText.color = Color.green;

        SetUsernameTextServerRpc(_userInfo.Username);
    }

    [ServerRpc]
    private void SetUsernameTextServerRpc(string username)
    {
        _username = username;
        //_usernameText.text = username;
        //SetUsernameTextObserversRpc(username);
    }

    [ObserversRpc]
    private void SetUsernameTextObserversRpc(string username)
    {
        //_usernameText.text = username;
    }
}
