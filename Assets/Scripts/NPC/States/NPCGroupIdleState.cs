using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCGroupIdleState : NPCBaseState
{
    public NPCGroupIdleState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        Ctx.StartActionCooldown();
        //Debug.Log("GroupIdle");
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
    }

    public override void CheckSwitchState()
    {
        if (Ctx.selectedAction == NPCAIStateManager.NPCAction.moveGroup) // start group movement, doshit is temp
        {
            Ctx.selectedAction = NPCAIStateManager.NPCAction.NONE;
            Ctx.targetRoom = GlobalGameManager.Instance.npcManager.WeightedRandomRoom();
            Ctx.groupManager.MoveGroup(GlobalGameManager.Instance.npcManager.GetRandomLocationInRoom(Ctx.targetRoom));
        }
        if (Ctx.followGroup)
        {
            Ctx.followGroup = false;
            SwitchState(Factory.FollowGroup());
        }
        if (Ctx.selectedAction == NPCAIStateManager.NPCAction.leaveGroup)
        {
            Ctx.selectedAction = NPCAIStateManager.NPCAction.NONE;
            Ctx.groupManager.LeaveGroup(Ctx);
            SwitchState(Factory.WalkToRoom());
        }
        if (Ctx.selectedAction == NPCAIStateManager.NPCAction.idle)
        {
            Ctx.selectedAction = NPCAIStateManager.NPCAction.NONE;
            EnterState();
        }
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.GroupIdle;
    }
}
