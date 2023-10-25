using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static NPCAIStateManager;

public class NPCFollowPlayerState : NPCBaseState
{
    public NPCFollowPlayerState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        Ctx.oldWinnerPos = Ctx.winnerToFollow.transform.position;
        Ctx.agent.SetDestination(Ctx.GetFollowPlayerPosition());
    }

    public override void UpdateState()
    {
        CheckSwitchState();
        if(Ctx.winnerToFollow != null)
        {
            if (Ctx.oldWinnerPos != Ctx.winnerToFollow.transform.position)
            {
                Ctx.agent.SetDestination(Ctx.GetFollowPlayerPosition());
                Ctx.oldWinnerPos = Ctx.winnerToFollow.transform.position;
            }
            if (Ctx.winnerToFollow.sprint) Ctx.agent.speed = Ctx.sprintSpeed;
            else Ctx.agent.speed = Ctx.speed;
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
        if ((Ctx.agent.remainingDistance <= Ctx.stopProximity) && !Ctx.agent.pathPending)
        {
            Ctx.agent.ResetPath();
            if(Ctx.winnerToFollow == null)
            {
                SwitchState(Factory.Idle());
                if (Ctx.type == NPCType.security) Ctx.startPatrol = true;
            }
            else if (Ctx.winnerToFollow.currentInfluence < 100)
            {
                Ctx.winnerToFollow = null;
                Ctx.agent.SetDestination(Ctx.posBeforeFollow);
            }
            else if (Ctx.oldWinnerPos != Ctx.winnerToFollow.transform.position)
            {
                Ctx.agent.SetDestination(Ctx.GetFollowPlayerPosition());
                Ctx.oldWinnerPos = Ctx.winnerToFollow.transform.position;
            }
        }
        /*
        if (!Ctx.isCorruptedP1 && Ctx.winnerToFollow.isPlayerOne)
        {
            SwitchState(Factory.Idle());
        }
        if (!Ctx.isCorruptedP2 && !Ctx.winnerToFollow.isPlayerOne)
        {
            SwitchState(Factory.Idle());
        }*/
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.FollowPlayer;
    }
}
