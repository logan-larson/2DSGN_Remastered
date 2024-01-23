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

    public bool IsDead = false;

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

    [SerializeField]
    private GameObject _jumpPredictionLine;

    [SerializeField]
    private GameObject _crosshair;

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Set the username UI.
        _usernameText.color = base.IsOwner ? Color.green : Color.red;

        if (base.IsOwner)
        {
            // Set the health UI.
            InitializeServerRpc(base.LocalConnection, _userInfo.Username);
        }
        else
        {
            // Disable the jump prediction line.
            _jumpPredictionLine.SetActive(false);
            
            // Disable the crosshair.
            _crosshair.SetActive(false);
        }
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

    [Server]
    public void OnDeath(Transform heaven, NetworkConnection targetConn, NetworkObject killer)
    {
        IsDead = true;

        transform.position = heaven.position;
        transform.rotation = heaven.rotation;

        if (Camera.main.TryGetComponent(out CameraController cameraController))
        {
            cameraController.SetPlayer(killer.transform);
        }

        SetPlayerToFollowTargetRpc(targetConn, killer);
    }

    [Server]
    public void OnRespawn(Transform spawnPoint, Player player)
    {

        // Set the player's position to the spawn position.
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        IsDead = false;

        if (Camera.main.TryGetComponent(out CameraController cameraController))
        {
            cameraController.ResetToLocal();
        }

        SetPlayerToFollowTargetRpc(player.Connection, player.Nob);
    }

    [TargetRpc]
    public void SetPlayerToFollowTargetRpc(NetworkConnection conn, NetworkObject target)
    {
        if (Camera.main.TryGetComponent(out CameraController cameraController))
        {
            cameraController.SetPlayer(target.transform);
        }
    }

}
