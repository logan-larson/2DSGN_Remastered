using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthNutPickupManager : Pickup
{
    public float HealthAmount = 100f;

    [SyncVar(OnChange = nameof(ToggleIsAvailable))]
    public bool IsAvailable = true;

    private void ToggleIsAvailable(bool oldValue, bool newValue, bool isServer)
    {
        if (newValue)
        {
            SpriteRenderer.enabled = true;
        }
        else
        {
            SpriteRenderer.enabled = false;
        }
    }

    public void Pickup()
    {
        if (!base.IsServer) return;

        IsAvailable = false;

        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(5);

        IsAvailable = true;
    }
}
