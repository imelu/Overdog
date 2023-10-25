using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCAvoidPlayerState : NPCBaseState
{
    public NPCAvoidPlayerState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        Ctx.agent.speed = Ctx.sprintSpeed;
        if(Ctx.PlayersToAvoid.Count > 0)
        {
            NavMeshHit hit; // NavMesh Sampling Info Container
            bool foundPosition = NavMesh.SamplePosition(Ctx.transform.position + ((Ctx.PlayersToAvoid[0].transform.position - Ctx.transform.position).normalized) * -5, out hit, Mathf.Infinity, 1);
            Ctx.agent.SetDestination(hit.position);
        }
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
        Ctx.agent.speed = Ctx.speed;
    }

    public override void CheckSwitchState()
    {
        if ((Ctx.agent.remainingDistance <= Ctx.stopProximity) && !Ctx.agent.pathPending)
        {
            Ctx.passedGate = false;
            bool punishOver = true;
            foreach (PlayerStateManager player in Ctx.PlayersToAvoid) if (player.gotPunished) punishOver = false;
            if (Ctx.PlayersToAvoid.Count > 0 && !punishOver)
            {
                NavMeshHit hit; // NavMesh Sampling Info Container
                bool foundPosition = NavMesh.SamplePosition(Ctx.transform.position + ((Ctx.PlayersToAvoid[0].transform.position - Ctx.transform.position).normalized) * -5, out hit, Mathf.Infinity, 1);
                Ctx.agent.SetDestination(hit.position);
            }
            else
            {
                Ctx.PlayersToAvoid = new List<PlayerStateManager>();
                Ctx.agent.ResetPath();
                SwitchState(Ctx.avoidPlayerBackupState);
            }
        }
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.AvoidPlayer;
    }
}
