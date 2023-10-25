using FMODUnity;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityDoor : MonoBehaviour
{
    [SerializeField] private float _openingTime = 0.4f;
    [SerializeField] private Transform _door;

    private bool _isIdle = true;

    [SerializeField] private EventReference OpenDoor;
    [SerializeField] private EventReference CloseDoor;

    public void OpenFromBack()
    {
        if (!_isIdle) return;
        StartCoroutine(RotateDoor(new Vector3(0, 270, 0)));
    }

    public void OpenFromFront()
    {
        if (!_isIdle) return;
        StartCoroutine(RotateDoor(new Vector3(0, 90, 0)));
    }

    IEnumerator RotateDoor(Vector3 rot)
    {
        _isIdle = false;
        float ratio = 0;
        Quaternion startRot = _door.localRotation;
        Quaternion endRot = Quaternion.Euler(rot);
        float startTime = Time.time;
        float endTime = _openingTime;

        if (_door.localEulerAngles.y == 180) RuntimeManager.PlayOneShot(OpenDoor, transform.position);

        while (ratio < 1)
        {
            ratio = (Time.time - startTime) / endTime;
            if (ratio > 1) ratio = 1;

            _door.localRotation = Quaternion.Lerp(startRot, endRot, ratio);
            yield return null;
        }

        if (_door.localEulerAngles.y != 180)
        {
            yield return new WaitForSeconds(1);
            StartCoroutine(RotateDoor(new Vector3(0, 180, 0))); 
        }
        else
        {
            RuntimeManager.PlayOneShot(CloseDoor, transform.position);
            _isIdle = true;
        }
    }
}
