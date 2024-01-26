using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : NetworkBehaviour
{

    [SerializeField]
    private UserInfo _userInfo;

    [SyncVar (OnChange = nameof(OnUsernameChanged))]
    private string _username;

    public bool IsDead = false;

    private void OnUsernameChanged(string oldValue, string newValue, bool asServer)
    {
        _usernameText.text = newValue;
    }

    [SyncVar (OnChange = nameof(OnHealthChanged))]
    private float _health = 100f;

    private void OnHealthChanged(float oldValue, float newValue, bool asServer)
    {
    }

    #region Constants

    const float MAX_HEALTH = 100f;

    #endregion

    #region Object References

    [SerializeField]
    private Image _redDamagedSprite;

    [SerializeField]
    private Image _whiteDamagedSprite;

    [SerializeField]
    private TMP_Text _usernameText;

    [SerializeField]
    private GameObject _jumpPredictionLine;

    [SerializeField]
    private GameObject _crosshair;

    [SerializeField]
    private GameObject _damageIndicatorPrefab;

    [SerializeField]
    private GameObject _deathIndicatorPrefab;

    #endregion

    #region Script References

    [SerializeField]
    private PlayerController _playerController;

    [SerializeField]
    private ModeManager _modeManager;

    [SerializeField]
    private WeaponManager _weaponManager;

    #endregion

    #region Private Fields

    private bool _subscribedToTimeManager = false;

    #endregion

    #region Time Management

    private void SubscribeToTimeManager(bool subscribe)
    {
        if (base.TimeManager == null)
            return;

        if (subscribe == _subscribedToTimeManager)
            return;

        _subscribedToTimeManager = subscribe;

        if (subscribe)
        {
            base.TimeManager.OnTick += OnTick;
        }
        else
        {
            base.TimeManager.OnTick -= OnTick;
        }
    }

    #endregion

    #region Initialization

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Set the username UI.
        _usernameText.color = base.IsOwner ? Color.green : Color.red;

        _modeManager.OnModeChanged.AddListener(OnModeChanged);

        OnModeChanged(_modeManager.CurrentMode);

        SubscribeToTimeManager(true);

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

        OnModeChanged(_modeManager.CurrentMode);

        OnHealthChanged(_health, _health, true);

        PlayersManager.Instance.SetUsername(conn, username);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        SubscribeToTimeManager(false);
    }

    #endregion

    #region Frame Updates

    private void OnTick()
    {
        // Red damaged sprite fill should lerp to MAX_HEALTH - _health.
        _redDamagedSprite.fillAmount = Mathf.Lerp(_redDamagedSprite.fillAmount, (MAX_HEALTH - _health) / 100f, 0.1f);

        // White damaged sprite should be equal to MAX_HEALTH - _health.
        _whiteDamagedSprite.fillAmount = (MAX_HEALTH - _health) / 100f;
    }

    #endregion

    #region Player State Events

    [Server]
    public void TakeDamage(float damage, float newHealth)
    {
        // Take damage.

        _health = newHealth;

        // Spawn damage indicator.
        var damageIndicator = Instantiate(_damageIndicatorPrefab, transform.position, Quaternion.identity);
        damageIndicator.GetComponent<DamageIndicatorManager>().Initialize((int)damage, newHealth);

        // Spawn hit particles.

        // Play hit sound based on health remaining.

        TakeDamageObserversRpc(damage, newHealth);
    }

    [ObserversRpc (ExcludeServer = true)]
    private void TakeDamageObserversRpc(float damage, float newHealth)
    {
        // Spawn damage indicator.
        var damageIndicator = Instantiate(_damageIndicatorPrefab, transform.position, Quaternion.identity);
        damageIndicator.GetComponent<DamageIndicatorManager>().Initialize((int)damage, newHealth);

        // Spawn hit particles.

        // Play hit sound based on health remaining.
    }

    [Server]
    public void SetHealth(float health)
    {
        // Add health.
        _health = health;

        // Spawn heal effect??
    }

    [Server]
    public void OnDeath(Transform heaven, NetworkConnection targetConn, NetworkObject killer)
    {
        IsDead = true;

        // Spawn death indicator prefab.
        var deathIndicatorPosition = transform.position;
        Instantiate(_deathIndicatorPrefab, deathIndicatorPosition, Quaternion.identity);

        // Drop current weapon.
        _weaponManager.DropCurrentWeapon();

        // Set the player's position to the heaven position.
        _playerController.OverrideTransform(heaven.position, heaven.rotation);

        // Set the player's camera to follow the killer.
        if (Camera.main.TryGetComponent(out CameraController cameraController))
        {
            cameraController.SetPlayer(killer.transform);
        }

        SetPlayerToFollowTargetRpc(targetConn, killer);

        OnDeathObserversRpc(deathIndicatorPosition);
    }

    [ObserversRpc (ExcludeServer = true)]
    private void OnDeathObserversRpc(Vector3 deathIndicatorPosition)
    {
        Instantiate(_deathIndicatorPrefab, deathIndicatorPosition, Quaternion.identity);
    }

    [Server]
    public void OnRespawn(Transform spawnPoint, Player player)
    {
        _weaponManager.EquipDefaultWeapon();

        _playerController.OverrideTransform(spawnPoint.position, spawnPoint.rotation);

        IsDead = false;

        if (Camera.main.TryGetComponent(out CameraController cameraController))
        {
            cameraController.ResetToLocal();
        }

        _health = MAX_HEALTH;

        SetPlayerToFollowTargetRpc(player.Connection, player.Nob);
    }

    private void OnModeChanged(Mode mode)
    {
        var currentRedFillAmount = _redDamagedSprite != null ? _redDamagedSprite.fillAmount : 0f;
        var currentWhiteFillAmount = _whiteDamagedSprite != null ? _whiteDamagedSprite.fillAmount : 0f;

        switch (mode)
        {
            case Mode.Sprint:
                _redDamagedSprite = _modeManager.RedSprintDamage;
                _whiteDamagedSprite = _modeManager.WhiteSprintDamage;
                break;
            case Mode.Shoot:
                _redDamagedSprite = _modeManager.RedShootDamage;
                _whiteDamagedSprite = _modeManager.WhiteShootDamage;
                break;
            case Mode.Slide:
                _redDamagedSprite = _modeManager.RedSlideDamage;
                _whiteDamagedSprite = _modeManager.WhiteSlideDamage;
                break;

        }

        _redDamagedSprite.fillAmount = currentRedFillAmount;
        _whiteDamagedSprite.fillAmount = currentWhiteFillAmount;
    }

    #endregion

    #region Camera

    [TargetRpc]
    public void SetPlayerToFollowTargetRpc(NetworkConnection conn, NetworkObject target)
    {
        if (Camera.main.TryGetComponent(out CameraController cameraController))
        {
            cameraController.SetPlayer(target.transform);
        }
    }

    #endregion

}
