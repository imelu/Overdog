using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPossessAimState : PlayerBaseState
{
    public PlayerPossessAimState(PlayerStateManager currentContext, PlayerStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {

    }

    public override void UpdateState()
    {
        Ctx.MoveCharacter();
        Ctx.TargetNPC(Ctx.possessRange, Ctx.possessAim);
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
        if (!Ctx.possessAim)
        {
            if (Ctx.possessAimHit) SwitchState(Factory.QTEvent());
            else SwitchState(Factory.Idle());
        }
    }

    public override string ReturnStateName()
    {
        string value = "PossessAim";
        return value;
    }
}
