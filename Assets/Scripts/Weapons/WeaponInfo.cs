using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponInfo", menuName = "SGN/WeaponInfo", order = 4)]
public class WeaponInfo : ScriptableObject
{
    public string Name;

    //public Sprite Sprite;
    public string SpritePath;
    public string FireSoundPath;
    public string BulletTrailRendererPath;

    public float FireRate;
    public float Damage;
    public float Range;
    public float HeadshotMultiplier;

    public bool IsAutomatic;

    public int BulletsPerShot;
    public float SpreadAngle;
    public float MaxBloomAngle;
    public float BloomAngleIncreasePerShot;

    public float Recoil;
    public float RecoilRecoveryRate;

    public float AirborneKnockback;

    public float MuzzleLength;
}
