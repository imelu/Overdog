using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCGoToBossState : NPCBaseState
{
    public NPCGoToBossState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        Ctx.agent.speed = Random.Range(Ctx.speedMin, Ctx.speedMax);
        Room room = GlobalGameManager.Instance.npcManager.rooms[GlobalGameManager.Instance.npcManager.rooms.Count - 1];

        NavMeshHit hit; // NavMesh Sampling Info Container
        NavMesh.SamplePosition(Random.insideUnitSphere * 5, out hit, room.GetRadius(), 1);

        if(Ctx.agent.isActiveAndEnabled) Ctx.agent.SetDestination(hit.position);
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
        
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.GoToBoss;
    }
}
