using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCorruptAimState : PlayerBaseState
{
    public PlayerCorruptAimState(PlayerStateManager currentContext, PlayerStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {

    }

    public override void UpdateState()
    {
        Ctx.MoveCharacter();
        Ctx.TargetNPC(Ctx.corruptRange, Ctx.corruptAim);
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
        if (!Ctx.corruptAim)
        {
            if (Ctx.corruptAimHit) SwitchState(Factory.QTEvent());
            else SwitchState(Factory.Walk());
        }
    }

    public override string ReturnStateName()
    {
        string value = "CorruptAim";
        return value;
    }
}
