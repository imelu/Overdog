using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PunchingBagAnimation : MonoBehaviour
{

    [SerializeField] private Transform PunchingBag;

    public void Punch()
    {
        PunchingBag.DOPunchRotation(new Vector3(45f, 0.0f, 0.0f), 0.5f, 10, 0.8f);
    }

}
