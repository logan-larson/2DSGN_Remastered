using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickupManager : MonoBehaviour
{
    public WeaponPickupInfo WeaponPickupInfo;

    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer.sprite = WeaponPickupInfo.Sprite;
    }

    [SyncVar]
    public bool IsAvailable = true;
}
