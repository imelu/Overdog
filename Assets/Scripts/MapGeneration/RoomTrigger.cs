using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    private Room _room;
    public Room room { get { return _room; } set { _room = value; } }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponentInParent<PlayerStateManager>().currentRoom = room;
        }
        else if (other.CompareTag("NPC"))
        {
            other.GetComponentInParent<NPCAIStateManager>().currentRoom = room;
        }
    }
}
