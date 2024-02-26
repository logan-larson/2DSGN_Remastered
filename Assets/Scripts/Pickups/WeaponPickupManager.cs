using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickupManager : Pickup
{
    #region Public Fields

    [SyncVar]
    public WeaponInfo WeaponInfo;

    #endregion

    #region Initialization

    public void Initialize(WeaponInfo weaponInfo)
    {
        if (weaponInfo == null)
            return;

        WeaponInfo = weaponInfo;
    }

    public override void OnSpawnServer(NetworkConnection connection)
    {
        base.OnSpawnServer(connection);

        base.SpriteRenderer.sprite = Resources.Load<Sprite>(WeaponInfo.SpritePath);

        SetSpriteObserversRpc();
    }

    [ObserversRpc]
    private void SetSpriteObserversRpc()
    {
        if (WeaponInfo == null)
            return;

        base.SpriteRenderer.sprite = Resources.Load<Sprite>(WeaponInfo.SpritePath);
    }

    #endregion
}
