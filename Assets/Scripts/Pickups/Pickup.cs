using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : NetworkBehaviour
{
    [SerializeField]
    protected SpriteRenderer SpriteRenderer;

    [SerializeField]
    protected Transform SpriteTransform;

    [SyncVar (OnChange = nameof(OnIsAvailableChanged))]
    public bool IsAvailable = true;

    private void OnIsAvailableChanged(bool oldValue, bool newValue, bool asServer)
    {
        SetDisabled(newValue);
    }

    [SerializeField]
    private float _floatAmplitude = 0.002f;

    [SerializeField]
    private float _floatFrequency = 1f;

    [SerializeField]
    private float _floatOffset;

    private Vector3 _tempPos = new Vector3();

    private void Start()
    {
        // Randomize the float offset so that pickups don't float in sync
        _floatOffset = Random.Range(0f, 1f);
    }

    private void Update()
    {
        // Float up and down
        _tempPos = SpriteTransform.position;
        _tempPos.y += Mathf.Sin((Time.time + _floatOffset) * _floatFrequency) * _floatAmplitude;

        SpriteTransform.position = _tempPos;
    }

    public void SetHighlight(bool isHighlighted)
    {
        if (!IsAvailable)
            return;

        SpriteRenderer.color = isHighlighted ? Color.yellow : Color.white;
    }

    public void SetDisabled(bool isAvailable)
    {
        SpriteRenderer.color = isAvailable ? Color.white : new Color32(255, 255, 255, 140);
    }
}
