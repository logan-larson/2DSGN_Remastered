using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : NetworkBehaviour
{
    [SerializeField]
    protected SpriteRenderer SpriteRenderer;

    [SyncVar (OnChange = nameof(OnIsAvailableChanged))]
    public bool IsAvailable = true;

    private void OnIsAvailableChanged(bool oldValue, bool newValue, bool asServer)
    {
        SetDisabled(newValue);
    }

    public void SetHighlight(bool isHighlighted)
    {
        SpriteRenderer.color = isHighlighted ? Color.yellow : Color.white;
    }

    public void SetDisabled(bool isAvailable)
    {
        SpriteRenderer.color = isAvailable ? Color.white : Color.gray;
    }
}
