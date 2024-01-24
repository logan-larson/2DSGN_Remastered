using FishNet.Object;
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
        while (Vector3.Distance(transform.localPosition, Vector3.zero) > 0.01f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, recoilRecoveryRate);
            yield return null;
        }
    }
}
