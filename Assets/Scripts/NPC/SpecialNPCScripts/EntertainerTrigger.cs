using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EntertainerTrigger : MonoBehaviour
{
    [SerializeField] private NPCAIStateManager npc;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            NPCAIStateManager npcAI = other.GetComponentInParent<NPCAIStateManager>();
            if (npcAI.type == NPCAIStateManager.NPCType.security || npcAI.type == NPCAIStateManager.NPCType.boss) return;
            if (npc.playingMusic) npcAI.ListenToEntertainer(npc);
            npc.npcsInMusicRange.Add(npcAI);
        } 
        else if (other.CompareTag("Player"))
        {
            PlayerStateManager player = other.GetComponentInParent<PlayerStateManager>();
            npc.playersInMusicRange.Add(player);
            if ((player.isPlayerOne && npc.isCorruptedP2) || (!player.isPlayerOne && npc.isCorruptedP1))
            {
                npc.playingMusic = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            NPCAIStateManager npcAI = other.GetComponentInParent<NPCAIStateManager>();
            if (npcAI.type == NPCAIStateManager.NPCType.security || npcAI.type == NPCAIStateManager.NPCType.boss) return;
            npc.npcsInMusicRange.Remove(npcAI);
        }
        else if (other.CompareTag("Player"))
        {
            PlayerStateManager player = other.GetComponentInParent<PlayerStateManager>();
            npc.playersInMusicRange.Remove(player);
            if ((player.isPlayerOne && npc.isCorruptedP2) || (!player.isPlayerOne && npc.isCorruptedP1))
            {
                npc.playingMusic = false;
            }
        }
    }
}
