using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCEntertainerPlayingState : NPCBaseState
{
    public NPCEntertainerPlayingState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        foreach(NPCAIStateManager npc in Ctx.npcsInMusicRange)
        {
            npc.ListenToEntertainer(Ctx);
        }
        if (Ctx.isCorruptedP1)
        {
            // piano anim
            Ctx.anim.SetBool("playPiano", true);
            Ctx.entertainerMusicP1.start();
        }
        else
        {
            // drums anim
            Ctx.entertainerAnimP2.SetBool("playDrums", true);
            Ctx.entertainerMusicP2.start();
        }
    }

    public override void UpdateState()
    {
        CheckSwitchState();
        if(Ctx.anim.GetBool("playPiano") && Ctx.isCorruptedP2)
        {
            Ctx.entertainerAnimP2.SetBool("playDrums", true);
            Ctx.anim.SetBool("playPiano", false);
            Ctx.entertainerMusicP1.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            Ctx.entertainerMusicP2.start();
        }
        if(Ctx.entertainerAnimP2.GetBool("playDrums") && Ctx.isCorruptedP1)
        {
            Ctx.anim.SetBool("playPiano", true);
            Ctx.entertainerAnimP2.SetBool("playDrums", false);
            Ctx.entertainerMusicP2.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            Ctx.entertainerMusicP1.start();
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
        foreach(NPCAIStateManager npc in Ctx.npcsInMusicRange)
        {
            if(npc.type != NPCAIStateManager.NPCType.boss && npc.type != NPCAIStateManager.NPCType.entertainer)
            {
                if(npc.type == NPCAIStateManager.NPCType.guard)
                {
                    npc.currentState = npc.states.Guard();
                    npc.currentState.EnterState();
                }
                else
                {
                    npc.currentState = npc.states.Walk();
                    npc.currentState.EnterState();
                }
            }
        }
        Ctx.anim.SetBool("playPiano", false);
        Ctx.entertainerAnimP2.SetBool("playDrums", false);
        Ctx.entertainerMusicP1.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        Ctx.entertainerMusicP2.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public override void CheckSwitchState()
    {
        if (!Ctx.playingMusic)
        {
            SwitchState(Factory.Idle());
        }
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.EntertainerPlaying;
    }
}
