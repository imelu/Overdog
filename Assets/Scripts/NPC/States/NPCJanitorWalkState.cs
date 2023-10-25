using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCJanitorWalkState : NPCBaseState
{
    public NPCJanitorWalkState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        Ctx.targetRoom = GlobalGameManager.Instance.npcManager.WeightedRandomRoom();
        Ctx.agent.SetDestination(GlobalGameManager.Instance.npcManager.GetRandomLocationInRoom(Ctx.targetRoom));
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
        if (col.CompareTag("JanitorDoorFront"))
        {
            col.GetComponentInParent<JanitorDoor>().OpenFromFront(Ctx);
        }

        if (col.CompareTag("JanitorDoorBack"))
        {
            col.GetComponentInParent<JanitorDoor>().OpenFromBack(Ctx);
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
        if ((Ctx.agent.remainingDistance <= Ctx.stopProximity) && !Ctx.agent.pathPending)
        {
            //Ctx.currentRoom = Ctx.targetRoom;
            Ctx.passedGate = false;
            Ctx.agent.ResetPath();
            SwitchState(Factory.Idle());
        }
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.JanitorWalk;
    }
}
