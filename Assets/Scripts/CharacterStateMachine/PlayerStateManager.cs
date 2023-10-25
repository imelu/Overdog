using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using Cinemachine;
using Unity.VisualScripting;
using Unity.Burst.CompilerServices;
using FMODUnity;
using DG.Tweening;
using FMOD.Studio;
using TMPro;

public class PlayerStateManager : MonoBehaviour
{
    private PlayerBaseState _currentState;
    public PlayerBaseState currentState { get { return _currentState; } set { _currentState = value; } }
    private PlayerStateFactory _states;

    private NavMeshObstacle _obstacle;

    [SerializeField] private CinemachineFreeLook _freelookCam;
    public CinemachineFreeLook freeLookCam { get { return _freelookCam; } }

    public bool isPlayerOne;

    public InputHandler IH;

    private PlayerInput _input;

    private PlayerVisuals _visuals;
    public PlayerVisuals visuals { get { return _visuals; } }

    private CooldownHandler _cooldowns;
    public CooldownHandler cooldowns { get { return _cooldowns; } }

    [SerializeField] private Animator _anim;

    private QuickTimeHandler _quickTime;
    public QuickTimeHandler quickTime { get { return _quickTime; } }

    public enum Abilities
    {
        possess,
        corrupt,
        attack
    }

    private NPCAIStateManager.NPCType _type = NPCAIStateManager.NPCType.normal;
    public NPCAIStateManager.NPCType type { get { return _type; } }

    private Room _currentRoom;
    public Room currentRoom { get { return _currentRoom; } set { _currentRoom = value; } }

    private bool _qtEvent;
    public bool qtEvent { get { return _qtEvent; } set { _qtEvent = value; } }

    private bool _inSecurityGate;

    private bool _backToMenu;
    public bool backToMenu { get { return _backToMenu; } set { _backToMenu = value; } }

    [SerializeField] private GameObject _ConfirmToQuitText;
    public GameObject ConfirmToQuitText { get { return _ConfirmToQuitText; } } 

    #region targeting

    [Header("Targeting")]

    private NPCAIStateManager targetedNPC;
    [SerializeField] private LayerMask layerMaskNPC;

    private PlayerStateManager targetedPlayer;
    [SerializeField] private LayerMask layerMaskPlayer;

    #endregion

    #region possess

    [Header("Possess")]

    [SerializeField] private float _possessRange = 5;
    private bool _possessAim = false;
    private bool _possessAimHit = false;
    private bool _possessCompleted = false;
    [SerializeField] private float _possessCooldown = 5;
    private bool _possessOnCD = false;
    public bool possessOnCD { get { return _possessOnCD; } set { _possessOnCD = value; } }

    private NPCAIStateManager _npcToPossess;
    public NPCAIStateManager npcToPossess { get { return _npcToPossess; } set { _npcToPossess = value; } }

    public float possessRange { get { return _possessRange; } }
    public bool possessAim { get { return _possessAim; } }
    public bool possessAimHit { get { return _possessAimHit; } }
    public bool possessCompleted { get { return _possessCompleted; } set { _possessCompleted = value; } }

    private GameObject _PossessedNPC;
    public GameObject PossessedNPC { get { return _PossessedNPC; } set { _PossessedNPC = value; } }

    private bool _isInGate;
    public bool isInGate { get { return _isInGate; } set { _isInGate = value; } }

    [SerializeField] private Transform _camLookAt;
    public Transform camLookAt { get { return _camLookAt; } }

    [SerializeField] private float _camSpeed;
    public float camSpeed { get { return _camSpeed; } }

    [SerializeField] private float _camSwingTime;
    public float camSwingTime { get { return _camSwingTime; } }

    private bool _possessing;
    public bool possessing { get { return _possessing; } set { _possessing = value; } }

    [SerializeField] private GameObject _TagP1;
    [SerializeField] private GameObject _TagP2;

    #endregion

    #region corrupt

    [Header("Corrupt")]

    [SerializeField] private float _corruptRange = 5;
    private bool _corruptAim = false;
    private bool _corruptAimHit = false;
    private bool _corruptCompleted = false;
    //[SerializeField] private float _corruptCooldown = 5;
    private bool _corruptOnCD = false;
    public bool corruptOnCD { get { return _corruptOnCD; } set { _corruptOnCD = value; } }
    private NPCAIStateManager _npcToCorrupt;
    public NPCAIStateManager npcToCorrupt { get { return _npcToCorrupt; } set { _npcToCorrupt = value; } }

    public float corruptRange { get { return _corruptRange; } }
    public bool corruptAim { get { return _corruptAim; } }
    public bool corruptAimHit { get { return _corruptAimHit; } }
    public bool corruptCompleted { get { return _corruptCompleted; } set { _corruptCompleted = value; } }

    private List<NPCAIStateManager> _lastCorruptedTarget = new List<NPCAIStateManager>();
    public List<NPCAIStateManager> lastCorruptedTarget { get { return _lastCorruptedTarget; } }

    #endregion

    #region attack

    [Header("Attack")]

    [SerializeField] private float _attackRange = 4;
    public float attackRange { get { return _attackRange; } }
    private bool _attackAim = false;
    public bool attackAim { get { return _attackAim; } }
    [SerializeField] private float _attackCooldown = 5;
    private bool _attackOnCD = false;
    public bool attackOnCD { get { return _attackOnCD; } set { _attackOnCD = value; } }

    [SerializeField] private GameObject _AttackTrigger;

    [SerializeField] private int _atkInfluenceDecrease;
    public int atkInfluenceDecrease { get { return _atkInfluenceDecrease; } }

    public GameObject AttackTrigger { get { return _AttackTrigger; } }

    private List<NPCAIStateManager> _targetedNPCs = new List<NPCAIStateManager>();
    public List<NPCAIStateManager> targetedNPCs { get { return _targetedNPCs; } }

    [SerializeField] private float _punishDuration;
    public float punishDuration { get { return _punishDuration; } }

    [SerializeField] private GameObject _attackParticle;

    #endregion

    #region suspicion

    [Header("Suspicion")]

    [SerializeField] private float _suspicionThreshold;
    public float suspicionThreshold { get { return _suspicionThreshold; } }
    [SerializeField] private float _suspicionDecSpeed;
    [SerializeField] private float _corruptSuspicion;
    [SerializeField] private float _possessSuspicion;
    [SerializeField] private float _attackSuspicion;
    private float _currentSuspicion;
    public float currentSuspicion { get { return _currentSuspicion; } set { _currentSuspicion = value; } }

    #endregion

    #region influence

    [Header("Influence")]

    [SerializeField] private int startInfluence = 1;
    private int _currentInfluence = 0;
    public int currentInfluence { get { return _currentInfluence; } }

    private PillarController _bossPillar;

    #endregion

    #region CharacterController

    [Header("Character Controller")]

    [SerializeField] Transform _cam;
    public Transform cam { get { return _cam; } }
    private CharacterController controller;

    private bool _sprint = false;
    private bool _movePlayer = false;
    public bool movePlayer { get { return _movePlayer; } }
    [SerializeField] private float speed = 12f;
    [SerializeField] private float sprintSpeed = 20f;
    private Vector3 direction;

    private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    private float gravity = -150f;
    private Vector3 velocity;
    private bool isGrounded;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    public bool sprint { get { return _sprint; } }

    #endregion

    #region avoiding

    [SerializeField] private GameObject _AvoidanceTrigger;
    public GameObject AvoidanceTrigger { get { return _AvoidanceTrigger; } }

    private bool _gotPunished;
    public bool gotPunished { get { return _gotPunished; } }

    #endregion

    #region punishments

    [Header("Punishments")]
    [SerializeField] private float _wrongTargetTimer;
    public float wrongTargetTimer { get { return _wrongTargetTimer; } }
    [SerializeField] private float _failedQTEventTimer;
    public float failedQTEventTimer { get { return _failedQTEventTimer; } }

    #endregion

    #region animation

    [SerializeField] private int _animInterval;
    public int animInterval { get { return _animInterval; } }

    [SerializeField] private Transform _victoryCam;
    public Transform victoryCam { get { return _victoryCam; } }

    private static readonly int _splitscreenScaleDiffP1 = Shader.PropertyToID("_splitscreenScaleDiffP1");
    private static readonly int _splitscreenScaleDiffP2 = Shader.PropertyToID("_splitscreenScaleDiffP2");
    #endregion

    #region sounds

    [Header("SFX")]
    [SerializeField] private EventReference Target;

    [SerializeField] private EventReference PossessSuccess;
    [SerializeField] private EventReference PossessFail;

    [SerializeField] private EventReference CorruptSuccess;
    [SerializeField] private EventReference CorruptFail;

    [SerializeField] private EventReference AttackTarget;
    [SerializeField] private EventReference AttackSuccess;
    [SerializeField] private EventReference AttackFail;

    [SerializeField] private EventReference MetalDetector;

    private EventInstance _targetSoundInstance;

    #endregion

    private void Awake()
    {
        _obstacle = GetComponent<NavMeshObstacle>();
        _visuals = GetComponent<PlayerVisuals>();
        controller = GetComponent<CharacterController>();
        _quickTime = GetComponent<QuickTimeHandler>();
        _cooldowns = GetComponent<CooldownHandler>();
        _input = GetComponent<PlayerInput>();
        _states = new PlayerStateFactory(this);
        _currentState = _states.Idle();
        _currentState.EnterState();

        //visuals.SetStartMats(isPlayerOne);

        AttackTrigger.GetComponent<SphereCollider>().radius = attackRange;

        ParticleSystem ps = _attackParticle.GetComponent<ParticleSystem>();
        var sh = ps.shape;
        sh.radius = attackRange;
        if (isPlayerOne) _attackParticle.layer = 8;
        else _attackParticle.layer = 9;

        if (isPlayerOne) _bossPillar = GameObject.FindGameObjectWithTag("BossPillarP1").GetComponent<PillarController>();
        else _bossPillar = GameObject.FindGameObjectWithTag("BossPillarP2").GetComponent<PillarController>();
    }

    private void Start()
    {
        IncreaseInfluence(startInfluence);
        if (isPlayerOne && _input != null) _input.actions.Disable();
    }

    void Update()
    {
        _currentState.UpdateStates();
        ApplyGravity();
        ReduceSuspicion();
        if (Time.frameCount % animInterval == 0)
        {
            UpdateWalkingBlend();
        }
    }

    public void EnablePlayer()
    {
        if (_input != null) _input.actions.Enable();
    }

    public void RemovePlayerActions()
    {
        if (_input != null)
        {
            _input.actions = null;
        }
        else
        {
            transform.GetComponentInParent<PlayerInput>().actions = null;
        }

    }

    private void FixedUpdate()
    {
        _currentState.FixedUpdateStates();
    }

    private void OnTriggerEnter(Collider other)
    {
        _currentState.OnTriggerEnter(other);
        if (other.CompareTag("JanitorDoorBack"))
        {
            other.GetComponentInParent<JanitorDoor>().OpenFromBack();
        }

        if (other.CompareTag("JanitorDoorFront") && type == NPCAIStateManager.NPCType.janitor)
        {
            other.GetComponentInParent<JanitorDoor>().OpenFromFront();
        }

        if (other.CompareTag("SecurityDoorFront") && (type == NPCAIStateManager.NPCType.security || type == NPCAIStateManager.NPCType.guard))
        {
            _inSecurityGate = true;
            other.GetComponentInParent<SecurityDoor>().OpenFromFront();
        }

        if (other.CompareTag("SecurityDoorBack") && (type == NPCAIStateManager.NPCType.security || type == NPCAIStateManager.NPCType.guard))
        {
            _inSecurityGate = true;
            other.GetComponentInParent<SecurityDoor>().OpenFromBack();
        }

        if (other.CompareTag("GateTrigger") && !_inSecurityGate)
        {
            RuntimeManager.PlayOneShot(MetalDetector, transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SecurityDoorBack") || other.CompareTag("SecurityDoorFront"))
        {
            _inSecurityGate = false;
        }
        if (other.CompareTag("GateTrigger"))
        {
            isInGate = !isInGate;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        _currentState.OnCollisionEnter(collision);
    }

    #region movement

    public void OnMove(InputValue inputValue)
    {
        if (currentState.ReturnStateName().Equals("QTEvent"))
        {
            direction = Vector3.zero;
            _movePlayer = false;
            return;
        }
        Vector2 MoveDelta = inputValue.Get<Vector2>();
        direction = new Vector3(MoveDelta.x, 0, MoveDelta.y);
        if (direction.magnitude > 1) direction = direction.normalized;
        _movePlayer = true;
    }

    public void OnSprint()
    {
        _sprint = !sprint;
    }

    private void ApplyGravity()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    public void MoveCharacter()
    {
        if (direction.magnitude >= 0.1f)
        {
            //if (possessing) return;
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);
            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            if (!sprint) controller.Move(moveDir.normalized * speed * Time.deltaTime);
            else controller.Move(moveDir.normalized * sprintSpeed * Time.deltaTime);
        }
        else
        {
            _movePlayer = false;
        }
    }

    #endregion

    #region targeting

    public void TargetNPC(float abilityRange, bool abilityAim)
    {

        RaycastHit hit;

        if (Physics.Raycast(transform.position, new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z), out hit, Mathf.Infinity, layerMaskNPC) && abilityAim)
        {

            NPCAIStateManager npcAI = hit.collider.GetComponentInParent<NPCAIStateManager>();
            if (npcAI == null) return;
            if (npcAI.getCorrupted) return;
            if (possessAim && (npcAI.type == NPCAIStateManager.NPCType.entertainer)) return;
            if (corruptAim && ((npcAI.isCorruptedP1 && isPlayerOne) || (npcAI.isCorruptedP2 && !isPlayerOne))) return; // cant corrupt your corrupted targets
            if (possessAim && ((npcAI.isCorruptedP1 && !isPlayerOne) || (npcAI.isCorruptedP2 && isPlayerOne))) return; // cant possess enemy corrupted targets

            if (hit.distance <= abilityRange)
            {
                if (targetedNPC != npcAI)
                {
                    ResetTarget();
                    targetedNPC = npcAI;
                    SetTarget();
                    PlayTargetSound();
                }
                //if(isPlayerOne) targetedNPC.Mesh.gameObject.GetComponent<SkinnedMeshRenderer>().material = matTargeted;
                //else targetedNPC.Mesh2.gameObject.GetComponent<SkinnedMeshRenderer>().material = matTargeted;
            }
            else ResetTarget();
        }
        else ResetTarget();

        if (Physics.Raycast(transform.position, new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z), out hit, Mathf.Infinity, layerMaskPlayer) && abilityAim)
        {
            if (this == hit.collider.GetComponent<PlayerStateManager>())
            {
                ResetTargetPlayer();
                return;
            }
            if (hit.distance <= abilityRange)
            {
                if (targetedPlayer != hit.collider.GetComponent<PlayerStateManager>())
                {
                    ResetTargetPlayer();
                    targetedPlayer = hit.collider.GetComponent<PlayerStateManager>();
                    SetPlayerTarget();
                    PlayTargetSound();
                }
                //if (isPlayerOne) targetedPlayer.GetComponent<PlayerVisuals>().MeshP1.GetComponent<SkinnedMeshRenderer>().material = matTargeted;
                //else targetedPlayer.GetComponent<PlayerVisuals>().MeshP2.GetComponent<SkinnedMeshRenderer>().material = matTargeted;
            }
            else ResetTargetPlayer();
        }
        else ResetTargetPlayer();
    }

    private void SetTarget()
    {
        SkinnedMeshRenderer meshRenderer;

        if (isPlayerOne)
        {
            meshRenderer = targetedNPC.Mesh.gameObject.GetComponent<SkinnedMeshRenderer>();
        }
        else
        {
            meshRenderer = targetedNPC.Mesh2.gameObject.GetComponent<SkinnedMeshRenderer>();
        }

        foreach (Material mat in meshRenderer.materials)
        {
            mat.SetInt("_targeted", 1);
        }
    }

    private void SetTarget(NPCAIStateManager npc)
    {
        SkinnedMeshRenderer meshRenderer;

        if (isPlayerOne)
        {
            meshRenderer = npc.Mesh.gameObject.GetComponent<SkinnedMeshRenderer>();
        }
        else
        {
            meshRenderer = npc.Mesh2.gameObject.GetComponent<SkinnedMeshRenderer>();
        }

        foreach (Material mat in meshRenderer.materials)
        {
            mat.SetInt("_targeted", 1);
        }
    }

    private void ResetTarget()
    {
        if (targetedNPC != null)
        {
            SkinnedMeshRenderer meshRenderer;

            if (isPlayerOne)
            {
                meshRenderer = targetedNPC.Mesh.gameObject.GetComponent<SkinnedMeshRenderer>();
            }
            else
            {
                meshRenderer = targetedNPC.Mesh2.gameObject.GetComponent<SkinnedMeshRenderer>();
            }

            foreach (Material mat in meshRenderer.materials)
            {
                mat.SetInt("_targeted", 0);
            }
        }
        targetedNPC = null;
    }

    private void ResetTarget(NPCAIStateManager npc)
    {
        SkinnedMeshRenderer meshRenderer;

        if (isPlayerOne)
        {
            meshRenderer = npc.Mesh.gameObject.GetComponent<SkinnedMeshRenderer>();
        }
        else
        {
            meshRenderer = npc.Mesh2.gameObject.GetComponent<SkinnedMeshRenderer>();
        }

        foreach (Material mat in meshRenderer.materials)
        {
            mat.SetInt("_targeted", 0);
        }
    }

    private void SetPlayerTarget()
    {
        SkinnedMeshRenderer meshRenderer;

        if (isPlayerOne)
        {
            meshRenderer = targetedPlayer.GetComponent<PlayerVisuals>().MeshP1.gameObject.GetComponent<SkinnedMeshRenderer>();
        }
        else
        {
            meshRenderer = targetedPlayer.GetComponent<PlayerVisuals>().MeshP2.gameObject.GetComponent<SkinnedMeshRenderer>();
        }

        foreach (Material mat in meshRenderer.materials)
        {
            mat.SetInt("_targeted", 1);
        }
    }

    private void ResetTargetPlayer()
    {
        if (targetedPlayer != null)
        {
            SkinnedMeshRenderer meshRenderer;

            if (isPlayerOne)
            {
                meshRenderer = targetedPlayer.GetComponent<PlayerVisuals>().MeshP1.gameObject.GetComponent<SkinnedMeshRenderer>();
            }
            else
            {
                meshRenderer = targetedPlayer.GetComponent<PlayerVisuals>().MeshP2.gameObject.GetComponent<SkinnedMeshRenderer>();
            }

            foreach (Material mat in meshRenderer.materials)
            {
                mat.SetInt("_targeted", 0);
            }
        }
        targetedPlayer = null;
    }

    private void PlayTargetSound()
    {
        if (!_targetSoundInstance.isValid())
        {
            _targetSoundInstance = RuntimeManager.CreateInstance(Target);
        }
        if (GlobalGameManager.Instance.IsPlaying(_targetSoundInstance))
        {
            _targetSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        //_targetSoundInstance = RuntimeManager.CreateInstance(Target);
        _targetSoundInstance.start();
    }


    #endregion

    #region possess

    public void OnPossessAim()
    {
        if (_possessOnCD || currentState.ReturnStateName().Equals("QTEvent") || gotPunished || qtEvent || corruptAim) return;
        _possessAim = true;
    }

    public void OnPossessRelease()
    {
        if (qtEvent) return;

        if (targetedPlayer != null)
        {
            GetPunished(wrongTargetTimer);
            //Debug.Log("tried to possess player");
            IncreaseSuspicion(_possessSuspicion);
            //ResetTargetPlayer();
        }

        if (targetedNPC != null && !targetedNPC.getCorrupted)
        {
            possessing = true;
            _npcToPossess = targetedNPC.GetComponent<NPCAIStateManager>();
            _possessAimHit = true;
            _npcToPossess.CorruptPossessStart();
        }
        ResetTarget();
        _possessAim = false;
    }

    private void WarpTo(Vector3 newPos)
    {
        controller.enabled = false;
        transform.position = new Vector3(newPos.x, transform.position.y, newPos.z); //  + Vector3.up;
        controller.enabled = true;
    }

    private void PossessSpecialNPC(NPCAIStateManager npc)
    {
        switch (npc.type)
        {
            case NPCAIStateManager.NPCType.normal:
                if (npc.groupManager != null)
                {
                    npc.groupManager.LeaveGroup(npc);
                }
                break;

            case NPCAIStateManager.NPCType.guard:
                npc.agent.enabled = true;
                npc.obstacle.enabled = false;
                break;

            case NPCAIStateManager.NPCType.janitor:

                break;

            case NPCAIStateManager.NPCType.security:
                if (!npc.isLeadingPatrol)
                {
                    npc.isLeadingPatrol = true;
                    //npc.agent.speed -= npc.securitySpeedInc;
                    npc.patrolPartnerScript.isLeadingPatrol = false;
                    //npc.patrolPartnerScript.agent.speed += npc.patrolPartnerScript.securitySpeedInc;

                    if (npc.patrolPartnerScript.pathingCor != null) npc.patrolPartnerScript.StopSecurityCoroutine();
                }
                else
                {
                    npc.StopSecurityCoroutine();
                }
                npc.patrolPartnerScript.partnerPossessed = true;
                npc.patrolPartnerScript.patrolPartner = gameObject;
                npc.patrolPartnerScript.agent.areaMask |= 1 << 4;
                if (npc.patrolPartnerScript.currentState.ReturnStateName() != NPCBaseState.NPCStates.SecurityPatrol)
                {
                    npc.patrolPartnerScript.waitingForPartner = true;
                    npc.waitingForPartner = true;
                    npc.patrolPartnerScript.startPatrol = true;
                    npc.patrolPartnerScript.currentState = npc.states.SecurityPatrol();
                }
                _npcToPossess.CorruptPossessEnd();
                break;
        }
    }

    public void PossessionFailed()
    {
        if (!qtEvent) return;
        //Debug.Log("possess fail");
        IncreaseSuspicion(_possessSuspicion); // maybe add "possessFailedSuspicion"
        _possessCompleted = true;
        _possessAimHit = false;
        _npcToPossess.CorruptPossessEnd();
        possessing = false;
        qtEvent = false;

        if (_npcToPossess.type == NPCAIStateManager.NPCType.boss)
        {
            if (_lastCorruptedTarget.Count > 0)
            {
                _lastCorruptedTarget[_lastCorruptedTarget.Count - 1].UncorruptTargetNPC();
                _lastCorruptedTarget.Remove(_lastCorruptedTarget[_lastCorruptedTarget.Count - 1]);
            }
            else
            {
                ReduceInfluence(10);
            }
        }
        else
        {
            GetPunished(failedQTEventTimer);
        }
        _npcToPossess = null;
        RuntimeManager.PlayOneShot(PossessFail, transform.position);
    }

    public void PossessionSucceeded()
    {
        _obstacle.carving = false;
        StartCoroutine(PossessCameraSwing());
        qtEvent = false;
        if (npcToPossess.type != NPCAIStateManager.NPCType.boss) npcToPossess.anim.SetTrigger("possessed");
        visuals.RemoveCurrentBlood();

        RuntimeManager.PlayOneShot(PossessSuccess, transform.position);
    }

    private void PossessCameraMovementComplete()
    {
        // TEMPORARY
        if (npcToPossess.type == NPCAIStateManager.NPCType.boss)
        {
            _currentInfluence = 1;
            victoryCam.GetComponent<Camera>().rect = cam.GetComponent<Camera>().rect;
            victoryCam.parent.parent = null;
            victoryCam.parent.position = Vector3.zero;
            victoryCam.parent.eulerAngles = Vector3.zero;
            victoryCam.gameObject.SetActive(true);

            npcToPossess.anim.SetTrigger("SitDown");

            StartCoroutine(TakeOverScreen());

            GlobalGameManager.Instance.npcManager.SendAllToMainHall();
            return;
        }

        possessCompleted = true;
        _possessAimHit = false;
        npcToPossess.agent.areaMask |= 1 << 4;
        if (npcToPossess.agent.isActiveAndEnabled) npcToPossess.agent.ResetPath();
        npcToPossess.agent.enabled = false;

        if (PossessedNPC != null)
        {
            npcToPossess.gameObject.SetActive(false);
            PossessedNPC.transform.position = new Vector3(transform.position.x, PossessedNPC.transform.position.y, transform.position.z);
            PossessedNPC.transform.rotation = transform.rotation;
            WarpTo(npcToPossess.transform.position);

            visuals.SetBloodToZero();

            transform.rotation = npcToPossess.transform.rotation;
            PossessedNPC.SetActive(true);
            SetLayerMask(PossessedNPC.GetComponent<NavMeshAgent>());

            ReleaseSpecialNPC(PossessedNPC.GetComponent<NPCAIStateManager>());

            OnPossessed?.Invoke(PossessedNPC);
            PossessedNPC = npcToPossess.gameObject;
        }
        else
        {
            Vector3 oldpos = transform.position;
            Quaternion oldrot = transform.rotation;
            npcToPossess.gameObject.SetActive(false);
            WarpTo(npcToPossess.transform.position);

            visuals.SetBloodToZero();

            transform.rotation = npcToPossess.transform.rotation;
            PossessedNPC = GlobalGameManager.Instance.npcManager.CreateNPC(oldpos - new Vector3(0, 1, 0), oldrot, visuals.startIndexP1, visuals.startIndexP2, currentRoom);
            SetLayerMask(PossessedNPC.GetComponent<NavMeshAgent>());
            PossessedNPC.GetComponent<NPCAIStateManager>().currentRoom = currentRoom;
            OnPossessed?.Invoke(PossessedNPC);
            PossessedNPC = npcToPossess.gameObject;
        }

        camLookAt.parent = transform;
        camLookAt.transform.position = transform.position;
        NPCAIStateManager possessedNPCAI = PossessedNPC.GetComponent<NPCAIStateManager>();
        visuals.SwapVisuals(possessedNPCAI.type, npcToPossess.Mesh.sharedMesh, npcToPossess.Mesh2.sharedMesh);
        PossessSpecialNPC(possessedNPCAI);
        _type = possessedNPCAI.type;
        _possessOnCD = true;

        // cooldown change
        StartCooldown(_possessCooldown, Abilities.possess);
        //cooldowns.SetCooldown(_possessCooldown, Abilities.possess);

        ClearSuspicion();
        visuals.EmulateNPCBlood(possessedNPCAI.isCorruptedP1 || possessedNPCAI.isCorruptedP2, possessedNPCAI.bloodAmount);
        visuals.AddCurrentBlood();

        if (possessedNPCAI.isCorruptedP1)
        {
            _TagP1.SetActive(true);
            _TagP2.SetActive(false);
        } else if (possessedNPCAI.isCorruptedP2)
        {
            _TagP2.SetActive(true);
            _TagP1.SetActive(false);
        }
        else
        {
            _TagP1.SetActive(false);
            _TagP2.SetActive(false);
        }

        _obstacle.carving = true;
        possessing = false;
        _npcToPossess = null;
    }

    IEnumerator PossessCameraSwing()
    {
        camLookAt.parent = null;
        float startTime = Time.time;
        float endTime = camSwingTime;

        float ratio = 0;

        Vector3 startPos = camLookAt.position;
        Vector3 targetPos = new Vector3(npcToPossess.transform.position.x, camLookAt.position.y, npcToPossess.transform.position.z);

        camLookAt.rotation = Quaternion.Euler(0, freeLookCam.m_XAxis.Value, 0);

        Quaternion startRot = camLookAt.rotation;
        Quaternion targetRot = npcToPossess.transform.rotation;
        Vector3 direction = (targetPos - camLookAt.position).normalized;
        while (ratio < 1)
        {

            ratio = (Time.time - startTime) / (endTime);
            if (ratio > 1) ratio = 1;

            camLookAt.position = Vector3.Lerp(startPos, targetPos, ratio);
            camLookAt.rotation = Quaternion.Lerp(startRot, targetRot, ratio);


            freeLookCam.m_XAxis.Value = camLookAt.eulerAngles.y;
            yield return null;
        }
        PossessCameraMovementComplete();
    }

    private void ReleaseSpecialNPC(NPCAIStateManager npc)
    {
        npc.getCorrupted = false;
        npc.exitStaffOnly = true;
        npc.currentState = npc.states.Released();
        npc.currentState.EnterState();
        switch (npc.type)
        {
            case NPCAIStateManager.NPCType.police:

                break;

            case NPCAIStateManager.NPCType.janitor:

                break;

            case NPCAIStateManager.NPCType.security:
                npc.patrolPartnerScript.partnerPossessed = false;
                npc.patrolPartnerScript.patrolPartner = npc.gameObject;
                if (npc.partnerPossessed)
                {
                    npc.isLeadingPatrol = false;
                }
                else
                {
                    npc.patrolPartnerScript.startPatrol = true;
                }
                npc.startPatrol = true;
                break;

            case NPCAIStateManager.NPCType.guard:
                npc.currentState = npc.states.Guard();
                npc.currentState.EnterState();
                break;
        }
    }

    private void SetLayerMask(NavMeshAgent agent)
    {
        if (isInGate)
        {
            agent.areaMask &= ~(1 << 5);
            agent.areaMask |= (1 << 6);
        }
        else
        {
            agent.areaMask &= ~(1 << 6);
            agent.areaMask |= (1 << 5);
        }
    }

    #endregion

    #region corrupt

    public void OnCorruptAim()
    {
        if (_corruptOnCD || currentState.ReturnStateName().Equals("QTEvent") || gotPunished || qtEvent || possessAim) return;
        _corruptAim = true;
    }

    public void OnCorruptRelease()
    {
        if (qtEvent) return;

        if (targetedPlayer != null)
        {
            GetPunished(wrongTargetTimer);
            //Debug.Log("tried to corrupt player");
            IncreaseSuspicion(_corruptSuspicion);
            //ResetTargetPlayer();
        }

        if (targetedNPC != null && !targetedNPC.getCorrupted)
        {
            if (targetedNPC.influence <= currentInfluence)
            {
                _npcToCorrupt = targetedNPC;
                _corruptAimHit = true;
                _npcToCorrupt.CorruptPossessStart();
            }
        }
        ResetTarget();
        _corruptAim = false;
    }

    public void CorruptionFailed()
    {
        if (!qtEvent) return;
        //Debug.Log("corrupt fail");
        IncreaseSuspicion(_corruptSuspicion); // maybe add "corruptFailedSuspicion"
        _corruptCompleted = true;
        _corruptAimHit = false;
        _npcToCorrupt.CorruptPossessEnd();
        qtEvent = false;

        //_corruptOnCD = true;

        // cooldown changes
        //StartCoroutine(StartCooldown(_corruptCooldown, Abilities.corrupt));
        //cooldowns.SetCooldown(_corruptCooldown, Abilities.corrupt);


        if (_npcToCorrupt.type == NPCAIStateManager.NPCType.boss)
        {
            if (_lastCorruptedTarget.Count > 0)
            {
                _lastCorruptedTarget[_lastCorruptedTarget.Count - 1].UncorruptTargetNPC();
                _lastCorruptedTarget.Remove(_lastCorruptedTarget[_lastCorruptedTarget.Count - 1]);
            }
            else
            {
                ReduceInfluence(10);
            }
        }
        else
        {
            GetPunished(failedQTEventTimer);
        }

        RuntimeManager.PlayOneShot(CorruptFail, transform.position);
    }

    public void CorruptionSucceeded()
    {
        _corruptCompleted = true;
        _corruptAimHit = false;

        if ((_npcToCorrupt.isCorruptedP1 && !isPlayerOne) || (_npcToCorrupt.isCorruptedP2 && isPlayerOne))
        {
            if (isPlayerOne) GlobalGameManager.Instance.Player2.lastCorruptedTarget.Remove(_npcToCorrupt);
            else GlobalGameManager.Instance.Player1.lastCorruptedTarget.Remove(_npcToCorrupt);
        }
        if (_npcToCorrupt.type == NPCAIStateManager.NPCType.target) _lastCorruptedTarget.Add(_npcToCorrupt);


        //Debug.Log("corrupt success sus inc");
        IncreaseSuspicion(_corruptSuspicion);
        _npcToCorrupt.CorruptPossessEnd();
        IncreaseInfluence(npcToCorrupt.GotCorrupted(this, isPlayerOne));
        qtEvent = false;

        //_corruptOnCD = true;

        // cooldown changes
        //StartCoroutine(StartCooldown(_corruptCooldown, Abilities.corrupt));
        //cooldowns.SetCooldown(_corruptCooldown, Abilities.corrupt);



        RuntimeManager.PlayOneShot(CorruptSuccess, transform.position);
    }

    #endregion

    #region attack

    public void OnAttackAim()
    {
        if (_attackOnCD || currentState.ReturnStateName().Equals("QTEvent") || gotPunished || qtEvent) return;
        _targetedNPCs = new List<NPCAIStateManager>();
        targetedPlayer = null;
        _attackAim = true;
        AttackTrigger.SetActive(true);
        _attackParticle.SetActive(true);

        RuntimeManager.PlayOneShot(AttackTarget, transform.position);
    }

    public void OnAttackRelease()
    {
        if (qtEvent || !attackAim) return;

        AttackTrigger.SetActive(false);
        //_attackParticle.SetActive(false);

        if (targetedPlayer != null)
        {
            targetedPlayer.GetPunished(punishDuration);
            int gained = targetedPlayer.ReduceInfluence(atkInfluenceDecrease);
            IncreaseInfluence(gained);
            //Debug.Log("attack success sus inc");
            IncreaseSuspicion(_attackSuspicion);

            RuntimeManager.PlayOneShot(AttackSuccess, transform.position);
        }
        else
        {
            RuntimeManager.PlayOneShot(AttackFail, transform.position);

            //IncreaseSuspicion(_attackSuspicion);
            //GetPunished(wrongTargetTimer);
        }

        // cooldown changes
        StartCooldown(_attackCooldown, Abilities.attack);
        _attackOnCD = true;
        //cooldowns.SetCooldown(_attackCooldown, Abilities.attack);

        ResetTargetPlayer();
        _attackAim = false;
    }

    public void AddTarget(NPCAIStateManager npc)
    {
        targetedNPCs.Add(npc);
        //if (isPlayerOne) npc.Mesh.gameObject.GetComponent<SkinnedMeshRenderer>().material = matTargeted;
        //else npc.Mesh2.gameObject.GetComponent<SkinnedMeshRenderer>().material = matTargeted;

        SetTarget(npc);
    }

    public void RemoveTarget(NPCAIStateManager npc)
    {
        targetedNPCs.Remove(npc);
        //if (isPlayerOne) npc.Mesh.gameObject.GetComponent<SkinnedMeshRenderer>().material = matUntargeted;
        //else npc.Mesh2.gameObject.GetComponent<SkinnedMeshRenderer>().material = matUntargeted;

        ResetTarget(npc);
    }

    public void ClearTargetedNPCs()
    {
        foreach (NPCAIStateManager npc in targetedNPCs)
        {
            //if (isPlayerOne) npc.Mesh.gameObject.GetComponent<SkinnedMeshRenderer>().material = matUntargeted;
            //else npc.Mesh2.gameObject.GetComponent<SkinnedMeshRenderer>().material = matUntargeted;
            ResetTarget(npc);
        }
        _targetedNPCs = new List<NPCAIStateManager>();
    }

    public void EnemyTargeted(PlayerStateManager enemy)
    {
        if (enemy == this) return;
        targetedPlayer = enemy;
        //if (isPlayerOne) enemy.GetComponent<PlayerVisuals>().MeshP1.GetComponent<SkinnedMeshRenderer>().material = matTargeted;
        //else enemy.GetComponent<PlayerVisuals>().MeshP2.GetComponent<SkinnedMeshRenderer>().material = matTargeted;
        SetPlayerTarget();
    }

    public void EnemyLost(PlayerStateManager enemy)
    {
        if (enemy == this) return;
        //if (isPlayerOne) targetedPlayer.GetComponent<PlayerVisuals>().MeshP1.GetComponent<SkinnedMeshRenderer>().material = matUntargeted;
        //else targetedPlayer.GetComponent<PlayerVisuals>().MeshP2.GetComponent<SkinnedMeshRenderer>().material = matUntargeted;
        //targetedPlayer = null;
        ResetTargetPlayer();
    }

    #endregion

    #region suspicion/influence

    private void ReduceSuspicion()
    {
        if (_currentSuspicion > 0) _currentSuspicion -= _suspicionDecSpeed * Time.deltaTime;
        else _currentSuspicion = 0;
    }

    private void ClearSuspicion()
    {
        _currentSuspicion = 0;
    }

    private void IncreaseSuspicion(float suspicionInc)
    {
        _currentSuspicion += suspicionInc;
        if (_currentSuspicion >= _suspicionThreshold)
        {
            OnSuspicion?.Invoke(this);
        }
    }

    public int ReduceInfluence(int influence)
    {
        int diff = influence;
        _currentInfluence -= influence;
        if (currentInfluence < startInfluence)
        {
            diff = currentInfluence + influence - startInfluence;
            _currentInfluence = startInfluence;
        }
        _bossPillar.UpdatedInfluence(currentInfluence);
        OnInfluence?.Invoke();
        visuals.UpdateBloodAmount(_currentInfluence);
        return diff;
    }

    public void IncreaseInfluence(int influence)
    {
        _currentInfluence += influence;
        if (currentInfluence > 100)
        {
            _currentInfluence = 100;
            GlobalGameManager.Instance.npcManager.FormACult(this);
        }
        _bossPillar.UpdatedInfluence(currentInfluence);
        OnInfluence?.Invoke();
        visuals.UpdateBloodAmount(_currentInfluence);
    }

    #endregion

    #region events

    public delegate void OnSuspicionDelegate(PlayerStateManager player);
    public event OnSuspicionDelegate OnSuspicion;

    public delegate void OnInfluenceDelegate();
    public event OnInfluenceDelegate OnInfluence;

    public delegate void OnPossessedDelegate(GameObject releasedNPC);
    public event OnPossessedDelegate OnPossessed;

    #endregion

    #region cooldowns

    private void StartCooldown(float cooldown, Abilities affectedAbility)
    {
        switch (affectedAbility)
        {
            case Abilities.possess:
                StartCoroutine(PossessCooldown(cooldown));
                break;

            case Abilities.corrupt:
                _corruptOnCD = false;
                break;

            case Abilities.attack:
                StartCoroutine(AttackCooldown(cooldown));
                break;
        }
    }

    private IEnumerator PossessCooldown(float cooldown)
    {
        visuals.UpdatePossessGlow(cooldown);

        yield return new WaitForSecondsRealtime(cooldown);

        _possessOnCD = false;
    }


    private IEnumerator AttackCooldown(float cooldown)
    {
        ParticleSystem ps = _attackParticle.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = ps.main;

        main.startColor = new Color(1, 1, 1, 0.01f);
        _attackParticle.transform.DOScale(Vector3.zero, cooldown);

        yield return new WaitForSeconds(cooldown);

        _attackParticle.SetActive(false);
        _attackParticle.transform.localScale = Vector3.one;
        main.startColor = new Color(1, 1, 1, 1);
        attackOnCD = false;
    }

    #endregion

    #region avoiding

    public void GetPunished(float duration)
    {
        if (!gotPunished) StartCoroutine(AvoidanceTimer(duration));
    }

    IEnumerator AvoidanceTimer(float duration)
    {
        visuals.UpdatePunishGlow(duration);
        _gotPunished = true;
        AvoidanceTrigger.SetActive(true);
        yield return new WaitForSecondsRealtime(duration);
        AvoidanceTrigger.SetActive(false);
        _gotPunished = false;
    }

    #endregion

    #region animatons

    private void UpdateWalkingBlend()
    {
        float currSpeed = sprint ? sprintSpeed : speed;
        float velocity = direction.magnitude * currSpeed / (sprintSpeed);
        _anim.SetFloat("velocity", velocity);
    }

    #endregion

    #region victory

    IEnumerator TakeOverScreen()
    {
        yield return new WaitForSeconds(2);
        victoryCam.parent.GetComponent<Animator>().SetTrigger("move");
        _bossPillar.UpdatedInfluence(currentInfluence);

        Camera othercam = cam.GetComponent<Camera>();
        othercam.depth = 0;
        Camera vicCam = victoryCam.GetComponent<Camera>();
        float screenSize;
        float startSize = vicCam.rect.width;
        float targetSize = 1;
        float startTime = Time.time;
        float time = 4;

        float ratio;

        while (startSize < 1)
        {
            ratio = (Time.time - startTime) / time;
            if (ratio > 1) ratio = 1;

            if (Time.frameCount % 2 == 0)
            {
                screenSize = Mathf.Lerp(startSize, targetSize, ratio);

                if (isPlayerOne)
                {
                    Shader.SetGlobalFloat(_splitscreenScaleDiffP1, screenSize);
                    vicCam.rect = new Rect(0, 0, screenSize, 1);
                    othercam.rect = new Rect(0, 0, screenSize, 1);
                }
                else
                {
                    Shader.SetGlobalFloat(_splitscreenScaleDiffP2, 1 - screenSize);
                    vicCam.rect = new Rect(1 - screenSize, 0, screenSize, 1);
                    othercam.rect = new Rect(1 - screenSize, 0, screenSize, 1);
                }
            }
            yield return null;
        }
    }

    #endregion

    public bool OnBackToMenu()
    {
        if (!isPlayerOne) return false;
        if (backToMenu)
        {
            GlobalGameManager.Instance.LoadSceneIn(0, 0);
            return true;
        }
        else
        {
            backToMenu = true;
            ConfirmToQuitText.SetActive(true);
            ConfirmToQuitText.GetComponent<TMP_Text>().color = Color.white;
            ConfirmToQuitText.GetComponent<TMP_Text>().DOColor(new Color(1, 1, 1, 0), 1).SetLoops(3, LoopType.Yoyo);
            StartCoroutine(BackToMenuTimer());
            return false;
        }
    }

    IEnumerator BackToMenuTimer()
    {
        yield return new WaitForSeconds(3);
        ConfirmToQuitText.SetActive(false);
        backToMenu = false;
    }
}
