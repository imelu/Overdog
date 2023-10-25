using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static NPCAIStateManager;

public class NPCTutorialState : NPCBaseState
{
    public NPCTutorialState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        switch (Ctx.type)
        {
            case NPCType.boss:

                break;

            case NPCType.normal:
                /*Ctx.currentRoom = GlobalGameManager.Instance.npcManager.tutorialCafeteria;
                Ctx.agent.speed = Random.Range(Ctx.speedMin, Ctx.speedMax);
                Ctx.agent.SetDestination(GlobalGameManager.Instance.npcManager.GetRandomLocationInRoom(Ctx.currentRoom));*/
                break;

            case NPCType.target:
                /*Ctx.currentRoom = GlobalGameManager.Instance.npcManager.tutorialCafeteria;
                Ctx.agent.speed = Random.Range(Ctx.speedMin, Ctx.speedMax);
                Ctx.agent.SetDestination(GlobalGameManager.Instance.npcManager.GetRandomLocationInRoom(Ctx.currentRoom));*/
                break;

            case NPCType.janitor:
                Ctx.agent.SetDestination(Ctx.targetPosition.position);
                break;

            case NPCType.security:
                Ctx.agent.SetDestination(Ctx.targetPosition.position);
                break;

            case NPCType.guard:

                break;

            case NPCType.entertainer:

                break;
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
        if(Ctx.type == NPCType.janitor)
        {
            if (col.CompareTag("JanitorDoorFront"))
            {
                col.GetComponentInParent<JanitorDoor>().OpenFromFront(Ctx);
            }

            if (col.CompareTag("JanitorDoorBack"))
            {
                col.GetComponentInParent<JanitorDoor>().OpenFromBack(Ctx);
            }
        }
    }

    public override void OnTriggerExit(Collider col)
    {

    }
    public override void ExitState()
    {
    }

    public override void CheckSwitchState()
    {
        switch (Ctx.type)
        {
            case NPCType.boss:

                break;

            case NPCType.normal:
                if ((Ctx.agent.remainingDistance <= Ctx.stopProximity) && !Ctx.agent.pathPending && Ctx.agent.hasPath)
                {
                    if (Ctx.transform.parent.childCount <= 1) GameObject.Destroy(Ctx.transform.parent.gameObject);
                    else
                    {
                        GameObject.Destroy(Ctx.gameObject);
                    }
                }
                break;

            case NPCType.target:
                /*if ((Ctx.agent.remainingDistance <= Ctx.stopProximity) && !Ctx.agent.pathPending)
                {
                    Ctx.agent.ResetPath();
                    SwitchState(Factory.Tutorial());
                }*/
                break;

            case NPCType.janitor:
                if ((Ctx.agent.remainingDistance <= Ctx.stopProximity) && !Ctx.agent.pathPending)
                {
                    GameObject.Destroy(Ctx.gameObject);
                }
                break;

            case NPCType.security:

                break;

            case NPCType.guard:
                if ((Ctx.agent.remainingDistance <= Ctx.stopProximity) && !Ctx.agent.pathPending)
                {
                    Ctx.RotateTowardsAngle(180);
                    if (Ctx.guardBlocking) Ctx.anim.SetTrigger("shakeHead");
                }
                break;

            case NPCType.entertainer:

                break;
        }
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.Tutorial;
    }
}
