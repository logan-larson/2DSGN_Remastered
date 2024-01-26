using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageIndicatorManager : MonoBehaviour
{
    private TMP_Text _damageValue;

    private float _initialYVelocityRange = 2f;
    private float _initialXVelocityRange = 2f;
    private float _lifetime = 0.5f;

    [SerializeField]
    private float _basePitch = 0.5f;

    private Vector3 _velocity;

    [SerializeField]
    private AudioSource _hit;

    [SerializeField]
    private AudioSource _death;

    private void Awake()
    {
        _damageValue = GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        _velocity = new Vector3(Random.Range(-_initialXVelocityRange, _initialXVelocityRange), Random.Range(-_initialYVelocityRange, _initialYVelocityRange)).normalized * 2f;

        Destroy(gameObject, _lifetime);
    }

    private void Update()
    {
        var position = transform.position + _velocity * Time.deltaTime;
        transform.SetPositionAndRotation(position, Quaternion.identity);
    }

    public void Initialize(int damage, float health)
    {
        _damageValue.text = damage.ToString();

        if (health <= 0f)
        {
            _death.Play();
            return;
            //_hit.volume = 0.75f;
        }

        //_hit.pitch = ((100f - health) / 100f) + _basePitch;

        _hit.Play();
    }

    public void SetColor(Color color)
    {
        _damageValue.color = color;
    }
}
