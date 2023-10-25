using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static NPCAIStateManager;

public class NPCGetCorruptedState : NPCBaseState
{
    public NPCGetCorruptedState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        
    }

    public override void UpdateState()
    {
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

    public override void OnTriggerExit(Collider col)
    {

    }

    public override void ExitState()
    {

    }

    public override void CheckSwitchState()
    {
        if(Ctx.type == NPCType.security)
        {
            if (Ctx.partnerPossessed) SwitchState(Factory.SecurityPatrol());
        }
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.GetCorrupted;
    }
}
