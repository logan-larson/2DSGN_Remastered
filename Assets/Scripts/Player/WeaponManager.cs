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

    public WeaponInfo CurrentWeaponInfo = null;

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

    #endregion

    #region Private Fields

    private bool _subscribedToTimeManager = false;

    // The parent object of all weapon pickups.
    private GameObject _weaponPickupsParent;

    private (float, WeaponPickupManager) _closestWeaponPickup;

    private float _currentPickupCooldown = 0.0f;

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
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        //SetCurrentWeapon(_defaultWeaponInfo);

        _weaponPickupsParent = GameObject.Find("WeaponPickups");

        if (_weaponPickupsParent == null)
        {
            Debug.LogError("WeaponPickups parent not found.");
        }

        _weaponHolder.GetChild(0).gameObject.SetActive(_playerController.PublicData.Mode == Mode.Shoot);

        _playerController.OnModeChange.AddListener(OnModeChange);

        _instanceID = gameObject.GetInstanceID();
    }

    // When the client starts, set the weapon pickups parent.
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner)
        {
            SetCurrentWeaponServerRpc(_defaultWeaponInfo);
        }

        _weaponPickupsParent = GameObject.Find("WeaponPickups");

        if (_weaponPickupsParent == null)
        {
            Debug.LogError("WeaponPickups parent not found.");
        }

        _weaponHolder.GetChild(0).gameObject.SetActive(_playerController.PublicData.Mode == Mode.Shoot);
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

        if (!_playerController.PublicData.IsFiring)
        {
            if (_bloomTimer < 0)
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
        if (_weaponPickupsParent == null)
            return;

        _closestWeaponPickup = (float.MaxValue, null);

        // Iterate through all the children of the weapon pickups parent.
        foreach (Transform pickup in _weaponPickupsParent.transform)
        {
            // Check if the child is a weapon pickup, continue if not.
            if (!pickup.TryGetComponent<WeaponPickupManager>(out var weaponPickupManager))
            {
                continue;
            }

            // Set the pickup as not highlighted.
            weaponPickupManager.SetHighlight(false);

            // Get the distance to the pickup.
            float distance = Vector3.Distance(pickup.position, transform.position);

            // If the distance is greater than the pickup radius, continue.
            if (distance > _pickupRadius)
                continue;

            // If the distance  is less than the current closest distance, set the current pickup as the closest.
            if (distance < _closestWeaponPickup.Item1)
            {
                _closestWeaponPickup = (distance, weaponPickupManager);
            }
        }

        // If there is a closest pickup, highlight it.
        if (_closestWeaponPickup.Item2 != null)
        {
            _closestWeaponPickup.Item2.SetHighlight(true);
        }
        else
        {
            _closestWeaponPickup= (float.MaxValue, null);
        }
    }

    private void CheckPickup()
    {
        // If there is no closest weapon pickup or the player is not pressing the interact button, return.
        if (_closestWeaponPickup.Item2 == null || !_inputManager.InteractInput)
            return;

        _currentPickupCooldown = 0.0f;

        // Send the pickup request to the server.
        if (base.IsHost)
        {
            PickupWeapon(_closestWeaponPickup.Item2.transform);
        }
        else
        {
            PickupWeaponServerRpc(_closestWeaponPickup.Item2.transform);
        }
    }

    [Server]
    private void PickupWeapon(Transform pickup)
    {
        // Try to get the weapon pickup manager from the pickup.
        if (!pickup.TryGetComponent<WeaponPickupManager>(out var weaponPickupManager))
        {
            return;
        }

        // Drop the current weapon.
        DropCurrentWeapon();

        // Set the player's weapon info to the pickup info.
        SetCurrentWeapon(weaponPickupManager.WeaponInfo);
        SetCurrentWeaponObserversRpc(weaponPickupManager.WeaponInfo);

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

    [ServerRpc]
    private void PickupWeaponServerRpc(Transform pickup)
    {
        PickupWeapon(pickup);
    }

    /// <summary>
    /// Called when the player dies and during pickups
    /// </summary>
    [Server]
    private void DropCurrentWeapon()
    {
        // If the player has no current weapon or if the current weapon is the default weapon, return.
        if (CurrentWeaponInfo == null || CurrentWeaponInfo == _defaultWeaponInfo)
            return;

        // Create a new weapon pickup based on the current weapon's info.
        GameObject weaponPickup = Instantiate(_weaponPickupPrefab, transform.position, Quaternion.identity, _weaponPickupsParent.transform);

        // Set the weapon pickup's info.
        var weaponPickupManager = weaponPickup.GetComponent<WeaponPickupManager>();

        // Call initialize on the weapon pickup.
        weaponPickupManager.Initialize(CurrentWeaponInfo, _playerController.PublicData.Velocity);

        InstanceFinder.ServerManager.Spawn(weaponPickup);

        // Set the current weapon to null.
        CurrentWeaponInfo = null;
    }

    private void SetCurrentWeapon(WeaponInfo weaponInfo)
    {
        // Set the current weapon to the weapon info.
        CurrentWeaponInfo = weaponInfo;

        // TODO: Initialize other things like the weapon sprite, etc.
        _weaponHolder.GetComponentInChildren<SpriteRenderer>(true).sprite = Resources.Load<Sprite>(weaponInfo.SpritePath);
    }

    [ServerRpc]
    private void SetCurrentWeaponServerRpc(WeaponInfo weaponInfo)
    {
        SetCurrentWeapon(weaponInfo);
        SetCurrentWeaponObserversRpc(weaponInfo);
    }

    [ObserversRpc]
    private void SetCurrentWeaponObserversRpc(WeaponInfo weaponInfo)
    {
        SetCurrentWeapon(weaponInfo);
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
        _weaponHolder.rotation = Quaternion.LookRotation(Vector3.forward, _playerController.PublicData.AimDirection) * Quaternion.Euler(0f, 0f, 90f);
    }

    #endregion

    #region Weapon Firing

    // Invoked by the player controller
    public void Fire()
    {
        if (!base.IsOwner)
            return;

        // Need to set the bullet direction here because the player controller uses it to apply airborne knockback.

        // Play the weapon's fire sound.

        // Play the weapon's fire animation.


        // -- Setup --
        var bulletSpawnPosition = _weaponHolder.transform.position + (_playerController.PublicData.AimDirection * CurrentWeaponInfo.MuzzleLength);

        // -- Calculate bullet direction(s) --
        Vector3[] bulletDirections = new Vector3[CurrentWeaponInfo.BulletsPerShot];
        if (CurrentWeaponInfo.BulletsPerShot == 1)
        {
            Vector3 bloomDir = Quaternion.Euler(0f, 0f, Random.Range(-_currentBloomAngle, _currentBloomAngle)) * _playerController.PublicData.AimDirection;
            bulletDirections[0] = bloomDir;
        }
        else
        {
            for (int i = 0; i < CurrentWeaponInfo.BulletsPerShot; i++)
            {
                Vector3 randomDirection = Quaternion.Euler(0f, 0f, Random.Range(-CurrentWeaponInfo.SpreadAngle, CurrentWeaponInfo.SpreadAngle)) * _playerController.PublicData.AimDirection;

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
                DrawShot(bulletSpawnPosition, bulletDirections[i], hit.distance);
            }
            else
            {
                // -- Didn't hit anything, so draw a line to the end of the range --
                DrawShot(bulletSpawnPosition, bulletDirections[i], CurrentWeaponInfo.Range);
            }
        }

        PreciseTick pt = base.TimeManager.GetPreciseTick(base.TimeManager.LastPacketTick);

        // -- Shoot on server -- 
        ShootServer(pt, CurrentWeaponInfo, transform.position, bulletSpawnPosition, bulletDirections, _instanceID);

        // -- Increase bloom --
        AddBloom();

    }

    [ServerRpc]
    public void ShootServer(PreciseTick pt, WeaponInfo weapon, Vector3 playerPosition, Vector3 bulletSpawnPosition, Vector3[] bulletDirections, int instanceID)
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
                if (!base.IsHost)
                    DrawShot(bulletSpawnPosition, bulletDirections[i], hit.distance);

                DrawShotObservers(bulletSpawnPosition, bulletDirections[i], hit.distance);
            }
            else
            {
                // -- Didn't hit anything, so draw a line to the end of the range --
                if (!base.IsHost)
                    DrawShot(bulletSpawnPosition, bulletDirections[i], weapon.Range);

                DrawShotObservers(bulletSpawnPosition, bulletDirections[i], weapon.Range);
            }
        }

        // -- Get all the player hits --
        LayerMask hitbox = LayerMask.GetMask("Hitbox", "Obstacle");
        for (int i = 0; i < bulletDirections.Length; i++)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(bulletSpawnPosition, bulletDirections[i], weapon.Range, hitbox);
            RaycastHit2D barrelStuff = Physics2D.Raycast(playerPosition, bulletDirections[i], weapon.MuzzleLength, LayerMask.GetMask("Obstacle"));

            // -- Damage the players --
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null) // Maybe check if the username is the same as the shooter
                {
                    if (barrelStuff.collider is not null && hit.distance > barrelStuff.distance) break;

                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle")) break;

                    var player = hit.transform.parent;

                    if (player.gameObject.GetInstanceID() == instanceID) continue;

                    var nob = player.GetComponent<NetworkObject>();

                    // -- Damage the player --
                    //PlayerManager.Instance.DamagePlayer(player.gameObject.GetInstanceID(), weapon.Damage, gameObject.GetInstanceID(), weapon.Name, nob.LocalConnection);
                }
            }
        }

        // -- Return the colliders --
        base.RollbackManager.Return();
    }


    private void DrawShot(Vector3 origin, Vector3 direction, float distance)
    {
        TrailRenderer bulletTrail = Instantiate(_bulletTrailRenderer, origin, Quaternion.identity);

        StartCoroutine(ShootCoroutine(origin, direction, distance, bulletTrail));
    }

    [ObserversRpc]
    public void DrawShotObservers(Vector3 origin, Vector3 direction, float distance)
    {
        // -- Owner of shot check --
        if (base.IsOwner) return;

        TrailRenderer bulletTrail = Instantiate(_bulletTrailRenderer, origin, Quaternion.identity);

        StartCoroutine(ShootCoroutine(origin, direction, distance, bulletTrail));
    }

    private IEnumerator ShootCoroutine(Vector3 position, Vector3 direction, float distance, TrailRenderer bulletTrail)
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
