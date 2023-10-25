using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class NPCBaseState
{
    private NPCAIStateManager _ctx;
    private NPCStateFactory _factory;
    protected NPCBaseState _currentState;

    protected NPCAIStateManager Ctx { get { return _ctx; } }
    protected NPCStateFactory Factory { get { return _factory; } }

    public enum NPCStates
    {
        Idle,
        Walk,
        WalkToRoom,
        FollowGroup,
        GroupIdle,
        FormGroup,
        JoinGroup,
        GetCorrupted,
        JanitorWalk,
        PoliceWatch,
        PolicePunish,
        PoliceCorrupt,
        SecurityPatrol,
        AvoidPlayer,
        Guard,
        EntertainerPlaying,
        ListenToEntertainer,
        Released, 
        GoToBoss,
        Tutorial,
        FollowPlayer
    }

    public NPCBaseState(NPCAIStateManager currentContext, NPCStateFactory factory)
    {
        _ctx = currentContext;
        _factory = factory;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void FixedUpdateState();
    public abstract void OnCollisionEnter(Collision col);
    public abstract void OnTriggerEnter(Collider col);
    public abstract void OnTriggerExit(Collider col);
    public abstract void ExitState();
    public abstract void CheckSwitchState();
    public abstract NPCStates ReturnStateName();

    public void UpdateStates()
    {
        UpdateState();
    }

    public void FixedUpdateStates()
    {
        FixedUpdateState();
    }

    protected void SwitchState(NPCBaseState newState)
    {
        ExitState();
        Ctx.currentState = newState;
        newState.EnterState();
    }
}
