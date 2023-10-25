using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour, AxisState.IInputAxisProvider
{
    [SerializeField] private PlayerStateManager playerStateManager;

    [HideInInspector]
    public InputAction horizontal;
    [HideInInspector]
    public InputAction vertical;

    public virtual float GetAxisValue(int axis)
    {
        if (playerStateManager.currentState.ReturnStateName().Equals("QTEvent")) return 0;
        switch (axis)
        {
            case 0: return horizontal.ReadValue<Vector2>().x;
            case 1: return horizontal.ReadValue<Vector2>().y;
            case 2: return vertical.ReadValue<float>();
        }
        return 0;
    }
}
