using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class PlayerBaseState
{
    private PlayerStateManager _ctx;
    private PlayerStateFactory _factory;
    protected PlayerBaseState _currentState;

    protected PlayerStateManager Ctx { get { return _ctx; } }
    protected PlayerStateFactory Factory { get { return _factory; } }

    public PlayerBaseState(PlayerStateManager currentContext, PlayerStateFactory factory)
    {
        _ctx = currentContext;
        _factory = factory;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void FixedUpdateState();
    public abstract void OnCollisionEnter(Collision col);
    public abstract void OnTriggerEnter(Collider col);
    public abstract void ExitState();
    public abstract void CheckSwitchState();
    public abstract string ReturnStateName();

    public void UpdateStates()
    {
        UpdateState();
    }

    public void FixedUpdateStates()
    {
        FixedUpdateState();
    }

    protected void SwitchState(PlayerBaseState newState)
    {
        ExitState();
        newState.EnterState();
        Ctx.currentState = newState;
    }
}
