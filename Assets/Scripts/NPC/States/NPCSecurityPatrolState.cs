using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;

public class NPCSecurityPatrolState : NPCBaseState
{
    public NPCSecurityPatrolState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        Debug.Log("start patrol");
        /*if (Ctx.isLeadingPatrol)
        {
            Ctx.GetNextPatrolPoint();
        }*/
    }

    public override void UpdateState()
    {
        if (!Ctx.isLeadingPatrol) // && Time.frameCount % Ctx.agentInterval == 0
        {
            if (!Ctx.partnerPossessed)
            {
                //if (Ctx.agent.pathPending) return;
                //if (Ctx.agent.velocity.magnitude <= 0) Ctx.agent.SetDestination(Ctx.currentPatrolGoal);
                //Vector3 direction = (Ctx.patrolPartner.transform.position - Ctx.transform.position).normalized;
                


                //Ctx.agent.SetDestination(direction * (distance - 1f) + Ctx.transform.position);
                //Ctx.agent.speed = Ctx.speed + Ctx.securitySpeedInc * (distance - 1f);
                /*if(Ctx.patrolPartnerScript.currentState.ReturnStateName() != NPCStates.Idle || Vector3.Distance(Ctx.patrolPartner.transform.position, Ctx.transform.position) > 2f) //|| Ctx.patrolPartnerScript.currentState.ReturnStateName() == NPCStates.PoliceCorrupt || Ctx.patrolPartnerScript.currentState.ReturnStateName() == NPCStates.PolicePunish)
                {
                    Vector3 direction = (Ctx.patrolPartner.transform.position - Ctx.transform.position).normalized;
                    float distance = Vector3.Distance(Ctx.patrolPartner.transform.position, Ctx.transform.position);
                    Ctx.agent.speed = Ctx.speed * (distance);
                    Ctx.agent.SetDestination(direction * (distance - 1f) + Ctx.transform.position);
                }
                else
                {
                    Ctx.agent.ResetPath();
                }*/
            }
            else
            {
                if (Ctx.patrolPartner.GetComponent<PlayerStateManager>().currentState.ReturnStateName().Equals("Walk") || Vector3.Distance(Ctx.patrolPartner.transform.position, Ctx.transform.position) > 2f)
                {
                    Vector3 direction = (Ctx.patrolPartner.transform.position - Ctx.transform.position).normalized;
                    float distance = Vector3.Distance(Ctx.patrolPartner.transform.position, Ctx.transform.position);


                    Ctx.agent.SetDestination(direction * (distance - 1f) + Ctx.transform.position);

                    if (Ctx.patrolPartner.GetComponent<PlayerStateManager>().sprint) Ctx.agent.speed = Ctx.sprintSpeed;
                    else Ctx.agent.speed = Ctx.speed;

                    Ctx.agent.speed += Ctx.securitySpeedInc * distance;
                }
                else Ctx.agent.ResetPath();
            }
        }
        else
        {
            if (!Ctx.partnerPossessed && !Ctx.patrolPartnerScript.partnerPossessed) Ctx.SyncUp();

            /*CheckSwitchState();

            if (Ctx.agent.isPathStale)
            {
                Ctx.agent.SetDestination(Ctx.currentPatrolGoal);
            }
            //Ctx.SyncUp();*/
        }

        CheckSwitchState();
        //if (Ctx.agent.isPathStale && !Ctx.passedGate) Ctx.currentState.EnterState();

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
        if (Ctx.punishPlayer && Ctx.isLeadingPatrol)
        {
            Ctx.passedGate = false;
            SwitchState(Factory.PolicePunish());
        }
        if (Ctx.moveToPlayer)// && Ctx.isLeadingPatrol)
        {
            Ctx.agent.ResetPath();
            Ctx.passedGate = false;
            SwitchState(Factory.PoliceCorrupt());
        }
        if (Ctx.patrolPartnerScript.partnerPossessed || Ctx.partnerPossessed) return;
        if ((Vector3.Distance(Ctx.transform.position, Ctx.agent.destination) <= Ctx.stopProximity) && !Ctx.agent.pathPending) // && !Ctx.agent.isPathStale)
        {
            Ctx.passedGate = false;

            Debug.Log("patrol complete");

            /*if (Ctx.isLeadingPatrol)
            {
                Ctx.agent.ResetPath();
                Ctx.anim.SetTrigger("look");
                Ctx.GoalReached();
                SwitchState(Factory.Idle());
            }
            if(!Ctx.partnerPossessed) SwitchState(Factory.Idle());*/


            if (!Ctx.partnerPossessed)
            {
                Ctx.agent.ResetPath();
                Ctx.GoalReached();
                SwitchState(Factory.Idle());

                if(Ctx.isLeadingPatrol) Ctx.anim.SetTrigger("look");
            }
        }
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.SecurityPatrol;
    }
}