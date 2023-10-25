using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCFollowGroupState : NPCBaseState
{
    public NPCFollowGroupState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        //Ctx.agent.SetDestination(Ctx.groupManager.gameObject.transform.position);
        Ctx.agent.speed = Ctx.speed;
        Ctx.agent.SetPath(Ctx.nextPath);
        if(Ctx.nextActionCoroutine != null) Ctx.StopCoroutine(Ctx.nextActionCoroutine);
    }

    public override void UpdateState()
    {
        CheckSwitchState();
        if (Ctx.agent.isPathStale && !Ctx.passedGate) Ctx.currentState.EnterState();
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
        if (Ctx.agent.remainingDistance <= Ctx.stopProximity)
        {
            Ctx.passedGate = false;
            //Ctx.currentRoom = Ctx.targetRoom;
            Ctx.agent.ResetPath();
            Ctx.RotateTowardsGroupCenter();
            SwitchState(Factory.GroupIdle());
        }
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.FollowGroup;
    }
}