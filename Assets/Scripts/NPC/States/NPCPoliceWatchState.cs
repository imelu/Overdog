using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCPoliceWatchState : NPCBaseState
{
    public NPCPoliceWatchState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        //Debug.Log("Police Watch State");   
        Ctx.StartActionCooldown();
        Ctx.JanitorAreaLeft();
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
        if (Ctx.punishPlayer)
        {
            SwitchState(Factory.PolicePunish());
        }
        if (Ctx.moveToPlayer)
        {
            SwitchState(Factory.PoliceCorrupt());
        }
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.PoliceWatch;
    }
}
