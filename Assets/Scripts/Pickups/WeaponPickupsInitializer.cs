using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickupsInitializer : MonoBehaviour
{
    private void Start()
    {
        foreach (Transform pickup in transform)
        {
            if (pickup.TryGetComponent<WeaponPickupManager>(out var weaponPickupManager))
            {
                weaponPickupManager.Initialize(weaponPickupManager.WeaponInfo, Vector2.zero);
            }
        }
        
    }
}
