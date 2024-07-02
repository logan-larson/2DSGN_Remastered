using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
<<<<<<< HEAD
=======
//using UnityEditor.Animations;
>>>>>>> the-revert-pt2
using UnityEngine;

public class WeaponPickupManager : Pickup
{
    #region Public Fields

    [SyncVar]
    public WeaponInfo WeaponInfo;

    #endregion

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private AnimatorOverrideController _overrideController;

    #region Initialization

    public void Initialize(WeaponInfo weaponInfo)
    {
        if (weaponInfo == null)
            return;

        WeaponInfo = weaponInfo;

        //LoadAnimation(weaponInfo.SpinningAnimationPath);
    }

    public override void OnSpawnServer(NetworkConnection connection)
    {
        base.OnSpawnServer(connection);

        LoadAnimation(WeaponInfo.SpinningAnimationPath);

        SetSpriteObserversRpc();
    }

    private void LoadAnimation(string path)
    {
        AnimationClip clip = Resources.Load<AnimationClip>(path);

        if (clip == null) return;

        if (_overrideController == null)
        {
            _overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
            _animator.runtimeAnimatorController = _overrideController;
        }

        _overrideController["RevolverSpinning"] = clip;

        _animator.Play("RevolverSpinning");
    }

    [ObserversRpc]
    private void SetSpriteObserversRpc()
    {
        if (WeaponInfo == null)
            return;

        LoadAnimation(WeaponInfo.SpinningAnimationPath);
    }

    #endregion
}
