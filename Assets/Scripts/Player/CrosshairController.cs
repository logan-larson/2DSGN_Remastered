using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using UnityEngine.InputSystem;

public class CrosshairController : NetworkBehaviour
{
    [SerializeField]
    private RectTransform _crosshair;

    [SerializeField]
    private PlayerManager _playerManager;

    [SerializeField]
    private WeaponManager _weaponManager;

    private CameraController _cameraController;

    [SerializeField]
    private InputManager _inputManager;

    private bool _subscribedToTimeManager = false;

    [SerializeField]
    private float _minSize = 5f;

    [SerializeField]
    private float _sizeMultiplier = 15f;

    [SerializeField]
    private GameObject _weaponHolder;

    private void Start()
    {
        _crosshair = GetComponent<RectTransform>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner)
        {
            gameObject.SetActive(false);
            return;
        }


        // Camera setup hullabaloo

        if (Camera.main != null)
        {
            _cameraController = Camera.main.GetComponent<CameraController>();
        }

        if (_cameraController == null)
        {
            if (_playerManager.Camera == null)
            {
                StartCoroutine(WaitForCamera());
            }
            else
            {
                _cameraController = _playerManager.Camera.GetComponent<CameraController>();
            }
        }

        SubscribeToTimeManager(true);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        SubscribeToTimeManager(false);
    }

    private IEnumerator WaitForCamera()
    {
        if (_playerManager.Camera == null)
        {
            Debug.Log("Waiting for camera");
            yield return new WaitForSeconds(0.1f);
        }

        _cameraController = _playerManager.Camera.GetComponent<CameraController>();
    }

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

    private void OnTick()
    {
        if (_weaponManager.CurrentWeaponInfo == null) return;

        var size = Mathf.Max(_minSize * _weaponManager.CurrentWeaponInfo.MaxBloomAngle, _weaponManager.Bloom * _sizeMultiplier);

        _crosshair.sizeDelta = new Vector2(size, size);

        if (_cameraController == null)
        {
            Debug.Log("Couldn't find camera controller or main camera, num cameras: " + Camera.allCamerasCount);
            return;
        }

        if (_inputManager.InputDevice == "Gamepad")
        {
            if (_inputManager.Aim != Vector2.zero)
            {
                //var aimDirection = _cameraController.transform.rotation * _inputManager.Aim.normalized;
                var aimDirection = Camera.main.transform.rotation * _inputManager.Aim.normalized;

                var maxMagnitude = _inputManager.CameraLockInput ? 10f : 5f;

                var aimMagnitude = Mathf.Clamp(_inputManager.Aim.magnitude * maxMagnitude, 0.25f, maxMagnitude);

                // The offset is the position of the crosshair relative to the weaponholder
                _crosshair.parent.position = _weaponHolder.transform.position + (aimDirection * aimMagnitude);
            }
        }
        else
        {
            var mousePosition = Input.mousePosition;

            mousePosition.z = _cameraController.CurrentZ * -1f;

            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            mouseWorldPosition.z = 0f;

            _crosshair.parent.position = mouseWorldPosition;

        }

        _cameraController.CrosshairPosition = _crosshair.parent.position;
    }
}
