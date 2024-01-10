using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public float HorizontalMoveInput { get; private set; }

    // Mode Toggles
    public bool SprintInput { get; private set; }
    public bool SlideInput { get; private set; }
    public bool ShootInput { get; private set; }

    public bool JumpInput { get; private set; }

    public Vector2 Aim { get; private set; }

    public bool CameraLockInput { get; private set; }

    void Start()
    {
        
    }

    void Update()
    {
    
    }

    public void OnMove(InputValue value)
    {
        HorizontalMoveInput = value.Get<Vector2>().x;
    }

    public void OnSprint(InputValue value)
    {
        SprintInput = value.isPressed;
    }

    public void OnJump(InputValue value)
    {
        JumpInput = value.isPressed;
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
}
