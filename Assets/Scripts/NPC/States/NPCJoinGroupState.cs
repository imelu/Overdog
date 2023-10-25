using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCJoinGroupState : NPCBaseState
{
    public NPCJoinGroupState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        foreach(GroupManager group in Ctx.currentRoom.groups)
        {
            if(group.groupCapacity > group.npcs.Count)
            {
                Ctx.agent.speed = Random.Range(Ctx.speedMin, Ctx.speedMax);
                group.JoinGroup(Ctx);
                Ctx.agent.SetDestination(group.gameObject.transform.position);
                Ctx.groupManager = group;
                //Debug.Log("JoinGroup");
                return;
            }
        }
        SwitchState(Factory.FormGroup());
    }

    public override void UpdateState()
    {
        CheckSwitchState();
        //if (Ctx.agent.isPathStale) Ctx.currentState.EnterState();
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
            Ctx.RotateTowardsGroupCenter();
            SwitchState(Factory.GroupIdle());
        }
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.JoinGroup;
    }
}
