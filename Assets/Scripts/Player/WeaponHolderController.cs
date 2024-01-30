using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHolderController : NetworkBehaviour
{

    #region Public Fields

    public Vector3 TargetPosition;

    #endregion

    #region Private Fields

    private bool _subscribedToTimeManager = false;

    [SerializeField]
    private SpriteRenderer _weaponSprite;

    [SerializeField]
    private RectTransform _crosshair;

    [SerializeField]
    private Vector3 _originalPosition;

    #endregion

    private bool _previousFlipY = false;

    [SyncVar (OnChange = nameof(OnFlipYChanged))]
    private bool _flipY = false;

    private void OnFlipYChanged(bool oldValue, bool newValue, bool asServer)
    {
        _weaponSprite.flipY = newValue;
    }

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

    #endregion

    #region Initialization

    public override void OnStartClient()
    {
        base.OnStartClient();

        SubscribeToTimeManager(true);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        SubscribeToTimeManager(false);
    }

    #endregion

    #region Frame Updates

    private void OnTick()
    {
        // Update the flipY based on the crosshair position relative to the player rotation
        // If the crosshair is to the left of the player, flip the weapon
        // If the crosshair is to the right of the player, don't flip the weapon
        if (!base.IsOwner)
            return;

        var flipY = _crosshair.localPosition.x < 0;

        if (_previousFlipY != flipY)
        {
            if (base.IsServer)
                _flipY = flipY;
            else
                SetFlipYServerRpc(flipY);

            _previousFlipY = flipY;
        }

    }

    [ServerRpc]
    private void SetFlipYServerRpc(bool flipY)
    {
        _flipY = flipY;
    }

    #endregion

    public void OnFire(Vector3 shotDirection, float recoil, float recoilRecoveryRate)
    {
        StopAllCoroutines();

        var localShotDirection = transform.parent.InverseTransformDirection(shotDirection);

        Vector3 localRecoilPosition = localShotDirection * -recoil;

        StartCoroutine(Recoil(localRecoilPosition, recoilRecoveryRate));
    }

    private IEnumerator Recoil(Vector3 recoilPosition, float recoilRecoveryRate)
    {
        // Move to Recoiled Position
        while (Vector3.Distance(transform.localPosition, recoilPosition) > 0.01f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, recoilPosition, recoilRecoveryRate * 1.5f);
            yield return null;
        }

        // Return to Original Position
        while (Vector3.Distance(transform.localPosition, _originalPosition) > 0.01f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, _originalPosition, recoilRecoveryRate);
            yield return null;
        }
    }
}
