using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    public PlayerWalkState(PlayerStateManager currentContext, PlayerStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        //Debug.Log("player walk");
    }

    public override void UpdateState()
    {
        Ctx.MoveCharacter();
        CheckSwitchState();
    }

    public override void FixedUpdateState()
    {

    }

    public override void OnCollisionEnter(Collision col)
    {

    }

    public override void OnTriggerEnter(Collider col)
    {
        
    }

    public override void ExitState()
    {

    }

    public override void CheckSwitchState()
    {
        if (Ctx.possessAim) SwitchState(Factory.PossessAim());
        if (Ctx.corruptAim) SwitchState(Factory.CorruptAim());
        if (Ctx.attackAim) SwitchState(Factory.AttackAim());
        if (!Ctx.movePlayer) SwitchState(Factory.Idle());
    }

    public override string ReturnStateName()
    {
        string value = "Walk";
        return value;
    }
}
