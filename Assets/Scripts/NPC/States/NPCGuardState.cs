using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class NPCGuardState : NPCBaseState
{
    public NPCGuardState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        if (Ctx.agent.isActiveAndEnabled) Ctx.agent.SetDestination(Ctx.idlePos.position);
        Ctx.JanitorAreaLeft();
    }

    public override void UpdateState()
    {
        CheckSwitchState();
        if (!(Ctx.agent.hasPath || Ctx.agent.pathPending) && Ctx.agent.isActiveAndEnabled)
        {
            if (Vector3.Distance(Ctx.transform.position, Ctx.idlePos.position) > 1 && Vector3.Distance(Ctx.transform.position, Ctx.blockingPos.position) > 1)
            {
                Ctx.agent.SetDestination(Ctx.idlePos.position);
            }
            else
            {
                if (!Ctx.inTutorial)
                {
                    Ctx.agent.enabled = false;
                    Ctx.obstacle.enabled = true;
                }
                Ctx.RotateTowardsAngle(180);
                if (Ctx.guardBlocking) Ctx.anim.SetTrigger("shakeHead");
            }
        }
        if (!(Ctx.agent.hasPath || Ctx.agent.pathPending) && !Ctx.obstacle.isActiveAndEnabled)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(Ctx.transform.position, out hit, 0.1f, NavMesh.AllAreas))
            {
                Ctx.agent.enabled = true;
                Ctx.agent.SetDestination(Ctx.nextGuardPos);
            }
        }
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
        
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.Guard;
    }
}
