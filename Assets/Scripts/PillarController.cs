using DG.Tweening;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PillarController : MonoBehaviour
{
    private int _currentStep;
    [SerializeField] private float _pillarMoveTime;

    [SerializeField] private Animator _anim;

    private float _startHeight;
    private float _endHeight = 0.1f;

    private int _steps = 10;
    private float _distPerStep;

    private List<Transform> _pillarElements = new List<Transform>();

    [SerializeField] private EventReference SpineCollapse;

    private void Start()
    {
        _startHeight = transform.position.y;
        _distPerStep = (_startHeight-_endHeight) / _steps;
        _currentStep = _steps;

        foreach(Transform child in transform.GetChild(0))
        {
            _pillarElements.Add(child);
        }
    }

    public void UpdatedInfluence(int influence)
    {
        int goalStep = _steps - (influence / _steps);

        float goalPos = goalStep * _distPerStep + _endHeight;

        if (goalStep < _currentStep)
        {
            StopAllCoroutines();
            StartCoroutine(MoveDown(goalPos));
            //Debug.Log("shit");
            for (int i = _steps - _currentStep; i < _steps - goalStep; i++)
            {
                //Debug.Log(i);
                //Debug.Log(_pillarMoveTime * Mathf.Abs(transform.position.y - goalPos));
                _pillarElements[i].DOShakePosition(_pillarMoveTime * Mathf.Abs(transform.position.y - goalPos), new Vector3(0.02f, 0, 0.02f), 20);
            }
            RuntimeManager.PlayOneShot(SpineCollapse, _pillarElements[_steps-_currentStep].transform.position);
        }
        else if (goalStep > _currentStep)
        {
            StopAllCoroutines();
            StartCoroutine(MoveUp(goalPos));
        }

        int crackingPillar = influence / _steps;
        float percent = (influence % _steps);
        percent = percent / 10;
        for(int i = 0; i < crackingPillar; i++) UpdateCracks(i, 1);
        for (int i = crackingPillar; i < _pillarElements.Count; i++) UpdateCracks(i, 0);
        UpdateCracks(crackingPillar, percent);
    }

    private void UpdateCracks(int index, float percent)
    {
        if (index > _pillarElements.Count - 1) return;
        _pillarElements[index].GetComponent<MeshRenderer>().materials[0].DOFloat(percent, "_CracksAmount", 0.3f);
        _pillarElements[index].GetComponent<MeshRenderer>().materials[1].DOFloat(percent * 0.7f, "_CracksAmount", 0.3f);
    }

    IEnumerator MoveUp(float goal)
    {
        float ratio = 0;
        float startTime = Time.time;
        float endTime = _pillarMoveTime * Mathf.Abs(transform.position.y - goal);

        Vector3 startPos = new Vector3(0, transform.position.y, 0);
        Vector3 targetPos = new Vector3(0, goal, 0);

        while (ratio < 1)
        {
            ratio = (Time.time - startTime) / (endTime);
            if (ratio > 1) ratio = 1;

            transform.position = Vector3.Lerp(startPos, targetPos, ratio);

            yield return null;
        }
        _currentStep = (int)(transform.position.y / _distPerStep);
    }

    IEnumerator MoveDown(float goal)
    {
        float ratio = 0;
        float startTime = Time.time;
        float endTime = _pillarMoveTime * Mathf.Abs(transform.position.y - goal);

        Vector3 startPos = new Vector3(0, transform.position.y, 0);
        Vector3 targetPos = new Vector3(0, goal, 0);

        while (ratio < 1)
        {
            ratio = (Time.time - startTime) / (endTime);
            if (ratio > 1) ratio = 1;

            transform.position = Vector3.Lerp(startPos, targetPos, ratio);

            yield return null;
        }
        _currentStep = (int)(transform.position.y / _distPerStep);
        if (_currentStep <= 0)
        {
            //endReached = true;
            _anim.SetTrigger("GetUp");
        }
    }
}
