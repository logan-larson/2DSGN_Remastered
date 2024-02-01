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

    #region Public

    public Camera Camera { get; private set; } = null;

    #endregion

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
    private GameObject _damageIndicatorPrefab;

    [SerializeField]
    private GameObject _hitParticlesPrefab;

    [SerializeField]
    private GameObject _deathIndicatorPrefab;

    [SerializeField]
    private GameObject _audioListener;

    [SerializeField]
    private SpriteRenderer _playerSpriteRenderer;

    [SerializeField]
    private GameObject _abovePlayerUICanvas;

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
        _usernameText.color = base.IsOwner ? new Color32(0x28, 0x2F, 0xFF, 0xFF) : new Color32(0xE2, 0x25, 0x25, 0xFF);
        _redDamagedSprite.color = base.IsOwner ? new Color32(0x28, 0x2F, 0xFF, 0xFF) : new Color32(0xE2, 0x25, 0x25, 0xFF);

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

            _audioListener.SetActive(false);
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

    public override void OnStartServer()
    {
        base.OnStartServer();

        //GameManager.Instance.OnGameStart.AddListener(() => _movementDisabled = false);
        GameManager.Instance.OnGameEnd.AddListener(OnGameEnd);
    }

    #endregion

    #region Frame Updates

    private void OnTick()
    {
        // White damaged sprite fill should lerp to MAX_HEALTH - _health.
        _redDamagedSprite.fillAmount = _health / 100f;

        // Red damaged sprite should be equal to MAX_HEALTH - _health.
        _whiteDamagedSprite.fillAmount = Mathf.Lerp(_whiteDamagedSprite.fillAmount, _health / 100f, 0.1f);
    }

    #endregion

    #region Player State Events

    [Server]
    public void TakeDamage(float damage, float newHealth, bool isHeadshot = false, Vector3 hitPosition = new Vector3())
    {
        // Take damage.

        _health = newHealth;

        // Spawn damage indicator.
        var damageIndicator = Instantiate(_damageIndicatorPrefab, hitPosition, Quaternion.identity);
        damageIndicator.GetComponent<DamageIndicatorManager>().Initialize((int)damage, newHealth, isHeadshot);

        // Spawn hit particles.
        var hitParticles = Instantiate(_hitParticlesPrefab, hitPosition, Quaternion.identity);
        Destroy(hitParticles, 2f);

        StartCoroutine(FlashHealthCoroutine(newHealth));

        // Play hit sound based on health remaining.

        TakeDamageObserversRpc(damage, newHealth, isHeadshot, hitPosition);
    }

    [ObserversRpc (ExcludeServer = true)]
    private void TakeDamageObserversRpc(float damage, float newHealth, bool isHeadshot = false, Vector3 hitPostion = new Vector3())
    {
        // Spawn damage indicator.
        var damageIndicator = Instantiate(_damageIndicatorPrefab, hitPostion, Quaternion.identity);
        damageIndicator.GetComponent<DamageIndicatorManager>().Initialize((int)damage, newHealth, isHeadshot);

        // Spawn hit particles.
        var hitParticles = Instantiate(_hitParticlesPrefab, hitPostion, Quaternion.identity);
        Destroy(hitParticles, 2f);

        StartCoroutine(FlashHealthCoroutine(newHealth));

        // Play hit sound based on health remaining.
    }

    private IEnumerator FlashHealthCoroutine(float newHealth)
    {
        _playerSpriteRenderer.color = new Color(1f, newHealth / 100f, newHealth / 100f);

        yield return new WaitForSeconds(0.1f);

        _playerSpriteRenderer.color = Color.white;
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
        if (killer != null && base.IsHost && Camera.main.TryGetComponent(out CameraController cameraController))
        {
            cameraController.SetPlayer(killer.transform, false);
        }

        SetPlayerToFollowTargetRpc(targetConn, killer, false);

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

        _health = MAX_HEALTH;

        if (Camera.main.TryGetComponent(out CameraController cameraController))
        {
            cameraController.ResetToLocal();
        }

        SetPlayerToFollowTargetRpc(player.Connection, player.Nob, true);
    }

    private void OnModeChanged(Mode mode)
    {
        /*
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
        */

        switch (mode)
        {
            case Mode.Sprint:
                _abovePlayerUICanvas.transform.localPosition = new Vector3(0f, 0f, 0f);
                break;
            case Mode.Shoot:
                _abovePlayerUICanvas.transform.localPosition = new Vector3(0f, 1f, 0f);
                break;
            case Mode.Slide:
                _abovePlayerUICanvas.transform.localPosition = new Vector3(0f, -0.5f, 0f);
                break;
        }
    }

    #endregion

    #region Game State Events

    private void OnGameEnd()
    {
        OnGameEndObserversRpc();
    }

    [ObserversRpc]
    private void OnGameEndObserversRpc()
    {
        // Show the scoreboard and enable
        GameUIManager.Instance.SetShowScoreboard(true);
    }

    #endregion

    #region Camera

    public void SetCamera(Camera camera)
    {
        if (!base.IsOwner)
            return;

        Camera = camera;
    }

    [TargetRpc]
    public void SetPlayerToFollowTargetRpc(NetworkConnection conn, NetworkObject target, bool isLocal)
    {
        if (target != null && Camera.main.TryGetComponent(out CameraController cameraController))
        {
            cameraController.SetPlayer(target.transform, isLocal);
        }
    }

    #endregion

}
