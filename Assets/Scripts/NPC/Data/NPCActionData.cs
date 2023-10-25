using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;
using Random = UnityEngine.Random;

[Serializable]
public class Action
{
    [SerializeField]
    private NPCAIStateManager.NPCAction _NPCAction;
    public NPCAIStateManager.NPCAction NPCAction { get { return _NPCAction; } }
    [SerializeField]
    [Range(0, 10)]
    private int _likelyhood;
    public int likelyhood { get { return _likelyhood; } }
    [SerializeField]
    private NPCBaseState.NPCStates _fromState;
    public NPCBaseState.NPCStates fromState { get { return _fromState; } }

    [HideInInspector] public float rngWeight;
}

public class NextAction
{
    public NPCAIStateManager.NPCAction nextAction;
    public float actionDelay;
}

[CreateAssetMenu(fileName = "NPCActionData", menuName = "ScriptableObjects/NPCActionData", order = 1)]
public class NPCActionData : ScriptableObject
{
    public NPCAIStateManager.NPCType type;
    
    public List<Action> actions = new List<Action>();
    public List<Action> corruptedActions = new List<Action>();

    [SerializeField] private float IdleDelayMin;
    [SerializeField] private float IdleDelayMax;

    [Header("Normal NPC only")]
    [SerializeField] private float GroupIdleDelayMin;
    [SerializeField] private float GroupIdleDelayMax;

    public NextAction GetNextAction(NPCBaseState.NPCStates currentState, bool isCorrupted)
    {
        NextAction nextAction = new NextAction();
        List<Action> possibleActions = new List<Action>();
        float likelyhoodSum = 0;
        float delayMin = IdleDelayMin;
        float delayMax = IdleDelayMax;

        if (currentState == NPCBaseState.NPCStates.GroupIdle)
        {
            delayMin = GroupIdleDelayMin;
            delayMax = GroupIdleDelayMax;
        }

        if (!isCorrupted)
        {
            foreach (Action action in actions)
            {
                if (action.fromState == currentState)
                {
                    possibleActions.Add(action);
                    likelyhoodSum += action.likelyhood;
                }
            }
        }
        else
        {
            foreach (Action action in corruptedActions)
            {
                if (action.fromState == currentState)
                {
                    possibleActions.Add(action);
                    likelyhoodSum += action.likelyhood;
                }
            }
        }

        foreach (Action action in possibleActions)
        {
            action.rngWeight = (float)action.likelyhood / likelyhoodSum;
        }

        possibleActions.Sort((r1, r2) => r1.rngWeight.CompareTo(r2.rngWeight));

        nextAction.actionDelay = Random.Range(delayMin, delayMax);

        float rng = Random.Range(0f, 1f);
        float weights = 0;
        foreach (Action action in possibleActions)
        {
            weights += action.rngWeight;
            if (weights > rng)
            {
                nextAction.nextAction = action.NPCAction;
                return nextAction;
            }
        }
        return null;
    }
}