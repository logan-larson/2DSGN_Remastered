using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : NetworkBehaviour
{

    #region Script References

    [SerializeField]
    private PlayerController _playerController;

    [SerializeField]
    private ModeManager _modeManager;

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    #endregion

    #region Serialized Fields

    [SerializeField]
    private PlayerMovementProperties _playerMovementProperties;

    #endregion

    #region Private Fields

    private bool _subscribedToTimeManager = false;

    private Mode _currentMode = Mode.Sprint;

    private string _currentAnimationState = "SprintIdle";

    private bool _currentFlipX = true;

    #endregion

    #region Constants

    private const string SPRINT_IDLE = "SprintIdle";
    private const string SPRINT_RUNNING = "SprintRunning";
    private const string SPRINT_JUMPING = "SprintJumping";

    private const string SHOOT_IDLE = "ShootIdle";
    private const string SHOOT_WALKING = "ShootWalking";
    private const string SHOOT_JUMPING = "ShootJumping";

    private const string SLIDE = "Slide";

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

        SubscribeToTimeManager(true);

        _modeManager.OnModeChanged.AddListener(OnModeChanged);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        SubscribeToTimeManager(false);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        SubscribeToTimeManager(true);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        SubscribeToTimeManager(false);
    }

    #endregion

    #region Frame Updates

    private void OnTick()
    {
        if (base.IsOwner)
        {
            UpdateAnimation(_playerController.MovementData.Velocity, _playerController.MovementData.DirectionLeft, _playerController.MovementData.IsGrounded);
        }
        else
        {
            UpdateAnimation(_playerController.Velocity, _playerController.DirectionLeft, _playerController.IsGrounded);
        }
    }

    private void UpdateAnimation(Vector3 velocity, bool directionLeft, bool isGrounded)
    {
        if (directionLeft != _currentFlipX)
        {
            _currentFlipX = directionLeft;
            _spriteRenderer.flipX = _currentFlipX;
        }

        if (velocity.magnitude > 0.1f)
        {
            switch (_currentMode)
            {
                case Mode.Sprint:
                    if (isGrounded)
                    {
                        ChangeAnimationState(SPRINT_RUNNING);
                    }
                    else
                    {
                        ChangeAnimationState(SPRINT_JUMPING);
                    }
                    break;
                case Mode.Shoot:
                    if (isGrounded)
                    {
                        ChangeAnimationState(SHOOT_WALKING);
                    }
                    else
                    {
                        ChangeAnimationState(SHOOT_JUMPING);
                    }
                    break;
                case Mode.Slide:
                    ChangeAnimationState(SLIDE);
                    break;
            }
        }
        else
        {
            switch (_currentMode)
            {
                case Mode.Sprint:
                    ChangeAnimationState(SPRINT_IDLE);
                    break;
                case Mode.Shoot:
                    ChangeAnimationState(SHOOT_IDLE);
                    break;
                case Mode.Slide:
                    ChangeAnimationState(SLIDE);
                    break;
            }
        }

        switch (_currentAnimationState)
        {
            case SPRINT_IDLE:
            case SHOOT_IDLE:
                _animator.speed = 1f;
                break;
            case SPRINT_RUNNING:
                _animator.speed = velocity.magnitude / (_playerMovementProperties.MaxSpeed * _playerMovementProperties.SprintMultiplier);
                break;
            case SHOOT_WALKING:
                _animator.speed = velocity.magnitude / (_playerMovementProperties.MaxSpeed * _playerMovementProperties.ShootMultiplier);
                break;
        }
    }

    #endregion

    #region Event Handlers

    private void OnModeChanged(Mode mode)
    {
        _currentMode = mode;
    }

    #endregion

    #region Private Methods

    private void ChangeAnimationState(string newState)
    {
        if (_currentAnimationState == newState)
            return;

        _animator.Play(newState);

        _currentAnimationState = newState;
    }

    #endregion
}
