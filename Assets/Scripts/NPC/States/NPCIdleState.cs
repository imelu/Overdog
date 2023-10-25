using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NPCIdleState : NPCBaseState
{
    public NPCIdleState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        //if(Ctx.type == NPCAIStateManager.NPCType.security) Debug.Log("Idle");
        if (Ctx.inTutorial) return;
        if (Ctx.type != NPCAIStateManager.NPCType.boss && Ctx.type != NPCAIStateManager.NPCType.entertainer && Ctx.type != NPCAIStateManager.NPCType.security && Ctx.type != NPCAIStateManager.NPCType.guard)
        {
            Ctx.StartActionCooldown();
            Ctx.JanitorAreaLeft();
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
    }

    public override void CheckSwitchState()
    {
        if (Ctx.inTutorial) return;
        if (Ctx.selectedAction == NPCAIStateManager.NPCAction.walk)
        {
            Ctx.selectedAction = NPCAIStateManager.NPCAction.NONE;
            switch (Ctx.type)
            {
                case NPCAIStateManager.NPCType.normal:
                    SwitchState(Factory.Walk());
                    break;

                case NPCAIStateManager.NPCType.janitor:
                    SwitchState(Factory.JanitorWalk());
                    break;

                case NPCAIStateManager.NPCType.police:
                    SwitchState(Factory.Walk());
                    break;

                case NPCAIStateManager.NPCType.target:
                    SwitchState(Factory.Walk());
                    break;
            }
        }
        if (Ctx.selectedAction == NPCAIStateManager.NPCAction.walkToRoom)
        {
            Ctx.selectedAction = NPCAIStateManager.NPCAction.NONE;
            switch (Ctx.type)
            {
                case NPCAIStateManager.NPCType.normal:
                    SwitchState(Factory.WalkToRoom());
                    break;

                case NPCAIStateManager.NPCType.janitor:
                    SwitchState(Factory.JanitorWalk());
                    break;

                case NPCAIStateManager.NPCType.police:
                    SwitchState(Factory.WalkToRoom());
                    break;
            }
        }
        if (Ctx.selectedAction == NPCAIStateManager.NPCAction.joinGroup)
        {
            Ctx.selectedAction = NPCAIStateManager.NPCAction.NONE;
            SwitchState(Factory.JoinGroup());
        }
        if (Ctx.selectedAction == NPCAIStateManager.NPCAction.formMob)
        {
            Ctx.selectedAction = NPCAIStateManager.NPCAction.NONE;
            // TODO basically join group, but lead it
        }
        if (Ctx.selectedAction == NPCAIStateManager.NPCAction.idle)
        {
            Ctx.selectedAction = NPCAIStateManager.NPCAction.NONE;
            EnterState();
        }
        if (Ctx.type == NPCAIStateManager.NPCType.security)
        {
            if (Ctx.punishPlayer && Ctx.isLeadingPatrol)
            {
                Ctx.passedGate = false;
                SwitchState(Factory.PolicePunish());
                Ctx.anim.SetTrigger("move");
            }
            if (Ctx.moveToPlayer) // && Ctx.isLeadingPatrol)
            {
                Ctx.passedGate = false;
                SwitchState(Factory.PoliceCorrupt());
            }
            if (Ctx.startPatrol)
            {
                Ctx.startPatrol = false;
                SwitchState(Factory.SecurityPatrol());
            }
        }
        if (Ctx.type == NPCAIStateManager.NPCType.guard)
        {
            SwitchState(Factory.Guard());
        }
        if (Ctx.playingMusic)
        {
            SwitchState(Factory.EntertainerPlaying());
        }
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.Idle;
    }
}
