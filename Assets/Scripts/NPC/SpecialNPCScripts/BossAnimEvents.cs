using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAnimEvents : MonoBehaviour
{
    [SerializeField] private Transform _boss;

    public void MoveBossToIdle()
    {
        _boss.position += new Vector3(0, 0, 0.5055475f);
        _boss.GetComponent<NPCAIStateManager>().ActivateBoss();
    }

    public void MoveBossToSit()
    {
        _boss.position -= new Vector3(0, 0, 0.5055475f);
    }
}
