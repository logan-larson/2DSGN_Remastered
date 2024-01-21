using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickupManager : NetworkBehaviour
{
    #region Public Fields

    public WeaponInfo WeaponInfo;
    public Vector2 InitialVelocity = Vector2.zero;

    #endregion

    #region Serialized Fields

    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private Rigidbody2D _rigidbody;

    #endregion

    #region Initialization

    public void Initialize(WeaponInfo weaponInfo, Vector2 initialVelocity)
    {
        WeaponInfo = weaponInfo;
        InitialVelocity = initialVelocity;

        _rigidbody.AddForce(InitialVelocity, ForceMode2D.Impulse);

        _spriteRenderer.sprite = WeaponInfo.Sprite;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        Initialize(WeaponInfo, InitialVelocity);
    }

    #endregion

    [SyncVar]
    public bool IsAvailable = true;


    public void SetHighlight(bool highlight)
    {
        _spriteRenderer.color = highlight ? Color.yellow : Color.white;
    }

}
