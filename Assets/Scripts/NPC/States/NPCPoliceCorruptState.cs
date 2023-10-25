using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCPoliceCorruptState : NPCBaseState
{
    public NPCPoliceCorruptState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        //Vector3 dest = Ctx.SetNextPatrolPoint(Ctx.playerToFollow.currentRoom);
        //Ctx.agent.SetDestination(dest);
        //Ctx.patrolPartnerScript.agent.SetDestination(dest);
        //Ctx.currentPatrolGoal = GlobalGameManager.Instance.npcManager.GetRandomLocationInRoom(Ctx.playerToFollow.currentRoom);
        //Ctx.patrolPartnerScript.currentPatrolGoal = Ctx.currentPatrolGoal;
        Ctx.CorruptSecuityMove();
    }

    public override void UpdateState()
    {
        CheckSwitchState();
        if (Ctx.agent.velocity.magnitude <= 0 && !Ctx.agent.pathPending) Ctx.agent.SetDestination(Ctx.currentPatrolGoal);
        if (Ctx.isLeadingPatrol && !Ctx.partnerPossessed && !Ctx.patrolPartnerScript.partnerPossessed) Ctx.SyncUp();
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
        //int i = Ctx.currentPatrolPoint - 1;
        //if (Ctx.currentPatrolPoint <= 0) i = Ctx.patrolRooms.Count - 1;
        if ((Vector3.Distance(Ctx.transform.position, Ctx.agent.destination) <= Ctx.stopProximity) && !Ctx.agent.pathPending && Ctx.agent.hasPath) //&& Ctx.currentRoom == Ctx.playerTargetRoom)
        {
            /*Ctx.patrolPartnerScript.moveToPlayer = false;
            Ctx.patrolPartnerScript.agent.ResetPath();
            Ctx.patrolPartnerScript.GoalReached();
            Ctx.patrolPartnerScript.currentState = Ctx.states.Idle();
            Ctx.patrolPartnerScript.anim.SetTrigger("look");*/


            Ctx.moveToPlayer = false;
            Ctx.agent.ResetPath();
            SwitchState(Factory.Idle());
            Ctx.GoalReached();
            if (Ctx.isLeadingPatrol) Ctx.anim.SetTrigger("look");
        }
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.PoliceCorrupt;
    }
}
