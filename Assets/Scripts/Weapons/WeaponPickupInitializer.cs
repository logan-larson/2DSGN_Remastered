using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickupInitializer : MonoBehaviour
{
    void Start()
    {
        foreach (Transform pickup in transform)
        {
            var weaponPickupManager = pickup.GetComponent<WeaponPickupManager>();
            weaponPickupManager.Initialize(weaponPickupManager.WeaponInfo, Vector2.zero);
        }
        
    }
}
