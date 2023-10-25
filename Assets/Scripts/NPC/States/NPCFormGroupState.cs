using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCFormGroupState : NPCBaseState
{
    public NPCFormGroupState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        Ctx.agent.speed = Random.Range(Ctx.speedMin, Ctx.speedMax);
        Vector3 groupPos = GlobalGameManager.Instance.npcManager.GetRandomLocationInRoom(Ctx.currentRoom);
        GroupManager gMan = Ctx.FormGroupAt(groupPos);
        gMan.currentRoom = Ctx.currentRoom;
        gMan.JoinGroup(Ctx);
        Ctx.agent.SetDestination(groupPos);
        Ctx.groupManager = gMan;
        //Debug.Log("FormGroup");
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
        if (Ctx.agent.remainingDistance <= 0.2f)
        {
            SwitchState(Factory.GroupIdle());

        }
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.FormGroup;
    }
}
