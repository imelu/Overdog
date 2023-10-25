using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static NPCAIStateManager;

public class NPCListenToEntertainerState : NPCBaseState
{
    public NPCListenToEntertainerState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        
    }

    public override void UpdateState()
    {
        
    }

    public override void FixedUpdateState()
    {

    }

    public override void OnCollisionEnter(Collision col)
    {

    }

    public override void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("EntertainerCircle"))
        {
            Ctx.agent.ResetPath();
            SwitchState(Factory.Idle());
        }
    }

    public override void OnTriggerExit(Collider col)
    {

    }

    public override void ExitState()
    {

    }

    public override void CheckSwitchState()
    {
        if ((Ctx.agent.remainingDistance <= Ctx.entertainerStopProximity) && !Ctx.agent.pathPending)
        {
            Ctx.agent.ResetPath();
            SwitchState(Factory.Idle());
        }
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.ListenToEntertainer;
    }
}
