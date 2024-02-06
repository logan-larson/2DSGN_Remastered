using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{

    #region Public Fields

    [Header("Public Fields")]

    public float CurrentZ;

    public Vector3 CrosshairPosition;

    public Vector3 WeaponHolderPosition;

    #endregion

    #region Serialized Fields

    [Header("Serialized Fields")]

    [SerializeField]
    private float _posLerpValue = 0.05f;

    [SerializeField]
    private float _airborneRotLerpValue = 0.05f;

    [SerializeField]
    private float _groundedRotLerpValue = 0.05f;

    [SerializeField]
    private float startingZ = -10f;

    [SerializeField]
    private float movementRotationFactor = 1.5f;

    /// <summary>
    /// The factor by which the z position is effected by the player's velocity.
    /// </summary>
    [SerializeField]
    private float zFactor = 0.4f;

    
    #endregion

    /// <summary>
    /// The current player being followed by the camera.
    /// </summary>
    private Transform _currentPlayer = null; // TODO: rename to _currentTargetTransform

    /// <summary>
    /// The player associated with this client.
    /// </summary>
    private Transform _clientPlayer = null;

    private bool _playerIsLocal = false;

    private bool _isFollowingPlayer = false;

    private Vector3? _currentTargetPosition = null;
    private Quaternion? _currentTargetRotation = null;

    /// <summary>
    /// Whether or not this client is subscribed to the time manager.
    /// </summary>
    private bool _subscribedToTimeManager = false;

    #region Script References

    [SerializeField]
    private PlayerController _playerController;

    [SerializeField]
    private InputManager _inputManager;

    [SerializeField]
    private PlayerManager _playerManager;

    #endregion

    #region Initialization

    private void Awake()
    {
        FirstObjectNotifier.OnFirstObjectSpawned += FirstObjectNotifier_OnFirstObjectSpawned;
    }

    private void OnDestroy()
    {
        FirstObjectNotifier.OnFirstObjectSpawned -= FirstObjectNotifier_OnFirstObjectSpawned;
    }


    private void FirstObjectNotifier_OnFirstObjectSpawned(Transform obj, GameObject go)
    {
        _clientPlayer = obj;

        SetPlayer(_clientPlayer, true);
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

    /// <summary>
    /// Sets the player to follow.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="isLocal"></param>
    public void SetPlayer(Transform player, bool isLocal)
    {
        // If we currently aren't following the local player, don't set the player to follow to
        // be another player that isn't the local player
        if (!_playerIsLocal && !isLocal)
        {
            SetNoFollow();
            return;
        }

        _isFollowingPlayer = true;

        _currentPlayer = player;

        _playerIsLocal = isLocal;

        // Set the local script references to the set player's script references
        _playerController = _currentPlayer.GetComponent<PlayerController>();
        _inputManager = _currentPlayer.GetComponent<InputManager>();
        _playerManager = _currentPlayer.GetComponent<PlayerManager>();
        _playerManager.SetCamera(GetComponent<Camera>());

        _currentTargetPosition = null;
        _currentTargetRotation = null;
    }

    public void SetPoint(Vector3 position, Quaternion rotation)
    {
        _isFollowingPlayer = true;

        _currentTargetPosition = position;
        _currentTargetRotation = rotation;
    }

    public void SetNoFollow()
    {
        _isFollowingPlayer = false;
    }

    public void ResetToLocal()
    {
        _playerIsLocal = true;

        SetPlayer(_clientPlayer, true);
    }


    void OnTick()
    {
        if (_currentPlayer == null || !_isFollowingPlayer) return;

        // If the current player is dead, don't follow them
        if (_playerManager.Status == PlayerStatus.Dead)
        {
            SetNoFollow();
            return;
        }

        // Calculate the camera z position based on the player's velocity
        // Zoom out when moving faster
        Vector3 velocity = _playerController.MovementData.Velocity;

        Vector3 targetPos = _currentPlayer.position;

        if (_currentTargetPosition != null)
            targetPos = _currentTargetPosition.Value;
        
        if (_playerIsLocal)
            targetPos = (_currentPlayer.position + WeaponHolderPosition) / 2f;

        if (_inputManager.CameraLockInput && _playerIsLocal)
        {
            targetPos = (targetPos + CrosshairPosition) / 2f;
        }
        
        float posLerpValue = _posLerpValue;

        targetPos.z = startingZ - (velocity.magnitude * zFactor);

        targetPos.z = Mathf.Clamp(targetPos.z, -100f, -1f);

        // Lerp to nearest position and rotation
        Vector3 lerpedPos = Vector3.Lerp(this.transform.position, targetPos, posLerpValue);

        // Set the rotation lerp value based on whether the player is grounded or not
        var rotLerpValue = _playerController.MovementData.IsGrounded ? _groundedRotLerpValue : _airborneRotLerpValue;

        // Check if the player is holding the camera rotation lock button
        Quaternion lerpedRot = _currentTargetRotation != null ? _currentTargetRotation.Value : this.transform.rotation;
        if (!_inputManager.CameraLockInput)
        {
            var movementRot = Quaternion.identity;
                
            if (_playerController.MovementData.DirectionLeft)
            {
                movementRot = Quaternion.Euler(0, 0, movementRotationFactor * _playerController.MovementData.Velocity.magnitude);
            }
            else if (_playerController.MovementData.DirectionRight)
            {
                movementRot = Quaternion.Euler(0, 0, -movementRotationFactor * _playerController.MovementData.Velocity.magnitude);
            }

            lerpedRot = Quaternion.Lerp(_currentTargetRotation != null ? _currentTargetRotation.Value : this.transform.rotation, _currentPlayer.rotation * movementRot, rotLerpValue);
        }

        this.transform.SetPositionAndRotation(lerpedPos, lerpedRot);

        CurrentZ = this.transform.position.z;
    }
}
