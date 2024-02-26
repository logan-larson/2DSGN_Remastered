using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickupsInitializer : NetworkBehaviour
{
    public override void OnStartServer()
    {
        foreach (Transform pickup in transform)
        {
            if (pickup.TryGetComponent<WeaponPickupManager>(out var weaponPickupManager))
            {
                weaponPickupManager.Initialize(weaponPickupManager.WeaponInfo);
            }
        }
        
    }
}
