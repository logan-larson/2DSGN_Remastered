using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    #region Private Fields

    private bool _subscribedToTimeManager = false;

    private Mode _currentMode = Mode.Sprint;

    private bool _currentDirectionLeft = false;

    private bool _currentDirectionRight = false;

    #endregion

    #region Script References

    [SerializeField]
    private MovementManager _movementManager;

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

        if (!base.IsOwner)
            return;

        UpdateModeClient(_movementManager.PublicData.Mode);
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

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        SubscribeToTimeManager(true);
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        SubscribeToTimeManager(false);
    }

    #endregion

    private void OnTick()
    {
        // If we are not the owner, we don't need to do anything.
        if (!base.IsOwner)
            return;

        // If the mode changes we need to update the current mode.
        if (_currentMode != _movementManager.PublicData.Mode)
        {
            // Set the mode on the client then server
            UpdateModeClient(_movementManager.PublicData.Mode);
        }

        // If the player direction changes we need to update the direction.
        if (_currentDirectionLeft != _movementManager.PublicData.DirectionLeft || _currentDirectionRight != _movementManager.PublicData.DirectionRight)
        {
            UpdateDirectionClient(_movementManager.PublicData.DirectionLeft, _movementManager.PublicData.DirectionRight);
        }
    }

    private void SetMode(Mode mode)
    {
        // Set the mode for the client
        _currentMode = mode;

        // Disable all modes.
        _sprintMode.SetActive(false);
        _shootMode.SetActive(false);
        _slideMode.SetActive(false);

        // Enable the current mode.
        switch (_currentMode)
        {
            case Mode.Sprint:
                _sprintMode.SetActive(true);
                break;
            case Mode.Shoot:
                _shootMode.SetActive(true);
                break;
            case Mode.Slide:
                _slideMode.SetActive(true);
                break;
        }
    }

    private void UpdateModeClient(Mode mode)
    {
        // Set the mode for the client
        SetMode(mode);

        UpdateModeServerRpc(mode);
    }

    [ServerRpc]
    private void UpdateModeServerRpc(Mode mode)
    {
        SetMode(mode);

        UpdateModeObserversRpc(mode);
    }

    [ObserversRpc (ExcludeOwner = true)]
    private void UpdateModeObserversRpc(Mode mode)
    {
        SetMode(mode);
    }

    private void SetDirection(bool directionLeft, bool directionRight)
    {
        _currentDirectionLeft = directionLeft;
        _currentDirectionRight = directionRight;

        if (_currentDirectionLeft)
            transform.localRotation = Quaternion.Euler(0, 180, 0);
        else if (_currentDirectionRight)
            transform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    private void UpdateDirectionClient(bool directionLeft, bool directionRight)
    {
        SetDirection(directionLeft, directionRight);

        UpdateDirectionServerRpc(directionLeft, directionRight);
    }

    [ServerRpc]
    private void UpdateDirectionServerRpc(bool directionLeft, bool directionRight)
    {
        SetDirection(directionLeft, directionRight);
    }
}
