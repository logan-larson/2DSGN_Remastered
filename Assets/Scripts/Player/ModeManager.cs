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
            _currentMode = _movementManager.PublicData.Mode;
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

        // If the player direction changes we need to update the direction.
        if (_currentDirectionLeft != _movementManager.PublicData.DirectionLeft || _currentDirectionRight != _movementManager.PublicData.DirectionRight)
        {
            _currentDirectionLeft = _movementManager.PublicData.DirectionLeft;
            _currentDirectionRight = _movementManager.PublicData.DirectionRight;

            if (_currentDirectionLeft)
               transform.localRotation = Quaternion.Euler(0, 180, 0);
            else if (_currentDirectionRight)
                transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
