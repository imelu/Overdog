using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTargeting : MonoBehaviour
{
    [SerializeField] private PlayerStateManager player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player.EnemyTargeted(other.GetComponentInParent<PlayerStateManager>());
        }

        if (other.CompareTag("NPC"))
        {
            player.AddTarget(other.GetComponentInParent<NPCAIStateManager>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player.EnemyLost(other.GetComponentInParent<PlayerStateManager>());
        }

        if (other.CompareTag("NPC"))
        {
            player.RemoveTarget(other.GetComponentInParent<NPCAIStateManager>());
        }
    }
}
