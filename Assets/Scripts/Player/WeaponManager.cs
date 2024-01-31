using FishNet;
using FishNet.Component.ColliderRollback;
using FishNet.Connection;
using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    #region Public Fields

    [Header("Public Fields")]
    public bool IsWeaponEquipped;

    [SyncVar (OnChange = nameof(OnCurrentWeaponInfoChange))]
    public WeaponInfo CurrentWeaponInfo = null;

    private void OnCurrentWeaponInfoChange(WeaponInfo oldValue, WeaponInfo newValue, bool asServer)
    {
        // TODO: Initialize other things like the weapon sprite, etc.
        if (!asServer && newValue != null)
        {
            _weaponHolder.GetComponentInChildren<SpriteRenderer>(true).sprite = Resources.Load<Sprite>(newValue.SpritePath);
        }
    }

    public float Bloom => _currentBloomAngle;

    #endregion

    #region Serialized Fields

    [Header("Serialized Fields")]

    [SerializeField]
    private float _pickupRadius = 5.0f;

    [SerializeField]
    private GameObject _weaponPickupPrefab;

    [SerializeField]
    private WeaponInfo _defaultWeaponInfo;

    [SerializeField]
    private Transform _weaponHolder;

    [SerializeField]
    private float _pickupCooldown = 0.5f;

    [SerializeField]
    private TrailRenderer _bulletTrailRenderer;

    [SerializeField]
    private GameObject _muzzleFlashPrefab;

    [SerializeField]
    private AudioSource _hitSound;

    [SerializeField]
    private AudioSource _headshotSound;

    [SerializeField]
    private AudioSource _shotSound;

    #endregion

    #region Private Fields

    private bool _subscribedToTimeManager = false;

    // The parent object of all weapon pickups.
    private GameObject _pickupsParent;

    private (float, Pickup) _closestPickup;

    private float _currentPickupCooldown = 0.0f;

    [SerializeField]
    private float _currentBloomAngle = 0.0f;

    private float _bloomTimer = 0.0f;

    [SyncVar]
    private int _instanceID = -1;

    #endregion

    #region Script References

    [Header("Script References")]

    [SerializeField]
    private InputManager _inputManager;

    [SerializeField]
    private PlayerController _playerController;

    [SerializeField]
    private WeaponHolderController _weaponHolderController;

    [SerializeField]
    private PlayerManager _playerManager;

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
        _playerController ??= GetComponent<PlayerController>();
        _playerManager ??= GetComponent<PlayerManager>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        SetCurrentWeapon(_defaultWeaponInfo);

        _pickupsParent = GameObject.Find("WeaponPickups");

        if (_pickupsParent == null)
        {
            Debug.LogError("WeaponPickups parent not found.");
        }

        _weaponHolder.GetChild(0).gameObject.SetActive(_playerController.MovementData.Mode == Mode.Shoot);

        _playerController.OnModeChange.AddListener(OnModeChange);

        _instanceID = gameObject.GetInstanceID();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        //_playerController.OnModeChange.RemoveListener(OnModeChange);
        DropCurrentWeapon();
    }

    // When the client starts, set the weapon pickups parent.
    public override void OnStartClient()
    {
        base.OnStartClient();

        var map = GameObject.Find("Map");

        _pickupsParent = map.transform.GetChild(1).gameObject;

        if (_pickupsParent == null)
        {
            Debug.LogError("WeaponPickups parent not found.");
        }

        _weaponHolder.GetChild(0).gameObject.SetActive(_playerController.MovementData.Mode == Mode.Shoot);
    }

    #endregion

    #region Frame Updates

    private void OnTick()
    {
        // Perform all actions on the client and notify server with RPCs.
        if (!base.IsOwner)
            return;

        // -- Weapon Equipping --

        // Highlight the closest weapon pickup. This only occurs client side.
        HighlightClosestPickup();

        if (_currentPickupCooldown >= _pickupCooldown)
        {
            _currentPickupCooldown = _pickupCooldown;

            // If the player presses the pickup button, try to pickup the weapon.
            CheckPickup();
        }
        else
        {
            // Increment the pickup cooldown.
            _currentPickupCooldown += (float) TimeManager.TickDelta;
        }

        // -- Weapon Aiming --

        UpdateWeaponHolderDirection();


        // -- Weapon Firing --

        /*
        if (_weaponHolder.transform.localPosition != Vector3.zero)
        {
            // If the weapon holder is not at the player's position, move it to the player's position at a constant speed.
            _weaponHolder.transform.localPosition = Vector3.MoveTowards(_weaponHolder.transform.localPosition, Vector3.zero, 12.0f * (float) TimeManager.TickDelta);
        }
        */

        if (!_inputManager.FireInput)
        {
            if (_bloomTimer <= 0)
            {
                SubtractBloom();
                if (CurrentWeaponInfo != null)
                    _bloomTimer = CurrentWeaponInfo.FireRate;
            }
            else
            {
                _bloomTimer -= (float) TimeManager.TickDelta;
            }
        }



        /* TODO: Fix the movement for the weapon holder after picking up a weapon.
        // Move the weapon holder to the player's location at a constant speed if the weapon is not equipped.
        if (!IsWeaponEquipped)
        {
            _weaponHolder.position = Vector3.MoveTowards(_weaponHolder.position, transform.position, 12.0f * (float) TimeManager.TickDelta);

            // If the weapon is close enough to the player, set the weapon holder to the player's position.
            if (Vector3.Distance(_weaponHolder.position, transform.position) < 1f)
            {
                EquipWeapon();
            }
        }
        */
    }

    #endregion

    #region Weapon Equipping

    private void HighlightClosestPickup()
    {
        if (_pickupsParent == null)
            return;

        _closestPickup = (float.MaxValue, null);

        // Iterate through all the children of the weapon pickups parent.
        foreach (Transform pickup in _pickupsParent.transform)
        {
            // TODO: Figure out how to make use of the common Pickup class.
            // Check if the child is a pickup, continue if not.
            if (pickup.TryGetComponent<WeaponPickupManager>(out var weaponPickupManager))
            {
                // Set the pickup as not highlighted.
                weaponPickupManager.SetHighlight(false);
            }

            if (pickup.TryGetComponent<HealthNutPickupManager>(out var healthNutManager))
            {
                // Set the pickup as not highlighted.
                healthNutManager.SetHighlight(false);
            }

            if (weaponPickupManager == null && healthNutManager == null)
                continue;

            // Get the distance to the pickup.
            float distance = Vector3.Distance(pickup.position, transform.position);

            // If the distance is greater than the pickup radius, continue.
            if (distance > _pickupRadius)
                continue;

            // If the distance  is less than the current closest distance, set the current pickup as the closest.
            if (distance < _closestPickup.Item1)
            {
                _closestPickup = (distance, weaponPickupManager == null ? healthNutManager : weaponPickupManager);
            }
        }

        // If there is a closest pickup, highlight it.
        if (_closestPickup.Item2 != null)
        {
            _closestPickup.Item2.SetHighlight(true);
        }
        else
        {
            _closestPickup= (float.MaxValue, null);
        }
    }

    private void CheckPickup()
    {
        // If there is no closest weapon pickup or the player is not pressing the interact button, return.
        if (_closestPickup.Item2 == null || !_inputManager.InteractInput)
            return;

        _currentPickupCooldown = 0.0f;

        // Send the pickup request to the server.
        if (base.IsHost)
        {
            PickupItem(_closestPickup.Item2.transform);
        }
        else
        {
            PickupItemServerRpc(_closestPickup.Item2.transform);
        }
    }

    [Server]
    private void PickupItem(Transform pickup)
    {
        // Try to get the weapon pickup manager from the pickup.
        if (pickup.TryGetComponent<WeaponPickupManager>(out var weaponPickupManager))
        {
            // Drop the current weapon.
            DropCurrentWeapon();

            // Set the player's weapon info to the pickup info.
            SetCurrentWeapon(weaponPickupManager.WeaponInfo);
            //SetCurrentWeaponObserversRpc(weaponPickupManager.WeaponInfo);

            /* TODO: Fix the movement for the weapon holder after picking up a weapon.
            IsWeaponEquipped = false;
            _weaponHolder.parent = null;
            _weaponHolder.position = pickup.position;
            _weaponHolder.rotation = pickup.rotation;
            */

            // Destroy the pickup.
            //InstanceFinder.ServerManager.Despawn(pickup.gameObject);
            Destroy(pickup.gameObject);
        }
        else if (pickup.TryGetComponent<HealthNutPickupManager>(out var healthNutManager))
        {
            // Add health to the player.
            PlayersManager.Instance.HealPlayer(base.Owner, healthNutManager.HealthAmount);

            //_playerManager.AddHealth(healthNutManager.HealthAmount);

            healthNutManager.Pickup();
        }
    }

    [ServerRpc]
    private void PickupItemServerRpc(Transform pickup)
    {
        PickupItem(pickup);
    }

    /// <summary>
    /// Called when the player dies and during pickups
    /// </summary>
    [Server]
    public void DropCurrentWeapon()
    {
        // If the player has no current weapon or if the current weapon is the default weapon, return.
        if (CurrentWeaponInfo == null || CurrentWeaponInfo.Name == _defaultWeaponInfo.Name)
            return;

        // Create a new weapon pickup based on the current weapon's info.
        GameObject weaponPickup = Instantiate(_weaponPickupPrefab, transform.position, Quaternion.identity, _pickupsParent.transform);

        // Set the weapon pickup's info.
        var weaponPickupManager = weaponPickup.GetComponent<WeaponPickupManager>();

        // Call initialize on the weapon pickup.
        weaponPickupManager.Initialize(CurrentWeaponInfo, _playerController.MovementData.Velocity);

        InstanceFinder.ServerManager.Spawn(weaponPickup);

        // Set the current weapon to null.
        CurrentWeaponInfo = null;
    }

    [Server]
    public void EquipDefaultWeapon()
    {
        SetCurrentWeapon(_defaultWeaponInfo);
    }

    [Server]
    private void SetCurrentWeapon(WeaponInfo weaponInfo)
    {
        // Set the current weapon to the weapon info.
        CurrentWeaponInfo = weaponInfo;
    }

    [ServerRpc]
    private void SetCurrentWeaponServerRpc(WeaponInfo weaponInfo)
    {
        //SetCurrentWeapon(weaponInfo);
        //SetCurrentWeaponObserversRpc(weaponInfo);
    }

    [ObserversRpc]
    private void SetCurrentWeaponObserversRpc(WeaponInfo weaponInfo)
    {
        //SetCurrentWeapon(weaponInfo);
    }

    private void EquipWeapon()
    {
        _weaponHolder.parent = transform;
        _weaponHolder.localPosition = Vector3.zero;
        IsWeaponEquipped = true;
    }

    #endregion

    #region Weapon Aiming

    private void UpdateWeaponHolderDirection()
    {
        // If the player is not aiming, return.
        _weaponHolder.rotation = Quaternion.LookRotation(Vector3.forward, _playerController.MovementData.AimDirection) * Quaternion.Euler(0f, 0f, 90f);
    }

    #endregion

    #region Weapon Firing

    // Invoked by the player controller
    [Client]
    public void Fire()
    {
        if (!base.IsOwner)
            return;

        // Need to set the bullet direction here because the player controller uses it to apply airborne knockback.

        // Play the weapon's fire sound.

        // Play the weapon's fire animation.


        // -- Setup --
        var bulletSpawnPosition = _weaponHolder.transform.position + (_playerController.MovementData.AimDirection * CurrentWeaponInfo.MuzzleLength);

        // -- Calculate bullet direction(s) --
        Vector3[] bulletDirections = new Vector3[CurrentWeaponInfo.BulletsPerShot];
        if (CurrentWeaponInfo.BulletsPerShot == 1)
        {
            Vector3 bloomDir = Quaternion.Euler(0f, 0f, Random.Range(-_currentBloomAngle, _currentBloomAngle)) * _playerController.MovementData.AimDirection;
            bulletDirections[0] = bloomDir;
        }
        else
        {
            for (int i = 0; i < CurrentWeaponInfo.BulletsPerShot; i++)
            {
                Vector3 randomDirection = Quaternion.Euler(0f, 0f, Random.Range(-CurrentWeaponInfo.SpreadAngle, CurrentWeaponInfo.SpreadAngle)) * _playerController.MovementData.AimDirection;

                bulletDirections[i] = randomDirection;
            }
        }

        // -- Draw the shot for the shooter --
        LayerMask environment = LayerMask.GetMask("Obstacle");
        for (int i = 0; i < bulletDirections.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(bulletSpawnPosition, bulletDirections[i], CurrentWeaponInfo.Range, environment);
            RaycastHit2D barrelStuff = Physics2D.Raycast(transform.position, bulletDirections[i], CurrentWeaponInfo.MuzzleLength, LayerMask.GetMask("Obstacle"));

            if (barrelStuff.collider is not null) continue;

            if (hit.collider is not null)
            {
                // -- Hit the environment, so draw a line to the hit point --
                DrawShot(bulletSpawnPosition, bulletDirections[i], hit.distance, CurrentWeaponInfo, true);
            }
            else
            {
                // -- Didn't hit anything, so draw a line to the end of the range --
                DrawShot(bulletSpawnPosition, bulletDirections[i], CurrentWeaponInfo.Range, CurrentWeaponInfo, false);
            }
        }

        // -- Get all the player hits --
        LayerMask hitbox = LayerMask.GetMask("Hitbox", "Obstacle");
        for (int i = 0; i < bulletDirections.Length; i++)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(bulletSpawnPosition, bulletDirections[i], CurrentWeaponInfo.Range, hitbox);
            RaycastHit2D barrelStuff = Physics2D.Raycast(transform.position, bulletDirections[i], CurrentWeaponInfo.MuzzleLength, LayerMask.GetMask("Obstacle"));

            // -- Damage the players --
            NetworkConnection recentHit = null;

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null) // Maybe check if the username is the same as the shooter
                {
                    if (barrelStuff.collider is not null && hit.distance > barrelStuff.distance) break;

                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle")) break;

                    var isHeadshot = hit.collider.gameObject.tag == "HeadHitbox";

                    var player = hit.transform.gameObject.GetComponentInParent<PlayerController>().gameObject;

                    var nob = player.GetComponent<NetworkObject>();

                    if (nob.Owner == base.Owner || nob.LocalConnection == recentHit) continue;

                    recentHit = nob.LocalConnection;

                    _hitSound.Play();

                    if (isHeadshot)
                    {
                        //_headshotSound.Play();
                    }
                }
            }
        }

        PreciseTick pt = base.TimeManager.GetPreciseTick(base.TimeManager.LastPacketTick);

        // -- Shoot on server -- 
        FireServer(pt, CurrentWeaponInfo, transform.position, bulletSpawnPosition, bulletDirections, _instanceID);

        // -- Increase bloom --
        AddBloom();

        // Get direction of all the bullets
        _weaponHolderController.OnFire(bulletDirections[0], CurrentWeaponInfo.Recoil, CurrentWeaponInfo.RecoilRecoveryRate);
    }

    [ServerRpc]
    public void FireServer(PreciseTick pt, WeaponInfo weapon, Vector3 playerPosition, Vector3 bulletSpawnPosition, Vector3[] bulletDirections, int instanceID)
    {
        if (weapon == null) return;

        // -- Play fire sound on observers --
        /*
        if (CurrentWeaponInfo.FireSoundPath != null)
            PlayFireSoundObservers(CurrentWeaponInfo.FireSoundPath, playerPosition);
        */

        // -- Rollback the colliders --
        base.RollbackManager.Rollback(pt, RollbackPhysicsType.Physics2D, base.IsOwner);

        // -- Get all the environment hits --
        LayerMask environment = LayerMask.GetMask("Obstacle");
        for (int i = 0; i < bulletDirections.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(bulletSpawnPosition, bulletDirections[i], weapon.Range, environment);
            RaycastHit2D barrelStuff = Physics2D.Raycast(playerPosition, bulletDirections[i], weapon.MuzzleLength, LayerMask.GetMask("Obstacle"));

            if (barrelStuff.collider is not null) continue;

            // -- Draw the shots for the other players --
            if (hit.collider is not null)
            {
                // -- Hit the environment, so draw a line to the hit point --
                //if (!base.IsHost)
                    //DrawShot(bulletSpawnPosition, bulletDirections[i], hit.distance, weapon.Range, true);

                DrawShotObservers(bulletSpawnPosition, bulletDirections[i], hit.distance, weapon, true);
            }
            else
            {
                // -- Didn't hit anything, so draw a line to the end of the range --
                //if (!base.IsHost)
                    //DrawShot(bulletSpawnPosition, bulletDirections[i], weapon.Range, weapon.Range, false);

                DrawShotObservers(bulletSpawnPosition, bulletDirections[i], weapon.Range, weapon, false);
            }
        }

        // -- Get all the player hits --
        LayerMask hitbox = LayerMask.GetMask("Hitbox", "Obstacle");
        for (int i = 0; i < bulletDirections.Length; i++)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(bulletSpawnPosition, bulletDirections[i], weapon.Range, hitbox);
            RaycastHit2D barrelStuff = Physics2D.Raycast(playerPosition, bulletDirections[i], weapon.MuzzleLength, LayerMask.GetMask("Obstacle"));

            // -- Damage the players --
            NetworkConnection recentHit = null;

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null) // Maybe check if the username is the same as the shooter
                {
                    if (barrelStuff.collider is not null && hit.distance > barrelStuff.distance) break;

                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle")) break;

                    var isHeadshot = hit.collider.gameObject.tag == "HeadHitbox";

                    var player = hit.transform.gameObject.GetComponentInParent<PlayerController>().gameObject;

                    //if (player.gameObject.GetInstanceID() == instanceID) continue;

                    var nob = player.GetComponent<NetworkObject>();

                    if (nob.Owner == base.Owner || nob.LocalConnection == recentHit) continue;

                    recentHit = nob.LocalConnection;

                    // -- Damage the player --
                    //PlayerManager.Instance.DamagePlayer(nob.Owner, weapon.Damage, gameObject.GetInstanceID(), weapon.Name, nob.LocalConnection);
                    PlayersManager.Instance.DamagePlayer(nob.Owner, base.Owner, weapon, isHeadshot, hit.point);
                }
            }
        }

        // -- Return the colliders --
        base.RollbackManager.Return();
    }


    private void DrawShot(Vector3 origin, Vector3 direction, float distance, WeaponInfo weaponInfo, bool hitSomething)
    {
        TrailRenderer bulletTrail = Instantiate(_bulletTrailRenderer, origin, Quaternion.identity);
        bulletTrail.time = (distance / weaponInfo.Range) * 0.1f;

        float shotAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        var muzzleFlash = Instantiate(_muzzleFlashPrefab, origin, Quaternion.identity);
        ParticleSystem.ShapeModule shape = muzzleFlash.GetComponentInChildren<ParticleSystem>().shape;
        shape.rotation = new Vector3(-shotAngle, 90f, 0f);

        Destroy(muzzleFlash, 0.1f);

        // Play shot sound
        if (weaponInfo.FireSoundPath != null)
        {
            AudioClip shotSound = Resources.Load<AudioClip>(weaponInfo.FireSoundPath);
            if (base.IsOwner)
            {
                _shotSound.clip = shotSound;
                _shotSound.PlayOneShot(shotSound);
            }
            else
            {
                AudioSource.PlayClipAtPoint(shotSound, origin);
            }
        }

        StartCoroutine(ShootCoroutine(origin, direction, distance, bulletTrail, hitSomething));
    }

    [ObserversRpc]
    public void DrawShotObservers(Vector3 origin, Vector3 direction, float distance, WeaponInfo weaponInfo, bool hitSomething)
    {
        // -- Owner of shot check --
        if (base.IsOwner) return;

        DrawShot(origin, direction, distance, weaponInfo, hitSomething);

        /*
        // Draw bullet trail
        TrailRenderer bulletTrail = Instantiate(_bulletTrailRenderer, origin, Quaternion.identity);
        bulletTrail.time = (distance / weaponInfo.Range) * 0.1f;

        // Draw muzzle flash
        float shotAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        var muzzleFlash = Instantiate(_muzzleFlashPrefab, origin, Quaternion.identity);
        ParticleSystem.ShapeModule shape = muzzleFlash.GetComponentInChildren<ParticleSystem>().shape;
        shape.rotation = new Vector3(-shotAngle, 90f, 0f);

        Destroy(muzzleFlash, 0.1f);

        // Play shot sound
        if (CurrentWeaponInfo.FireSoundPath != null)
        {
            AudioClip shotSound = Resources.Load<AudioClip>(CurrentWeaponInfo.FireSoundPath);
            AudioSource.PlayClipAtPoint(shotSound, origin);
        }

        StartCoroutine(ShootCoroutine(origin, direction, distance, bulletTrail, hitSomething));
        */
    }

    private IEnumerator ShootCoroutine(Vector3 position, Vector3 direction, float distance, TrailRenderer bulletTrail, bool hitSomething)
    {
        //_weaponEquipManager.CurrentWeapon.ShowMuzzleFlash();

        float time = 0;
        Vector3 startPosition = bulletTrail.transform.position;
        Vector3 endPosition = position + direction * distance;

        while (time < 1f)
        {
            bulletTrail.transform.position = Vector3.Lerp(startPosition, endPosition, time);
            time += Time.deltaTime / bulletTrail.time;

            yield return null;
        }

        if (hitSomething)
        {
            // -- Play hit sound --

            // -- Play hit particle effect --

        }


        Destroy(bulletTrail.gameObject, bulletTrail.time);
    }

    private void AddBloom()
    {
        if (CurrentWeaponInfo.BulletsPerShot != 1) return;

        _currentBloomAngle = Mathf.Clamp(_currentBloomAngle + CurrentWeaponInfo.BloomAngleIncreasePerShot, 0f, CurrentWeaponInfo.MaxBloomAngle);
    }

    private void SubtractBloom()
    {
        if (CurrentWeaponInfo == null || CurrentWeaponInfo.BulletsPerShot != 1) return;

        _currentBloomAngle = Mathf.Clamp(_currentBloomAngle - (CurrentWeaponInfo.BloomAngleIncreasePerShot * 1.5f), 0f, CurrentWeaponInfo.MaxBloomAngle);
    }

    #endregion

    #region Events

    private void OnModeChange(Mode mode)
    {
        var spriteActive = mode == Mode.Shoot;
        _weaponHolder.GetChild(0).gameObject.SetActive(spriteActive);

        OnModeChangeObserversRpc(mode);
    }

    [ObserversRpc]
    private void OnModeChangeObserversRpc(Mode mode)
    {
        var spriteActive = mode == Mode.Shoot;
        _weaponHolder.GetChild(0).gameObject.SetActive(spriteActive);
    }

    #endregion
}
