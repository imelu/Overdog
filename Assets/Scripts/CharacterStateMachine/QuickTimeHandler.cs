using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DG.Tweening;
using FMODUnity;

public class QuickTimeHandler : MonoBehaviour
{
    private PlayerStateManager playerStateManager;
    [SerializeField] private List<Sprite> _iconsController = new List<Sprite>();
    [SerializeField] private List<Sprite> _iconsKeyboardP1 = new List<Sprite>();
    [SerializeField] private List<Sprite> _iconsKeyboardP2 = new List<Sprite>();
    private List<Sprite> _iconsToUse;
    [SerializeField] private Image _iconRenderer;
    [SerializeField] private Image _iconTimer;
    [SerializeField] private Transform _iconTween; //tween transform

    [SerializeField] private float timePerButton;
    private float timePassed;
    private bool timerStarted = false;

    [Serializable]
    public enum QuickTimeAction
    {
        x,
        circle,
        square,
        triangle,
        left,
        right,
        up,
        down,
        NONE
    }

    private int QTActionsCount;

    private Queue<QuickTimeAction> QTActions = new Queue<QuickTimeAction>();
    private QuickTimeAction nextAction;

    [SerializeField] private List<QuickTimeAction> PossessActions = new List<QuickTimeAction>();
    private int nextActionIndex;

    public enum QuickTimePurpose
    {
        corrupt,
        possess
    }

    private QuickTimePurpose purpose;

    [SerializeField] private EventReference ButtonPress;

    // Start is called before the first frame update
    void Start()
    {
        playerStateManager = GetComponent<PlayerStateManager>();
        QTActionsCount = Enum.GetNames(typeof(QuickTimeAction)).Length;

        if(GetComponent<PlayerInput>() == null)
        {
            if (playerStateManager.isPlayerOne)
            {
                _iconsToUse = _iconsKeyboardP1;
                //Debug.Log("controller icons P1");
            }
            else
            {
                _iconsToUse = _iconsKeyboardP2;
                //Debug.Log("controller icons P2");
            }
        }
        else
        {
            if(GetComponent<PlayerInput>().currentControlScheme.Equals("Gamepad"))
            {
                _iconsToUse = _iconsController;
                //Debug.Log("controller icons");
            }
            else
            {
                _iconsToUse = _iconsKeyboardP1;
                //Debug.Log("keyboard icons");
            }
        }
    }

    private void Update()
    {
        UpdateTimer();
    }

    public void StartCorruptQTEvent(int difficulty)
    {
        _iconTween.localScale = Vector3.one;

        purpose = QuickTimePurpose.corrupt;
        QTActions = GetQTActions(difficulty);
        nextAction = GetNextAction();
        _iconRenderer.transform.parent.gameObject.SetActive(true);
    }

    public void StartPossessQTEvent()
    {
        _iconTween.localScale = Vector3.one;

        purpose = QuickTimePurpose.possess;
        nextActionIndex = 0;
        nextAction = GetNextAction();
        _iconRenderer.transform.parent.gameObject.SetActive(true);
    }

    private void QuickTimeEventSuccess()
    {
        _iconRenderer.transform.parent.gameObject.SetActive(false);
        timerStarted = false;
        if(purpose == QuickTimePurpose.corrupt) playerStateManager.CorruptionSucceeded();
        else if(purpose == QuickTimePurpose.possess) playerStateManager.PossessionSucceeded();
    }

    private void QuickTimeEventFailed()
    {
        timePassed = 0;
        _iconRenderer.transform.parent.gameObject.SetActive(false);
        timerStarted = false;
        if (purpose == QuickTimePurpose.corrupt) playerStateManager.CorruptionFailed();
        else if (purpose == QuickTimePurpose.possess) playerStateManager.PossessionFailed();
    }

    private Queue<QuickTimeAction> GetQTActions(int difficulty)
    {
        Queue<QuickTimeAction> actions = new Queue<QuickTimeAction>();

        for(int i = 0; i < difficulty; i++)
        {
            actions.Enqueue((QuickTimeAction)Random.Range(0, QTActionsCount - 1));
        }

        return actions;
    }

    private void StartTimer()
    {
        timePassed = 0;
        timerStarted = true;
    }

    private void UpdateTimer()
    {
        if (timerStarted) timePassed += Time.deltaTime;
        if (timePassed >= timePerButton)
        {
            QuickTimeEventFailed();
        }
        _iconTimer.fillAmount = 1 - timePassed / timePerButton;
    }

    private QuickTimeAction GetNextAction()
    {
        QuickTimeAction action = QuickTimeAction.triangle;
        if (purpose == QuickTimePurpose.corrupt) action = QTActions.Dequeue();
        else if (purpose == QuickTimePurpose.possess)
        {
            action = PossessActions[nextActionIndex];
            nextActionIndex++;
        }
        _iconRenderer.sprite = _iconsToUse[(int)action];
        StartTimer();
        return action;
    }

    private void InputAction(QuickTimeAction input)
    {
        if (!playerStateManager.currentState.ReturnStateName().Equals("QTEvent") || !timerStarted) return;
        if (input == nextAction)
        {
            RuntimeManager.PlayOneShot(ButtonPress, transform.position);

            // tween right button
            _iconTween.DOPunchScale(new Vector3 (0.4f, 0.4f, 0.4f), 0.4f, 5, 0.5f);

            if(purpose == QuickTimePurpose.corrupt)
            {
                if (QTActions.Count != 0) nextAction = GetNextAction();
                else
                {
                    nextAction = QuickTimeAction.NONE;
                    if (playerStateManager.npcToCorrupt.type == NPCAIStateManager.NPCType.boss)
                    {
                        purpose = QuickTimePurpose.possess;
                        playerStateManager.npcToPossess = playerStateManager.npcToCorrupt;
                    }
                    QuickTimeEventSuccess();
                }
            }
            else if(purpose == QuickTimePurpose.possess)
            {
                if (nextActionIndex < PossessActions.Count) nextAction = GetNextAction();
                else
                {
                    QuickTimeEventSuccess();
                }
            }
        }
        else
        {
            if (nextAction == QuickTimeAction.NONE && purpose == QuickTimePurpose.corrupt) return;
            if (nextActionIndex > PossessActions.Count && purpose == QuickTimePurpose.possess) return;

            // tween wrong button
            _iconTween.DOShakeScale(0.4f, 1f, 10, 90, true, ShakeRandomnessMode.Full).OnComplete(() => QuickTimeEventFailed()); //wait for fail
        }
    }

    #region input

    public void OnX()
    {
        InputAction(QuickTimeAction.x);
    }

    public void OnSquare()
    {
        InputAction(QuickTimeAction.square);
    }

    public void OnCircle()
    {
        InputAction(QuickTimeAction.circle);
    }

    public void OnTriangle()
    {
        InputAction(QuickTimeAction.triangle);
    }

    public void OnLeft()
    {
        InputAction(QuickTimeAction.left);
    }

    public void OnRight()
    {
        InputAction(QuickTimeAction.right);
    }

    public void OnUp()
    {
        InputAction(QuickTimeAction.up);
    }

    public void OnDown()
    {
        InputAction(QuickTimeAction.down);
    }

    #endregion
}
