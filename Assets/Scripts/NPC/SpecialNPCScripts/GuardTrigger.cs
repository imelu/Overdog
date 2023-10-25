using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardTrigger : MonoBehaviour
{
    [SerializeField] private NPCAIStateManager guard;
    public void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            guard.GuardExit(col.GetComponent<PlayerStateManager>());
        }
    }

    public void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            guard.LeaveExit(col.GetComponent<PlayerStateManager>());
        }
    }
}
