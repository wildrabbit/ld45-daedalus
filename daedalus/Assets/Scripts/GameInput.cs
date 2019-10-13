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
    const int kActionMoveIdx = 0;
    const int kActionRotateIdx = 1;
    const int kActionConfirmIdx = 2;
    const int kActionCancelIdx = 3;

    const float MoveThreshold = 0.5f;
    const float RotateThreshold = 0.5f;

    [HideInInspector] public MoveDirection Direction = MoveDirection.None;
    [HideInInspector] public RotateDirection Rotation = RotateDirection.None;
    [HideInInspector] public bool Confirmed = false;
    [HideInInspector] public bool Cancelled = false;

    [HideInInspector] public bool AnyPressed;

    InputAction _anyPressAction;
    InputActionMap _actionMap;
 
    [SerializeField] PlayerInput _playerInput;

    private void Awake()
    {
        _anyPressAction = new InputAction(binding: "/*/<button>");
        _anyPressAction.performed += OnAnyAction;
        //_anyPressAction.Enable();


        _actionMap = _playerInput.currentActionMap;
        var lesActions = _actionMap.actions;
        //lesActions[kActionMoveIdx].performed += OnInputMove;
        lesActions[kActionRotateIdx].performed += OnInputRotate;
        lesActions[kActionConfirmIdx].performed += OnInputPlaceConfirm;
        lesActions[kActionCancelIdx].performed += OnInputPlaceCancel;
    }

    void OnAnyAction(InputAction.CallbackContext context)
    {
        AnyPressed = true;
    }

    private void Update()
    {
        if(_actionMap == null)
        {
            return;
        }

        ReadDirection();        
    }

    void ReadDirection()
    {
        Vector2 value = _actionMap.actions[kActionMoveIdx].ReadValue<Vector2>();
        Direction = MoveDirection.None;
        if (value.y > MoveThreshold)
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

    private void OnDestroy()
    {
        _anyPressAction.Disable();
        _anyPressAction.performed -= OnAnyAction;

        if(_actionMap != null)
        {
            var lesActions = _actionMap.actions;
            //lesActions[kActionMoveIdx].performed -= OnInputMove;
            lesActions[kActionRotateIdx].performed -= OnInputRotate;
            lesActions[kActionConfirmIdx].performed -= OnInputPlaceConfirm;
            lesActions[kActionCancelIdx].performed -= OnInputPlaceCancel;
        }        
    }

    public void OnInputMove(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
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
        var test = _actionMap.actions[kActionRotateIdx].ReadValue<float>();
        float value = context.ReadValue<float>();
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
        Confirmed = true;
    }

    public void OnInputPlaceCancel(InputAction.CallbackContext context)
    {
        Cancelled = true;
    }

    public void Reset()
    {
        Direction = MoveDirection.None;
        Rotation = RotateDirection.None;
        Confirmed = false;
        Cancelled = false;
        AnyPressed = false;
    }

    public override string ToString()
    {
        return $"Current State: \nMove: {Direction}, \nRotation: {Rotation}, \nConfirm?: {Confirmed}, \nCancelled? {Cancelled}, \nAny? {AnyPressed}";
    }
}