using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerStateManager;
using UnityEngine.UI;
using Unity.VisualScripting;
using DG.Tweening;

[Serializable]
public class CooldownSlot
{
    public Transform transform;
    [HideInInspector] public CooldownVisual cooldownVisual;

    public CooldownSlot(Transform slot)
    {
        transform = slot;
    }
}

[Serializable]
public class CooldownVisual
{
    public GameObject go;
    [HideInInspector] public int currentSlot;
}


public class CooldownHandler : MonoBehaviour
{
    private List<CooldownSlot> _slots = new List<CooldownSlot>();
    [SerializeField] private List<Transform> _slotsP1 = new List<Transform>();
    [SerializeField] private List<Transform> _slotsP2 = new List<Transform>();
    [SerializeField] private List<CooldownVisual> _cooldownVisuals = new List<CooldownVisual>();

    private PlayerStateManager _player;

    private int freeSlotIndex = 0;

    [SerializeField] private float _transitionTime;

    private void Awake()
    {
        _player = GetComponent<PlayerStateManager>();
    }

    private void Start()
    {
        if (_player.isPlayerOne) foreach (Transform slot in _slotsP1) _slots.Add(new CooldownSlot(slot));
        else foreach (Transform slot in _slotsP2) _slots.Add(new CooldownSlot(slot));
    }

    public void SetCooldown(float time, Abilities ability)
    {
        switch (ability)
        {
            case Abilities.corrupt:
                SetUpCooldownVisual(_slots[freeSlotIndex], _cooldownVisuals[0]);
                StartCoroutine(Cooldown(_slots[freeSlotIndex].cooldownVisual, time, ability));
                freeSlotIndex++;
                break;

            case Abilities.possess:
                SetUpCooldownVisual(_slots[freeSlotIndex], _cooldownVisuals[1]);
                StartCoroutine(Cooldown(_slots[freeSlotIndex].cooldownVisual, time, ability));
                freeSlotIndex++;
                break;

            case Abilities.attack:
                SetUpCooldownVisual(_slots[freeSlotIndex], _cooldownVisuals[2]);
                StartCoroutine(Cooldown(_slots[freeSlotIndex].cooldownVisual, time, ability));
                freeSlotIndex++;
                break;
        }
    }

    private void SetUpCooldownVisual(CooldownSlot slot, CooldownVisual visual)
    {
        slot.cooldownVisual = visual;
        slot.cooldownVisual.go.transform.SetParent(_slots[freeSlotIndex].transform, true);
        slot.cooldownVisual.go.transform.localPosition = Vector3.zero;
        slot.cooldownVisual.go.transform.localScale = Vector3.one;
        //slot.cooldownVisual.go.GetComponent<Image>().fillAmount = 0;
        slot.cooldownVisual.go.SetActive(true);
        visual.currentSlot = _slots.IndexOf(slot);
    }

    private void MoveSlotUp(CooldownSlot slot)
    {
        int index = _slots.IndexOf(slot);
        int newIndex = index - 1;

        slot.cooldownVisual.go.transform.SetParent(_slots[newIndex].transform, true);
        _slots[newIndex].cooldownVisual = slot.cooldownVisual;
        slot.cooldownVisual.currentSlot--;
        slot.cooldownVisual = null;

        // tween move ui image
        // tween scale ui image

        _slots[newIndex].cooldownVisual.go.transform.DOLocalMove(Vector3.zero, _transitionTime);
        _slots[newIndex].cooldownVisual.go.transform.DOScale(Vector3.one, _transitionTime); //  * _slots[newIndex].transform.localScale.x

        if (index < _slots.Count - 1)
        {
            if (_slots[index + 1].cooldownVisual != null) MoveSlotUp(_slots[index + 1]);
        }
    }

    IEnumerator Cooldown(CooldownVisual visual, float time, Abilities ability)
    {
        float startTime = Time.time;
        float endTime = time;

        float ratio = 0;

        Image image = visual.go.transform.GetChild(1).GetComponent<Image>();
        image.fillAmount = 0;

        while (ratio < 1)
        {
            ratio = (Time.time - startTime) / endTime;
            if (ratio > 1) ratio = 1;

            image.fillAmount = ratio;
            yield return null;
        }



        int index = visual.currentSlot;
        CooldownSlot slot = _slots[index];
        slot.cooldownVisual.go.SetActive(false);
        slot.cooldownVisual.go.transform.SetParent(slot.transform.parent, true);
        slot.cooldownVisual = null;
        if (index < _slots.Count - 1)
        {
            if (_slots[index + 1].cooldownVisual != null)
            {
                if (_slots[index + 1].cooldownVisual.go != null) MoveSlotUp(_slots[index + 1]);
            }
        }

        switch (ability)
        {
            case Abilities.corrupt:
                _player.corruptOnCD = false;
                break;

            case Abilities.possess:
                _player.possessOnCD = false;
                break;

            case Abilities.attack:
                _player.attackOnCD = false;
                break;
        }
        freeSlotIndex--;
    }
}
