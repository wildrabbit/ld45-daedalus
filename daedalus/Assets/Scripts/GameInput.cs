using UnityEngine;
using UnityEngine.InputSystem;


public enum MoveDirection
{
    None = -1,
    N,
    S,
    E,
    W
}

public enum RotateDirection
{
    None = -1,
    Left,
    Right
}


public class GameInput : MonoBehaviour
{
    const float MoveThreshold = 0.5f;
    const float RotateThreshold = 0.5f;

    public MoveDirection Direction = MoveDirection.None;
    public RotateDirection Rotation = RotateDirection.None;
    public bool Confirmed = false;
    public bool Cancelled = false;

    [SerializeField] PlayerInput _playerInput;


    public void OnInputMove(InputAction.CallbackContext context)
    {
        if(context.phase != InputActionPhase.Started)
        {
            return;
        }

        Vector2 value = context.action.ReadValue<Vector2>();
        Direction = MoveDirection.None;
        if(value.y > MoveThreshold)
        {
            Direction = MoveDirection.N;
        }
        else if (value.y < -MoveThreshold)
        {
            Direction = MoveDirection.S;
        }
        else if (value.x > MoveThreshold)
        {
            Direction = MoveDirection.E;
        }
        else if (value.x < -MoveThreshold)
        {
            Direction = MoveDirection.W;
        }
    }
    public void OnInputRotate(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Started)
        {
            return;
        }

        float value = context.action.ReadValue<float>();
        Rotation = RotateDirection.None;
        if(value < -RotateThreshold)
        {
            Rotation = RotateDirection.Left;
        }
        else if (value > RotateThreshold)
        {
            Rotation = RotateDirection.Right;
        }
    }
    public void OnInputPlaceConfirm(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Started)
        {
            return;
        }

        Confirmed = context.action.ReadValue<float>() > 0.5f;
    }

    public void OnInputPlaceCancel(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Started)
        {
            return;
        }

        Cancelled = context.action.ReadValue<float>() > 0.5f;
    }

    public void Reset()
    {
        Direction = MoveDirection.None;
        Rotation = RotateDirection.None;
        Confirmed = false;
        Cancelled = false;
    }
}