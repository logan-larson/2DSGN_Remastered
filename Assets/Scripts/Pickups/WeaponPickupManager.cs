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

    [SyncVar]
    public Vector2 InitialVelocity;

    #endregion

    #region Serialized Fields

    [SerializeField]
    private Rigidbody2D _rigidbody;

    #endregion

    #region Initialization

    public void Initialize(WeaponInfo weaponInfo, Vector2 initialVelocity)
    {
        if (weaponInfo == null)
            return;

        WeaponInfo = weaponInfo;
        InitialVelocity = initialVelocity;
    }

    public override void OnSpawnServer(NetworkConnection connection)
    {
        base.OnSpawnServer(connection);

        _rigidbody.AddForce(InitialVelocity, ForceMode2D.Impulse);
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
