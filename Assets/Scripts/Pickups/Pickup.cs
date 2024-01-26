using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : NetworkBehaviour
{
    [SerializeField]
    protected SpriteRenderer SpriteRenderer;

    public void SetHighlight(bool isHighlighted)
    {
        SpriteRenderer.color = isHighlighted ? Color.yellow : Color.white;
    }
}
