using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private float _pickupRadius = 5.0f;

    #endregion

    #region Private Fields

    private bool _subscribedToTimeManager = false;

    // The parent object of all weapon pickups.
    private GameObject _weaponPickupsParent;


    private WeaponPickupInfo _closestWeaponPickupInfo;

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

    // When the client starts, set the weapon pickups parent.
    public override void OnStartClient()
    {
        base.OnStartClient();

        _weaponPickupsParent = GameObject.Find("WeaponPickups");
    }

    #endregion


    private void OnTick()
    {
        // Perform all actions on the client and notify server with RPCs.
        if (!base.IsOwner)
            return;

        // Highlight the closest weapon pickup. This only occurs client side.
        HighlightClosestPickup();

        // If the player presses the pickup button, try to pickup the weapon.
    }

    private void HighlightClosestPickup()
    {
        (float, WeaponPickupManager) closestPickup = (float.MaxValue, null);

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
            if (distance < closestPickup.Item1)
            {
                closestPickup = (distance, weaponPickupManager);
            }
        }

        // If there is a closest pickup, highlight it.
        if (closestPickup.Item2 != null)
        {
            closestPickup.Item2.SetHighlight(true);
            _closestWeaponPickupInfo = closestPickup.Item2.WeaponPickupInfo;
        }
        else
        {
            _closestWeaponPickupInfo = null;
        }
    }

}
