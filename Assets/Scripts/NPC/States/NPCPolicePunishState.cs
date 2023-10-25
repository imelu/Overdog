using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCPolicePunishState : NPCBaseState
{
    public NPCPolicePunishState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        Ctx.agent.speed = Ctx.sprintSpeed;
        Ctx.patrolPartnerScript.agent.speed = Ctx.sprintSpeed + Ctx.securitySpeedInc;
        Ctx.agent.SetDestination(Ctx.playerToPunish.transform.position);
        Ctx.patrolPartnerScript.currentPatrolGoal = (Ctx.playerToPunish.transform.position);
    }

    public override void UpdateState()
    {
        Ctx.agent.SetDestination(Ctx.playerToPunish.transform.position);
        Ctx.patrolPartnerScript.currentPatrolGoal = (Ctx.playerToPunish.transform.position);
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
        Ctx.agent.speed = Ctx.speed;
        Ctx.patrolPartnerScript.agent.speed = Ctx.speed + Ctx.securitySpeedInc;
    }

    public override void CheckSwitchState()
    {
        if ((Ctx.agent.remainingDistance <= Ctx.policeStopProximity) && !Ctx.agent.pathPending)
        {
            Ctx.passedGate = false;
            Ctx.punishPlayer = false;
            Ctx.agent.ResetPath();
            Ctx.PunishPlayer(Ctx.playerToPunish);
            //SwitchState(Factory.SecurityPatrol());
            Ctx.anim.SetTrigger("punish");
            Ctx.GoalReached();
            SwitchState(Factory.Idle());
        }
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.PolicePunish;
    }
}
