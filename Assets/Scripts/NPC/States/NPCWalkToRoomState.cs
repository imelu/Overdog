using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCWalkToRoomState : NPCBaseState
{
    public NPCWalkToRoomState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        Ctx.agent.speed = Random.Range(Ctx.speedMin, Ctx.speedMax);
        Ctx.targetRoom = GlobalGameManager.Instance.npcManager.WeightedRandomRoom();
        Ctx.agent.SetDestination(GlobalGameManager.Instance.npcManager.GetRandomLocationInRoom(Ctx.targetRoom));
        if(Ctx.agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            if (Ctx.currentRoom.isInGate)
            {
                SwitchState(Factory.Walk());
            }
            else
            {
                if(Ctx.failSafeIngate < 7)
                {
                    Ctx.failSafeIngate++;
                    SwitchState(Factory.WalkToRoom());
                }
                else
                {
                    SwitchState(Factory.Walk());
                    Ctx.failSafeIngate = 0;
                }
            }
        }
        else
        {
            Ctx.failSafeIngate = 0;
        }
        //Debug.Log("WalkToRoom");
    }

    public override void UpdateState()
    {
        CheckSwitchState();
        if (Ctx.agent.isPathStale && !Ctx.passedGate)
        {
            SwitchState(Factory.Walk());
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
            //Ctx.currentRoom = Ctx.targetRoom;
            Ctx.agent.ResetPath();
            Ctx.passedGate = false;
            //if (Ctx.type == NPCAIStateManager.NPCType.police) SwitchState(Factory.PoliceWatch());
            //else SwitchState(Factory.Idle());
            SwitchState(Factory.Idle());
        }
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.WalkToRoom;
    }
}
