using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NPCReleasedState : NPCBaseState
{
    public NPCReleasedState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        
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
        Ctx.anim.SetTrigger("released");
        SwitchState(Factory.Idle());
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.Released;
    }
}