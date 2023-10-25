using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static NPCAIStateManager;

public class NPCWalkState : NPCBaseState
{
    public NPCWalkState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        Ctx.agent.speed = Random.Range(Ctx.speedMin, Ctx.speedMax);
        Ctx.agent.SetDestination(GlobalGameManager.Instance.npcManager.GetRandomLocationInRoom(Ctx.currentRoom));
        //Debug.Log("Walk");
    }

    public override void UpdateState()
    {
        CheckSwitchState();
        if (Ctx.agent.isPathStale) Ctx.currentState.EnterState();
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
        if ((Ctx.agent.remainingDistance <= Ctx.stopProximity) && !Ctx.agent.pathPending)
        {
            Ctx.agent.ResetPath();
            if(Ctx.type == NPCType.police) SwitchState(Factory.PoliceWatch());
            else SwitchState(Factory.Idle());
        }
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.Walk;
    }
}
