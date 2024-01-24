using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using UnityEngine.InputSystem;

public class CrosshairController : NetworkBehaviour
{
    private RectTransform _crosshair;
    [SerializeField]
    private WeaponManager _weaponManager;
    private WeaponInfo _weapon;
    private CameraController _cameraController;
    [SerializeField]
    private InputManager _inputManager;
    //private InputSystem _input;

    [SerializeField]
    private float _minSize = 5f;

    [SerializeField]
    private float _sizeMultiplier = 15f;

    private void ChangeWeapon()
    {
        _weapon = _weaponManager.CurrentWeaponInfo;
    }

    private void Start()
    {
        _crosshair = GetComponent<RectTransform>();
        //_input = GetComponentInParent<InputManager>();

        _cameraController = Camera.main.GetComponent<CameraController>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        //_crosshair = GetComponent<RectTransform>();

        //_weaponManager = _weaponManager ?? GetComponentInParent<WeaponEquipManager>();

        //_weaponManager.ChangeWeapon.AddListener(ChangeWeapon);

        //ChangeWeapon();

        //_camera = _weaponManager.transform.GetComponent<CameraManager>().Camera;
    }

    private void Update()
    {
        if (_weaponManager.CurrentWeaponInfo == null) return;

        var size = Mathf.Max(_minSize * _weaponManager.CurrentWeaponInfo.MaxBloomAngle, _weaponManager.Bloom * _sizeMultiplier);

        _crosshair.sizeDelta = new Vector2(size, size);

        if (_inputManager.InputDevice == "Gamepad")
        {
            if (_inputManager.Aim != Vector2.zero)
            {
                var aimDirection = Camera.main.transform.rotation * _inputManager.Aim.normalized;

                var aimMagnitude = _inputManager.Aim.magnitude;

                aimMagnitude = aimMagnitude > 0.3f ? aimMagnitude : 0.3f;

                aimDirection *= 5f;

                _crosshair.parent.position = transform.parent.parent.position + (aimDirection * aimMagnitude);
            }
        }
        else
        {
            var mousePosition = Input.mousePosition;

            if (_cameraController == null || Camera.main == null) return;

            mousePosition.z = _cameraController.CurrentZ * -1f;

            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            mouseWorldPosition.z = 0f;

            _crosshair.parent.position = mouseWorldPosition;
        }
    }
}
