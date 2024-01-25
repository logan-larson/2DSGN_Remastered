using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ModeManager : NetworkBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private GameObject _sprintMode;

    [SerializeField]
    private GameObject _shootMode;

    [SerializeField]
    private GameObject _slideMode;

    #endregion

    #region Public Fields

    public Image RedSprintDamage;
    public Image WhiteSprintDamage;

    public Image RedShootDamage;
    public Image WhiteShootDamage;

    public Image RedSlideDamage;
    public Image WhiteSlideDamage;

    [SyncVar(OnChange = nameof(OnCurrentModeChanged))]
    public Mode CurrentMode = Mode.Sprint;

    private void OnCurrentModeChanged(Mode oldValue, Mode newValue, bool asServer)
    {
        // Disable all modes.
        _sprintMode.SetActive(false);
        _shootMode.SetActive(false);
        _slideMode.SetActive(false);

        // Enable the current mode.
        switch (newValue)
        {
            case Mode.Sprint:
                _sprintMode.SetActive(true);
                OnModeChanged.Invoke(Mode.Sprint);
                break;
            case Mode.Shoot:
                _shootMode.SetActive(true);
                OnModeChanged.Invoke(Mode.Shoot);
                break;
            case Mode.Slide:
                _slideMode.SetActive(true);
                OnModeChanged.Invoke(Mode.Slide);
                break;
        }
    }

    #endregion

    #region Private Fields

    private bool _subscribedToTimeManager = false;

    [SyncVar (OnChange = nameof(OnCurrentDirectionLeftChanged))]
    private bool _currentDirectionLeft = false;

    private void OnCurrentDirectionLeftChanged(bool oldValue, bool newValue, bool asServer)
    {
        if (newValue)
        {
            transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }

    [SyncVar (OnChange = nameof(OnCurrentDirectionRightChanged))]
    private bool _currentDirectionRight = true;

    private void OnCurrentDirectionRightChanged(bool oldValue, bool newValue, bool asServer)
    {
        if (newValue)
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    #endregion

    #region Script References

    [SerializeField]
    private PlayerController _playerController;

    #endregion

    #region Events

    public UnityEvent<Mode> OnModeChanged = new UnityEvent<Mode>();

    #endregion

    #region Initialization

    private void Awake()
    {
        //_movementManager = GetComponent<MovementManager>();
        // Set the modes to the children of this object.
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Set the damage images.
        RedSprintDamage = _sprintMode.transform.Find("RedDamage").GetComponent<Image>();
        WhiteSprintDamage = _sprintMode.transform.Find("WhiteDamage").GetComponent<Image>();

        RedShootDamage = _shootMode.transform.Find("RedDamage").GetComponent<Image>();
        WhiteShootDamage = _shootMode.transform.Find("WhiteDamage").GetComponent<Image>();

        RedSlideDamage = _slideMode.transform.Find("RedDamage").GetComponent<Image>();
        WhiteSlideDamage = _slideMode.transform.Find("WhiteDamage").GetComponent<Image>();

        // Set the fill amounts to zero.
        RedSprintDamage.fillAmount = 0f;
        WhiteSprintDamage.fillAmount = 0f;

        RedShootDamage.fillAmount = 0f;
        WhiteShootDamage.fillAmount = 0f;

        RedSlideDamage.fillAmount = 0f;
        WhiteSlideDamage.fillAmount = 0f;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        // Set the mode for the server.
        CurrentMode = _playerController.MovementData.Mode;

        SubscribeToTimeManager(true);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        SubscribeToTimeManager(false);
    }

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

    #region Frame Updates

    private void OnTick()
    {
        // If the mode changes we need to update the current mode.
        if (CurrentMode != _playerController.MovementData.Mode)
        {
            CurrentMode = _playerController.MovementData.Mode;
        }

        // If the player direction changes we need to update the direction.
        if (_currentDirectionLeft != _playerController.MovementData.DirectionLeft || _currentDirectionRight != _playerController.MovementData.DirectionRight)
        {
            _currentDirectionLeft = _playerController.MovementData.DirectionLeft;
            _currentDirectionRight = _playerController.MovementData.DirectionRight;
        }
    }

    #endregion
}
