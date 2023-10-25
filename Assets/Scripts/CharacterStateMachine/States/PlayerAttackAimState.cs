using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackAimState : PlayerBaseState
{
    public PlayerAttackAimState(PlayerStateManager currentContext, PlayerStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {

    }

    public override void UpdateState()
    {
        Ctx.MoveCharacter();
        //Ctx.TargetNPC(Ctx.attackRange, Ctx.attackAim);
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
        Ctx.ClearTargetedNPCs();
    }

    public override void CheckSwitchState()
    {
        if (!Ctx.attackAim) SwitchState(Factory.Walk());
    }

    public override string ReturnStateName()
    {
        string value = "AttackAim";
        return value;
    }
}
