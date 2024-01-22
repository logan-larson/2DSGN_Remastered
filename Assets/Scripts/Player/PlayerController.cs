using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    #region Types

    /// <summary>
    /// All the data needed to move the player on the client and server.
    /// </summary>
    public struct MoveData : IReplicateData
    {
        // Movement input
        public float Horizontal;
        public bool Jump;
        public bool Fire;
        public Vector2 AimDirection;

        // Mode input
        public bool Sprint;
        public bool Slide;
        public bool Shoot;

        private uint _tick;

        public MoveData(float horizontal, bool jump, bool fire, Vector2 aimDirection, bool sprint, bool slide, bool shoot)
        {
            Horizontal = horizontal;
            Sprint = sprint;
            Slide = slide;
            Shoot = shoot;
            Jump = jump;
            Fire = fire;
            AimDirection = aimDirection;
            _tick = 0;
        }

        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    /// <summary>
    /// All the data needed to reconcile the player on the client and server.
    /// </summary>
    public struct ReconcileData : IReconcileData
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public Quaternion Rotation;
        public bool IsGrounded;
        public float TimeOnGround;

        // What do I need to reconcile for the shooting?
        public float TimeSinceLastShot;
        public bool CanShoot;

        public Mode Mode;

        private uint _tick;

        public ReconcileData(Vector3 position, Vector3 velocity, Quaternion rotation, bool isGrounded, float timeOnGround, float timeSinceLastShot, bool canShoot, Mode mode)
        {
            Position = position;
            Velocity = velocity;
            Rotation = rotation;
            IsGrounded = isGrounded;
            TimeOnGround = timeOnGround;
            TimeSinceLastShot = timeSinceLastShot;
            CanShoot = canShoot;
            Mode = mode;
            _tick = 0;
        }

        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    /// <summary>
    /// The origins of the raycasts used to determine if the player is grounded.
    /// </summary>
    public struct RaycastOrigins
    {
        public Vector2 bottomLeft, bottomRight;
    }

    public struct PublicMovementData
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public Vector3 AimDirection;
        public bool IsGrounded;
        public Mode Mode;
        public bool DirectionLeft;
        public bool DirectionRight;
        public bool IsJumping;
    }

    #endregion

    #region Public Fields

    [Header("Public Fields")]

    public PublicMovementData PublicData;

    #endregion

    #region Events

    public UnityEvent<Mode> OnModeChange = new UnityEvent<Mode>();

    #endregion

    #region Script References

    [Header("Script References")]

    [SerializeField]
    private InputManager _inputManager;

    [SerializeField]
    private WeaponManager _weaponManager;

    #endregion

    #region Inspector Variables

    [Header("Movement")]
    /// <summary>
    /// The amount by which the player's speed is changed when they move.
    /// </summary>
    [SerializeField]
    private float _acceleration = 2f;

    /// <summary>
    /// The amount by which the player's speed is changed when they stop moving.
    /// </summary>
    [SerializeField]
    private float _friction = 1f;

    /// <summary>
    /// The maximum speed at which the player can move.
    /// </summary>
    [SerializeField]
    private float _maxSpeed = 5f;

    /// <summary>
    /// The maximum speed at which the player can move while in the air.
    /// </summary>
    //[SerializeField]
    //private float _maxAirborneSpeed = 10f;

    /// <summary>
    /// The amount by which the player's speed is changed when they are in sprint mode.
    /// </summary>
    [SerializeField]
    private float _sprintMultiplier = 2f;

    /// <summary>
    /// The amount by which the player's speed is changed when they are in shoot mode.
    /// </summary>
    [SerializeField]
    private float _shootMultiplier = 0.75f;

    /// <summary>
    /// The amount by which the player's speed is changed when they are in slide mode.
    /// </summary>
    [SerializeField]
    private float _slideMultiplier = 1f;

    /// <summary>
    /// The worlds gravity
    /// TODO: test with 9.81f and higher jump velocity.
    /// </summary>
    [SerializeField]
    private float _gravity = 10f;

    /// <summary>
    /// The maximum angle the player can rotate at.
    [SerializeField]
    private float _maxRotationDegrees = 180f;

    [Header("Jumping")]

    /// <summary>
    /// The amount of force applied to the player when they jump.
    /// </summary>
    [SerializeField]
    private float _jumpVelocity = 10f;

    /// <summary>
    /// The amount of time the player has to jump for before being able to be grounded again.
    /// </summary>
    [SerializeField]
    private float _minimumJumpTime = 0.1f;

    /// <summary>
    /// Fudge factor for predicting landings
    /// </summary>
    [SerializeField]
    private float _fFactor = 7.5f;

    [Header("Grounded")]

    /// <summary>
    /// The height above the ground at which the player is considered to be grounded.
    /// </summary>
    [SerializeField]
    private float _groundedHeight = 1.1f;

    /// <summary>
    /// Layer mask for obstacles.
    /// </summary>
    [SerializeField]
    private LayerMask _obstacleMask;

    /// <summary>
    /// The length of the ray used to override player angle checks.
    /// </summary>
    [SerializeField]
    private float _overrideRayLength = 0;

    /// <summary>
    /// Current mode of the player.
    /// </summary>
    [SerializeField]
    private Mode _currentMode = Mode.Sprint;

    /// <summary>
    /// Previous mode of the player.
    /// </summary>
    [SerializeField]
    private Mode _previousMode = Mode.Sprint;

    #endregion

    #region Private Variables

    /// <summary>
    /// The current airborne velocity of the player.
    /// </summary>
    private Vector3 _currentVelocity = new Vector3();

    /// <summary>
    /// True if subscribed to the TimeManager.
    /// </summary>
    private bool _subscribedToTimeManager = false;

    /// <summary>
    /// The raycast origins of the player.
    /// </summary>
    private RaycastOrigins _raycastOrigins;

    /// <summary>
    /// True if the player is currently grounded.
    /// </summary>
    private bool _isGrounded = false;

    /// <summary>
    /// The distance from the player to the ground.
    /// </summary>
    private float _groundDistance = 0f;

    /// <summary>
    /// Time since player has been grounded, used for jumping and re-enabling grounded.
    /// </summary>
    private float _timeSinceGrounded = 0f;

    /// <summary>
    /// Time since player has been grounded, used for jumping and re-enabling grounded.
    /// </summary>
    private float _timeOnGround = 0f;

    /// <summary>
    /// True if the player is allowed to jump.
    /// </summary>
    private bool _canJump = true;

    /// <summary>
    /// True if the player is allowed to shoot.
    /// </summary>
    private bool _canShoot = true;

    /// <summary>
    /// Time since player has shot, used for re-enabling shooting.
    /// </summary>
    private float _timeSinceLastShot = 0f;

    /// <summary>
    /// The predicted landing position for airborne player.
    /// </summary>
    private Vector3 _predictedPosition = new Vector3();

    /// <summary>
    /// The predicted normal of surface of landing position for airborne player.
    /// </summary>
    private Vector3 _predictedNormal = new Vector3();

    /// <summary>
    /// True if the predicted landing position and normal should be recalculated.
    /// </summary>
    private bool _recalculateLanding = false;

    private IEnumerator _recalculateLandingCoroutine;
    private bool _recalculateLandingCoroutineIsRunning;

    /// <summary>
    /// Set to true when the player's movement should be disabled.
    /// </summary>
    private bool _movementDisabled = false;

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

    #region Initialization

    private void Awake()
    {
        _inputManager ??= GetComponent<InputManager>();
    }


    #endregion

    #region Frame Updates

    /// <summary>
    /// Every server tick, update the player's movement.
    /// </summary>
    private void OnTick()
    {
        if (base.IsOwner)
        {
            Reconciliation(default, false);

            BuildActions(out MoveData moveData);

            Move(moveData, false);
        }

        if (base.IsServer)
        {
            Move(default, true);

            ReconcileData reconcileData = new ReconcileData()
            {
                /* Copy by value */
                Position = new Vector3(transform.position.x, transform.position.y, transform.position.z),
                Velocity = new Vector3(_currentVelocity.x, _currentVelocity.y, _currentVelocity.z),
                Rotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w),

                IsGrounded = _isGrounded,
                TimeOnGround = _timeOnGround,
            };

            Reconciliation(reconcileData, true);
        }

        SetPublicMovementData();
    }


    /// <summary>
    /// Use player input to build move data that will be used in the move function.
    /// </summary>
    /// <param name="moveData"></param>
    private void BuildActions(out MoveData moveData)
    {
        moveData = default;

        moveData.Horizontal = _inputManager.HorizontalMoveInput;
        moveData.Jump = _inputManager.JumpInput;

        moveData.Sprint = _inputManager.SprintInput;
        moveData.Slide = _inputManager.SlideInput;
        moveData.Shoot = _inputManager.ShootInput;

        moveData.Fire = _inputManager.FireInput;

        // Need to calculate the aim direction client side because the server doesn't know the mouse position or player camera.
        if (_inputManager.InputDevice == "Keyboard&Mouse")
        {
            var mousePosition = Input.mousePosition;

            mousePosition.z = Camera.main.transform.position.z * -1f;

            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            mouseWorldPosition.z = 0f;

            moveData.AimDirection = (mouseWorldPosition - transform.position).normalized;
        }
        else if (_inputManager.InputDevice == "Gamepad")
        {
            if (_inputManager.Aim != Vector2.zero)
            {
                moveData.AimDirection = Camera.main.transform.rotation * new Vector3(_inputManager.Aim.x, _inputManager.Aim.y, 0f).normalized;
            }
        }
        else
        {
            moveData.AimDirection = Vector2.zero;
        }
    }

    /// <summary>
    /// Move the player using the provided moveData.
    /// This function is replicated on both the server and client.
    /// </summary>
    /// <param name="moveData"></param>
    /// <param name="asServer"></param>
    /// <param name="replaying"></param>
    [Replicate]
    private void Move(MoveData moveData, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
    {
        if (_movementDisabled)
        {
            _currentVelocity = Vector3.zero;
            _predictedNormal = Vector3.zero;
            _predictedPosition = transform.position;
            return;
        }

        UpdateRaycastOrigins();

        UpdateGrounded();

        UpdateMode(moveData);

        UpdateAimDirection(moveData);

        UpdateFire();

        UpdateVelocity(moveData, asServer);

        UpdatePosition(moveData);
    }

    /// <summary>
    /// Reconcile the player's data.
    /// This function is only called on the client.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="asServer"></param>
    [Reconcile]
    private void Reconciliation(ReconcileData data, bool asServer, Channel channel = Channel.Unreliable)
    {
        transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
        transform.rotation = new Quaternion(data.Rotation.x, data.Rotation.y, data.Rotation.z, data.Rotation.w);
        _currentVelocity = new Vector3(data.Velocity.x, data.Velocity.y, data.Velocity.z);
        _isGrounded = data.IsGrounded;
        _timeOnGround = data.TimeOnGround;
        _canShoot = data.CanShoot;
        _timeSinceLastShot = data.TimeSinceLastShot;
    }

    #endregion

    #region Movement

    private void UpdateRaycastOrigins()
    {
        _raycastOrigins.bottomLeft = transform.position - (transform.right / 2) - (transform.up / 2);
        _raycastOrigins.bottomRight = transform.position + (transform.right / 2) - (transform.up / 2);
    }

    private void UpdateGrounded()
    {
        if (_isGrounded && _timeOnGround <= _minimumJumpTime * 2f)
            _timeOnGround += (float)TimeManager.TickDelta;
        else if (!_isGrounded)
            _timeOnGround = 0f;

        // If the player is currently grounded, check if they are still grounded and set ground distance
        if (_isGrounded || _timeSinceGrounded > _minimumJumpTime)
        {
            RaycastHit2D groundedHit = Physics2D.Raycast(transform.position, -transform.up, _groundedHeight, _obstacleMask);
            _isGrounded = groundedHit.collider != null;
            _groundDistance = groundedHit.distance;
            if (_isGrounded && _timeOnGround > _minimumJumpTime * 2f)
                _canJump = true;

            PublicData.IsJumping = false;
        }
        // Otherwise, increase the time since grounded
        else
        {
            _timeSinceGrounded += (float)TimeManager.TickDelta;
        }
    }

    private void UpdateMode(MoveData moveData)
    {
        // Mode Priority: Shoot > Sprint > Slide
        if (moveData.Shoot)
        {
            _currentMode = Mode.Shoot;
        }
        else if (moveData.Sprint)
        {
            _currentMode = Mode.Sprint;
        }
        else if (moveData.Slide)
        {
            _currentMode = Mode.Slide;
        }
    }

    private void UpdateAimDirection(MoveData moveData)
    {
        PublicData.AimDirection = moveData.AimDirection;
    }

    private void UpdateFire()
    {
        _canShoot = false;

        if (_weaponManager.CurrentWeaponInfo == null)
            return;

        // If the player can shoot, check if they are shooting
        if (_timeSinceLastShot >= _weaponManager.CurrentWeaponInfo.FireRate)
        {
            _canShoot = true;
        }
        else
        {
            _timeSinceLastShot += (float)TimeManager.TickDelta;
        }
    }

    private void UpdateVelocity(MoveData moveData, bool asServer)
    {
        // Based on the mode, set the movement speed multiplier
        float modeMultiplier = 1f;
        switch (_currentMode)
        {
            case Mode.Sprint:
                modeMultiplier = _sprintMultiplier;
                break;
            case Mode.Shoot:
                modeMultiplier = _shootMultiplier;
                break;
            case Mode.Slide:
                modeMultiplier = _slideMultiplier;
                break;
        }

        // If grounded, change velocity
        if (_isGrounded)
        {
            // If sliding, adjust the velocity to match the slope
            if (_currentMode == Mode.Slide)
            {
                Vector3 newVelo = Vector3.ProjectOnPlane(_currentVelocity, transform.up);
                _currentVelocity = newVelo;
            }
            else
            {

                // If horizontal input is given, add velocity
                if (moveData.Horizontal != 0f)
                {
                    _currentVelocity += transform.right * moveData.Horizontal * _acceleration * modeMultiplier;
                }
                else
                {
                    // If no horizontal input is given, decrease velocity by friction
                    _currentVelocity = Vector3.MoveTowards(_currentVelocity, Vector3.zero, _friction);
                }
            }
        }

        // Limit top speed
        //float maxSpeed = _isGrounded ? MovementProperties.MaxSpeed : MovementProperties.MaxAirborneSpeed;
        // Sliding tweaks lol
        float maxSpeed = _isGrounded && _currentMode != Mode.Slide ? _maxSpeed : 100f;

        if (_isGrounded && _currentVelocity.magnitude > maxSpeed * modeMultiplier)
        {
            _currentVelocity = _currentVelocity.normalized * maxSpeed * modeMultiplier;
        }


        // The direction has to be set based on velocity relative to the player
        if (_isGrounded)
        {
            // Jump
            if (moveData.Jump && _canJump)
            {
                _canJump = false;
                _isGrounded = false;
                _timeSinceGrounded = 0f;

                PublicData.IsJumping = true;

                _currentVelocity += transform.up * _jumpVelocity;

                RecalculateLandingPosition();

                _recalculateLandingCoroutine = RecalculateNextLandingCoroutine();

                _recalculateLandingCoroutineIsRunning = true;

                StartCoroutine(_recalculateLandingCoroutine);
            }
            // Set height relative to ground
            else
            {
                if (_recalculateLandingCoroutineIsRunning)
                {
                    StopCoroutine(_recalculateLandingCoroutine);
                    _recalculateLandingCoroutineIsRunning = false;
                }

                if (_currentMode == Mode.Slide)
                {
                    // Apply gravity in direction of slope
                    Vector3 gravity = new Vector3(0f, _gravity, 0f);

                    Vector3 gravityParallel = Vector3.Project(gravity, transform.right);

                    _currentVelocity -= gravityParallel * (float)TimeManager.TickDelta;
                }
            }
        }
        else
        {
            // Apply gravity
            _currentVelocity += (Vector3.down * _gravity * (float)TimeManager.TickDelta);

            // This is where airborne movement forces can be applied
            if (_canShoot && moveData.Fire)
            {
                _canShoot = false;
                _timeSinceLastShot = 0f;

                _currentVelocity += -new Vector3(moveData.AimDirection.x, moveData.AimDirection.y, 0f) * _weaponManager.CurrentWeaponInfo.AirborneKnockback;

                _recalculateLanding = true;
            }
        }

        //CheckNeedRecalc();

        // If we are not grounded and we have not predicted a landing position then we need to calculate it
        // Or if we manually trigger a recalculation
        if ((!_isGrounded && _predictedNormal == Vector3.zero && _predictedPosition == Vector3.zero) || _recalculateLanding)
        {
            RecalculateLandingPosition();
        }
    }

    private IEnumerator RecalculateNextLandingCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            Vector2? normal = RecalculateNextLandingNormal();

            if (normal != null)
            {
                Vector2 diff = (Vector2)_predictedNormal - (Vector2)normal;

                if (diff.magnitude > 0.1f)
                {
                    _predictedNormal = (Vector2)normal;
                }
                else
                {
                    yield break;
                }
            }
        }

    }

    private void UpdatePosition(MoveData moveData)
    {
        Vector3 changeGround = Vector3.zero;

        if (_isGrounded)
        {
            changeGround = transform.up * (1f - _groundDistance);
        }

        Vector3 finalPosition = (transform.position + changeGround) + _currentVelocity * (float)TimeManager.TickDelta;

        if (_isGrounded)
        {
            Vector2 velocity = new Vector2(_currentVelocity.x, _currentVelocity.y);

            Ray2D leftRay = new Ray2D(_raycastOrigins.bottomLeft + (velocity * (float)TimeManager.TickDelta), -transform.up);
            Ray2D rightRay = new Ray2D(_raycastOrigins.bottomRight + (velocity * (float)TimeManager.TickDelta), -transform.up);

            RaycastHit2D leftHit = Physics2D.Raycast(leftRay.origin, leftRay.direction, _groundedHeight, _obstacleMask);
            RaycastHit2D rightHit = Physics2D.Raycast(rightRay.origin, rightRay.direction, _groundedHeight, _obstacleMask);

            // Use override hit to prevent clipping
            RaycastHit2D overrideHit = Physics2D.Raycast((leftRay.origin + rightRay.origin) / 2, transform.right, _overrideRayLength * Mathf.Sign(_currentVelocity.x), _obstacleMask);
            if (_currentVelocity.x < 0f && overrideHit.collider != null)
            {
                leftHit = overrideHit;
            }
            else if (_currentVelocity.x > 0f && overrideHit.collider != null)
            {
                rightHit = overrideHit;
            }

            // Apply rotation to orient body to match ground
            Quaternion finalRotation = transform.rotation;
            if (leftHit && rightHit)
            {
                Vector2 avgNorm = (leftHit.normal + rightHit.normal) / 2;

                Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, avgNorm);
                finalRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _maxRotationDegrees);
            }

            transform.SetPositionAndRotation(finalPosition, finalRotation);
        }
        else
        {
            // And rotate to the predicted landing spots normal
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, _predictedNormal);
            Quaternion finalRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _maxRotationDegrees);

            transform.SetPositionAndRotation(finalPosition, finalRotation);
        }
    }

    private void RecalculateLandingPosition()
    {

        _recalculateLanding = false;

        // Predict the landing spot 
        RaycastHit2D predictHit = new RaycastHit2D();
        RaycastHit2D predictHit2 = new RaycastHit2D();
        Vector2 pos = transform.position - (transform.up * 0.7f);
        Vector2 pos2 = transform.position + (transform.up * 0.7f);

        Vector2 velo = new Vector2(_currentVelocity.x, _currentVelocity.y) * Time.fixedDeltaTime * _fFactor;

        int count = 0;
        while ((predictHit.collider == null && predictHit2.collider == null) && count < 100)
        {

            // Generate new ray
            Ray2D ray = new Ray2D(pos, velo.normalized);
            Ray2D ray2 = new Ray2D(pos2, velo.normalized);

            Color randColor = UnityEngine.Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f);

            Debug.DrawRay(ray.origin, ray.direction * velo.magnitude, randColor, 2f);
            Debug.DrawRay(ray2.origin, ray2.direction * velo.magnitude, randColor, 2f);

            // Update predictHit
            predictHit = Physics2D.Raycast(ray.origin, ray.direction, velo.magnitude, _obstacleMask);
            predictHit2 = Physics2D.Raycast(ray2.origin, ray2.direction, velo.magnitude, _obstacleMask);

            // Update position to end of predictHit ray
            pos += (ray.direction * velo.magnitude);
            pos2 += (ray2.direction * velo.magnitude);

            velo += (Vector2.down * _gravity * Time.fixedDeltaTime);

            count++;
        }

        // Set the predicted landing position and normal
        // By nature this will get the closer of the two landing positions
        if (predictHit.collider != null)
        {
            _predictedPosition = predictHit.point;

            _predictedNormal = predictHit.normal;
        }
        else if (predictHit2.collider != null)
        {
            _predictedPosition = predictHit2.point;

            _predictedNormal = predictHit2.normal;
        }
    }

    private Vector2? RecalculateNextLandingNormal()
    {

        _recalculateLanding = false;

        // Predict the landing spot 
        RaycastHit2D predictHit = new RaycastHit2D();
        RaycastHit2D predictHit2 = new RaycastHit2D();
        Vector2 pos = transform.position - (transform.up * 0.7f);
        Vector2 pos2 = transform.position + (transform.up * 0.7f);

        Vector2 velo = new Vector2(_currentVelocity.x, _currentVelocity.y) * Time.fixedDeltaTime * _fFactor;

        int count = 0;
        while ((predictHit.collider == null && predictHit2.collider == null) && count < 100)
        {

            // Generate new ray
            Ray2D ray = new Ray2D(pos, velo.normalized);
            Ray2D ray2 = new Ray2D(pos2, velo.normalized);

            Color randColor = UnityEngine.Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f);

            Debug.DrawRay(ray.origin, ray.direction * velo.magnitude, randColor, 2f);
            Debug.DrawRay(ray2.origin, ray2.direction * velo.magnitude, randColor, 2f);

            // Update predictHit
            predictHit = Physics2D.Raycast(ray.origin, ray.direction, velo.magnitude, _obstacleMask);
            predictHit2 = Physics2D.Raycast(ray2.origin, ray2.direction, velo.magnitude, _obstacleMask);

            // Update position to end of predictHit ray
            pos += (ray.direction * velo.magnitude);
            pos2 += (ray2.direction * velo.magnitude);

            velo += (Vector2.down * _gravity * Time.fixedDeltaTime);

            count++;
        }

        // Set the predicted landing position and normal
        // By nature this will get the closer of the two landing positions
        if (predictHit.collider != null)
        {
            //_predictedPosition = predictHit.point;

            return predictHit.normal;
        }
        else if (predictHit2.collider != null)
        {
            //_predictedPosition = predictHit2.point;

            return predictHit2.normal;
        }

        return null;
    }

    #endregion

    private void SetPublicMovementData()
    {
        PublicData.Position = transform.position;
        PublicData.Velocity = _currentVelocity;
        PublicData.IsGrounded = _isGrounded;

        PublicData.Mode = _currentMode;

        var angle = Vector3.SignedAngle(PublicData.Velocity, transform.right, transform.up);
        if (PublicData.Velocity.magnitude != 0f)
        {
            if (angle < 5f || angle > 355f)
            {
                PublicData.DirectionLeft = false;
                PublicData.DirectionRight = true;
            }
            else if (angle > 175f && angle < 185f)
            {
                PublicData.DirectionLeft = true;
                PublicData.DirectionRight = false;
            }
        }

        if (_currentMode != _previousMode)
        {
            OnModeChange.Invoke(_currentMode);
            _previousMode = _currentMode;
        }
    }
}
