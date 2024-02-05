using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : NetworkBehaviour
{

    [SerializeField]
    private UserInfo _userInfo;

    [SyncVar (OnChange = nameof(OnPlayerStatusChanged))]
    public PlayerStatus Status = PlayerStatus.Alive;

    private void OnPlayerStatusChanged(PlayerStatus oldStatus, PlayerStatus newStatus, bool asServer)
    {
        if (newStatus == PlayerStatus.Dead)
        {
            // Disable the player's visual elements.
        }
        else if (newStatus == PlayerStatus.Alive)
        {
            // Enable the player's visual elements.
        }
    }

    [SyncVar (OnChange = nameof(OnUsernameChanged))]
    private string _username;

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

    [SerializeField]
    private InputManager _inputManager;

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

        // Enable the player's visual elements.
        OnPlayerStatusChanged(PlayerStatus.Alive, PlayerStatus.Alive, true);

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

        // Enable the player's visual elements.
        OnPlayerStatusChanged(PlayerStatus.Alive, PlayerStatus.Alive, true);

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

        GameManager.Instance.OnGameStateChange.AddListener(OnGameStateChange);
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
        Status = PlayerStatus.Dead;

        // Spawn death indicator prefab.
        var deathIndicatorPosition = transform.position;
        Instantiate(_deathIndicatorPrefab, deathIndicatorPosition, Quaternion.identity);

        // Drop current weapon.
        _weaponManager.DropCurrentWeapon();

        // Set the player's position to the heaven position.
        _playerController.OverrideTransform(heaven.position, heaven.rotation);

        // Set the player's camera to follow the killer.
        // Only do it on the server, if the player is host.
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
        if (base.IsOwner)
        {
            //// Hide the player's sprite, jump prediction line, username and health UI.
            
        }

        Instantiate(_deathIndicatorPrefab, deathIndicatorPosition, Quaternion.identity);
    }

    [Server]
    public void OnRespawn(Transform spawnPoint, Player player)
    {
        _weaponManager.EquipDefaultWeapon();

        _playerController.OverrideTransform(spawnPoint.position, spawnPoint.rotation);

        Status = PlayerStatus.Alive;

        _health = MAX_HEALTH;

        if (base.IsHost && Camera.main.TryGetComponent(out CameraController cameraController))
        {
            cameraController.ResetToLocal();
        }

        SetPlayerToFollowTargetRpc(player.Connection, player.Nob, true);
    }

    private void OnModeChanged(Mode mode)
    {
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

    private void OnGameStateChange(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.PreGame:
            case GameState.InGame:
                OnGameStart();
                break;
            case GameState.PostGame:
                OnGameEnd();
                break;
        }
    }

    private void OnGameStart()
    {
        OnGameStartObserversRpc();
    }

    [ObserversRpc]
    private void OnGameStartObserversRpc()
    {
        // Hide the scoreboard and disable
        GameUIManager.Instance.SetShowScoreboard(false);

        if (base.IsOwner)
        {
            SetCursorEnabled(false);
        }
    }

    private void OnGameEnd()
    {
        OnGameEndObserversRpc();
    }

    [ObserversRpc]
    private void OnGameEndObserversRpc()
    {
        // Show the scoreboard and enable
        GameUIManager.Instance.SetShowScoreboard(true);

        if (base.IsOwner)
        {
            SetCursorEnabled(true);
        }
    }

    #endregion

    #region Cursor

    private void SetCursorEnabled(bool isEnabled)
    {
        Cursor.visible = isEnabled;
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
        if (Camera.main.TryGetComponent(out CameraController cameraController))
        {
            if (target == null)
            {
                cameraController.SetNoFollow();
            }
            else
            {
                cameraController.SetPlayer(target.transform, isLocal);
            }
        }
    }

    #endregion

}
