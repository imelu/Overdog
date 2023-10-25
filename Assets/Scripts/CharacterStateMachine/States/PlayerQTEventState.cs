using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerQTEventState : PlayerBaseState
{
    public PlayerQTEventState(PlayerStateManager currentContext, PlayerStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        //Debug.Log("player QTEvent");
        Ctx.possessCompleted = false;
        Ctx.corruptCompleted = false;
        Ctx.qtEvent = true;

        if (Ctx.corruptAimHit)
        {
            if (Ctx.npcToCorrupt.type == NPCAIStateManager.NPCType.boss)
            {
                Ctx.quickTime.StartCorruptQTEvent(Ctx.npcToCorrupt.corruptDifficulty);
            }
            else
            {
                int difficulty = Ctx.npcToCorrupt.corruptDifficulty;
                if (Ctx.npcToCorrupt.isCorruptedP1 || Ctx.npcToCorrupt.isCorruptedP2) difficulty += Ctx.npcToCorrupt.incCorruptDifficulty;
                Ctx.quickTime.StartCorruptQTEvent(difficulty);
            }
        }
        else if (Ctx.possessAimHit)
        {
            if (Ctx.npcToPossess.type == NPCAIStateManager.NPCType.boss)
            {
                Ctx.npcToCorrupt = Ctx.npcToPossess;
                Ctx.quickTime.StartCorruptQTEvent(Ctx.npcToPossess.corruptDifficulty);
            }
            else
            {
                Ctx.quickTime.StartPossessQTEvent();
            }
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

    public override void ExitState()
    {

    }

    public override void CheckSwitchState()
    {
        if (Ctx.corruptCompleted)
        {
            Ctx.corruptCompleted = false;
            SwitchState(Factory.Idle());
        }
        if (Ctx.possessCompleted)
        {
            Ctx.possessCompleted = false;
            SwitchState(Factory.Idle());
        }
    }

    public override string ReturnStateName()
    {
        string value = "QTEvent";
        return value;
    }
}
