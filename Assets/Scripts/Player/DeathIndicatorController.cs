using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathIndicatorController : MonoBehaviour
{
    private float _lifetime = 1f;
    private float _currentLifetime = 0f;

    private Vector3 _velocity = new Vector3(0f, 2f);

    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        Destroy(gameObject, _lifetime);
    }

    private void Update()
    {
        byte alpha = (byte)(200 - (255 * (_currentLifetime / _lifetime)));
        _spriteRenderer.color = new Color32(140, 140, 140, alpha);

        var position = transform.position + _velocity * Time.deltaTime;
        transform.SetPositionAndRotation(position, Quaternion.identity);
    }
}
