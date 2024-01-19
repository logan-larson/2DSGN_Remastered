using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{

    #region Serialized Fields

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
    private Transform _currentPlayer = null;

    /// <summary>
    /// The player associated with this client.
    /// </summary>
    private Transform _clientPlayer = null;

    /// <summary>
    /// Whether or not this client is subscribed to the time manager.
    /// </summary>
    private bool _subscribedToTimeManager = false;

    #region Script References

    [SerializeField]
    private MovementManager _movementManager;

    [SerializeField]
    private InputManager _inputManager;

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

        SetPlayer(_clientPlayer);
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

    public void SetPlayer(Transform player)
    {
        _currentPlayer = player;

        // Set the local script references to the set player's script references
        _movementManager = _currentPlayer.GetComponent<MovementManager>();
        _inputManager = _currentPlayer.GetComponent<InputManager>();
    }


    void OnTick()
    {
        if (_currentPlayer == null) return;

        // Calculate the camera z position based on the player's velocity
        // Zoom out when moving faster
        Vector3 velocity = _movementManager.PublicData.Velocity;

        Vector3 targetPos = _currentPlayer.position;
        float posLerpValue = _posLerpValue;

        targetPos.z = startingZ - (velocity.magnitude * zFactor);

        targetPos.z = Mathf.Clamp(targetPos.z, -100f, -1f);

        // Lerp to nearest position and rotation
        Vector3 lerpedPos = Vector3.Lerp(this.transform.position, targetPos, posLerpValue);

        // Set the rotation lerp value based on whether the player is grounded or not
        var rotLerpValue = _movementManager.PublicData.IsGrounded ? _groundedRotLerpValue : _airborneRotLerpValue;

        // Check if the player is holding the camera rotation lock button
        Quaternion lerpedRot = this.transform.rotation;
        if (!_inputManager.CameraLockInput)
        {
            var movementRot = Quaternion.identity;
                
            if (_movementManager.PublicData.DirectionLeft)
            {
                movementRot = Quaternion.Euler(0, 0, movementRotationFactor * _movementManager.PublicData.Velocity.magnitude);
            }
            else if (_movementManager.PublicData.DirectionRight)
            {
                movementRot = Quaternion.Euler(0, 0, -movementRotationFactor * _movementManager.PublicData.Velocity.magnitude);
            }

            lerpedRot = Quaternion.Lerp(this.transform.rotation, _currentPlayer.rotation * movementRot, rotLerpValue);
        }

        this.transform.SetPositionAndRotation(new Vector3(lerpedPos.x, lerpedPos.y, lerpedPos.z), lerpedRot);
    }
}
