using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : NetworkBehaviour
{
    public float HorizontalMoveInput { get; private set; }

    // Mode Toggles
    public bool SprintInput { get; private set; }
    public bool SlideInput { get; private set; }
    public bool ShootInput { get; private set; }

    public bool JumpInput { get; private set; }
    public bool InteractInput { get; private set; }
    public bool FireInput { get; private set; }

    public Vector2 Aim { get; private set; }

    public bool CameraLockInput { get; private set; }

    [SyncVar]
    public string InputDevice = "Keyboard&Mouse";

    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
    }

    public void OnMove(InputValue value)
    {
        HorizontalMoveInput = value.Get<Vector2>().x;
    }

    public void OnAim(InputValue value)
    {
        Aim = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        SprintInput = value.isPressed;
    }

    public void OnJump(InputValue value)
    {
        JumpInput = value.isPressed;
    }

    public void OnFire(InputValue value)
    {
        FireInput = value.isPressed;
    }

    public void OnInteract(InputValue value)
    {
        InteractInput = value.isPressed;
    }

    public void OnSlide(InputValue value)
    {
        SlideInput = value.isPressed;
    }

    public void OnShoot(InputValue value)
    {
        ShootInput = value.isPressed;
    }

    public void OnCameraLock(InputValue value)
    {
        CameraLockInput = value.isPressed;
    }

    public void OnControlsChanged()
    {
        if (!base.IsClientInitialized || _playerInput == null)
            return;

        OnControlsChangedServerRpc(_playerInput.currentControlScheme);
        //InputDevice = _playerInput.currentControlScheme;
    }

    [ServerRpc]
    private void OnControlsChangedServerRpc(string inputDevice)
    {
        InputDevice = inputDevice;
    }
}
