using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    #region Public Fields

    public bool IsWeaponEquipped;

    #endregion

    #region Serialized Fields

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

    #endregion

    #region Private Fields

    private bool _subscribedToTimeManager = false;

    // The parent object of all weapon pickups.
    private GameObject _weaponPickupsParent;

    private (float, WeaponPickupManager) _closestWeaponPickup;

    [SerializeField]
    private WeaponInfo _currentWeaponInfo;

    private float _currentPickupCooldown = 0.0f;

    #endregion

    #region Script References

    [SerializeField]
    private InputManager _inputManager;

    [SerializeField]
    private MovementManager _movementManager;

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
        _movementManager ??= GetComponent<MovementManager>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _weaponPickupsParent = GameObject.Find("WeaponPickups");

        if (_weaponPickupsParent == null)
        {
            Debug.LogError("WeaponPickups parent not found.");
        }
    }

    // When the client starts, set the weapon pickups parent.
    public override void OnStartClient()
    {
        base.OnStartClient();

        _weaponPickupsParent = GameObject.Find("WeaponPickups");

        if (_weaponPickupsParent == null)
        {
            Debug.LogError("WeaponPickups parent not found.");
        }
    }

    #endregion

    #region Frame Updates

    private void OnTick()
    {
        // Perform all actions on the client and notify server with RPCs.
        if (!base.IsOwner)
            return;

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
        if (_currentWeaponInfo == null || _currentWeaponInfo == _defaultWeaponInfo)
            return;

        // Create a new weapon pickup based on the current weapon's info.
        GameObject weaponPickup = Instantiate(_weaponPickupPrefab, transform.position, Quaternion.identity, _weaponPickupsParent.transform);

        // Set the weapon pickup's info.
        var weaponPickupManager = weaponPickup.GetComponent<WeaponPickupManager>();

        // Call initialize on the weapon pickup.
        weaponPickupManager.Initialize(_currentWeaponInfo, _movementManager.PublicData.Velocity);

        InstanceFinder.ServerManager.Spawn(weaponPickup);

        // Set the current weapon to null.
        _currentWeaponInfo = null;
    }

    private void SetCurrentWeapon(WeaponInfo weaponInfo)
    {
        // Set the current weapon to the weapon info.
        _currentWeaponInfo = weaponInfo;

        // TODO: Initialize other things like the weapon sprite, etc.
        _weaponHolder.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>(weaponInfo.SpritePath);
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
}
