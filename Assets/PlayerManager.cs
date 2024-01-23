using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{

    [SerializeField]
    private UserInfo _userInfo;

    [SerializeField]
    private TMP_Text _usernameText;

    [SyncVar (OnChange = nameof(OnUsernameChanged))]
    private string _username;

    private void OnUsernameChanged(string oldValue, string newValue, bool asServer)
    {
        _usernameText.text = newValue;
    }

    [SerializeField]
    private TMP_Text _healthText;

    [SyncVar (OnChange = nameof(OnHealthChanged))]
    private float _health = 100f;

    private void OnHealthChanged(float oldValue, float newValue, bool asServer)
    {
        // Update the health UI.
        _healthText.text = newValue.ToString();
    }


    public override void OnStartClient()
    {
        base.OnStartClient();

        // Set the username UI.
        _usernameText.color = base.IsOwner ? Color.green : Color.red;

        if (!base.IsOwner) return;

        // Set the health UI.
        InitializeServerRpc(base.LocalConnection, _userInfo.Username);
    }

    [ServerRpc]
    private void InitializeServerRpc(NetworkConnection conn, string username)
    {
        _username = username;

        _health = 100f;

        OnHealthChanged(_health, _health, true);

        PlayersManager.Instance.SetUsername(conn, username);
    }

    [Server]
    public void TakeDamage(float damage, float newHealth)
    {
        // Take damage.
        _health = newHealth;

        // Spawn damage indicator.

        // Spawn hit particles.

        // Play hit sound based on health remaining.
    }

}
