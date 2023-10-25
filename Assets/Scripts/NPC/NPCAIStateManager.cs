using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Transactions;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class NPCAIStateManager : MonoBehaviour
{
    [TypeDropDown(typeof(NPCBaseState))]
    [SerializeField]
    private string startStateType;
    private NPCBaseState _currentState;
    public NPCBaseState currentState { get { return _currentState; } set { _currentState = value; } }
    private NPCStateFactory _states;
    public NPCStateFactory states { get { return _states; } }

    public enum NPCType
    {
        normal,
        janitor,
        police,
        security,
        guard,
        entertainer,
        target,
        boss
    }

    [SerializeField] private NPCType _type;
    public NPCType type { get { return _type; } }

    public SkinnedMeshRenderer Mesh;
    public SkinnedMeshRenderer Mesh2;

    private SkinnedMeshRenderer _MeshLOD;
    private SkinnedMeshRenderer _Mesh2LOD;

    private NavMeshAgent _agent;
    public NavMeshAgent agent { get { return _agent; } }


    [SerializeField] private int _agentInterval;
    public int agentInterval { get { return _agentInterval; } }

    private NavMeshObstacle _obstacle;
    public NavMeshObstacle obstacle { get { return _obstacle; } }

    [SerializeField] private float _speedMin;
    public float speedMin { get { return _speedMin; } }

    [SerializeField] private float _speedMax;
    public float speedMax { get { return _speedMax; } }

    [SerializeField] private float _speed;
    public float speed { get { return _speed; } }
    [SerializeField] private float _sprintSpeed;
    public float sprintSpeed { get { return _sprintSpeed; } }

    [SerializeField] private Animator _anim;
    public Animator anim { get { return _anim; } }

    private Room _currentRoom;
    private Room _targetRoom;
    public Room currentRoom { get { return _currentRoom; } set { _currentRoom = value; } }
    public Room targetRoom { get { return _targetRoom; } set { _targetRoom = value; } }

    #region grouping

    [SerializeField] private GameObject _GroupPrefab;
    private GroupManager _groupManager;
    private bool _followGroup = false;
    public GameObject GroupPrefab { get { return _GroupPrefab; } }
    public bool followGroup { get { return _followGroup; } set { _followGroup = value; } }
    public GroupManager groupManager { get { return _groupManager; } set { _groupManager = value; } }

    #endregion

    private float _stopProximity = 0.5f;
    public float stopProximity { get { return _stopProximity; } }

    private NavMeshPath _nextPath;
    public NavMeshPath nextPath { get { return _nextPath; } set { _nextPath = value; } }

    private bool _startGeneration = true;
    public bool startGeneration { get { return _startGeneration; } set { _startGeneration = value; } }

    private bool _passedGate;
    public bool passedGate { get { return _passedGate; } set { _passedGate = value; } }

    private PlayerStateManager _winnerToFollow;
    public PlayerStateManager winnerToFollow { get { return _winnerToFollow; } set { _winnerToFollow = value; } }

    private Vector3 _oldWinnerPos;
    public Vector3 oldWinnerPos { get { return _oldWinnerPos; } set { _oldWinnerPos = value; } }

    private Vector3 _posBeforeFollow;
    public Vector3 posBeforeFollow { get { return _posBeforeFollow; } set { _posBeforeFollow = value; } }

    private int _failSafeInGate = 0;
    public int failSafeIngate { get { return _failSafeInGate; } set { _failSafeInGate = value; } }

    #region influence

    [Header("Influence")]
    [SerializeField] private int _npcInfluence;
    [SerializeField] private int _specialNpcInfluence;
    [SerializeField] private int _targetNpcInfluence;

    #endregion

    #region brain

    public enum NPCAction
    {
        walk,
        walkToRoom,
        joinGroup,
        leaveGroup,
        moveGroup,
        formMob,
        leadMob,
        idle,
        NONE
    }

    [Header("Brain")]
    [SerializeField] private NPCActionData _actionData;
    public NPCActionData actionData { get { return _actionData; } }

    public Coroutine nextActionCoroutine;

    private NPCAction _selectedAction;
    public NPCAction selectedAction { get { return _selectedAction; } set { _selectedAction = value; } }

    #endregion

    #region janitor

    private bool _exitStaffOnly = false;
    public bool exitStaffOnly { get { return _exitStaffOnly; } set { _exitStaffOnly = value; } }

    #endregion

    #region police

    [Header("Police")]
    [SerializeField] private float _policeRange;
    public float policeRange { get { return _policeRange; } }
    [SerializeField] private int influencePunish;
    [SerializeField] private LayerMask policeLayerMask;
    private bool _punishPlayer = false;
    public bool punishPlayer { get { return _punishPlayer; } set { _punishPlayer = value; } }
    private GameObject _playerToPunish;
    public GameObject playerToPunish { get { return _playerToPunish; } }
    [SerializeField] private float _policeStopProximity;
    public float policeStopProximity { get { return _policeStopProximity; } }

    private PlayerStateManager _playerToFollow;
    public PlayerStateManager playerToFollow { get { return _playerToFollow; } }
    private bool _moveToPlayer = false;
    public bool moveToPlayer { get { return _moveToPlayer; } set { _moveToPlayer = value; } }

    private Room _playerTargetRoom;
    public Room playerTargetRoom { get { return _playerTargetRoom; } set { _playerTargetRoom = value; } }

    #endregion

    #region security

    [Header("Security")]
    [SerializeField] private float _securityStopProximity;
    public float securityStopProximity { get { return _securityStopProximity; } }
    private GameObject _patrolPartner;
    public GameObject patrolPartner { get { return _patrolPartner; } set 
        { 
            _patrolPartner = value; 
            if(_patrolPartner.CompareTag("NPC")) _patrolPartnerScript = value.GetComponent<NPCAIStateManager>(); 
        } 
    }

    private NPCAIStateManager _patrolPartnerScript;
    public NPCAIStateManager patrolPartnerScript { get { return _patrolPartnerScript; } set { _patrolPartnerScript = value; } }

    private bool _isLeadingPatrol = false;
    public bool isLeadingPatrol { get { return _isLeadingPatrol; } set { _isLeadingPatrol = value; } }
    private Vector3 _currentPatrolGoal;
    public Vector3 currentPatrolGoal { get { return _currentPatrolGoal; } 
        set 
        { 
            _currentPatrolGoal = value;
            /*if (!_isLeadingPatrol)
            {
                //_currentPatrolGoal += Vector3.one;
                //agent.ResetPath();
            } else
            {
                //agent.SetDestination(_currentPatrolGoal);
            }*/
            //agent.SetDestination(_currentPatrolGoal);
        } 
    }
    private List<Room> _patrolRooms = new List<Room>();
    public List<Room> patrolRooms { get { return _patrolRooms; } set { _patrolRooms = value; } }
    private int _currentPatrolPoint = 0;
    public int currentPatrolPoint { get { return _currentPatrolPoint; } }
    private bool _startPatrol = false;
    public bool startPatrol { get { return _startPatrol; } set { _startPatrol = value; } }
    private bool _partnerPossessed = false;
    public bool partnerPossessed { get { return _partnerPossessed; } set { _partnerPossessed = value; } }

    private float _securitySpeedInc = 0.4f;
    public float securitySpeedInc { get { return _securitySpeedInc; } }

    private bool _waitingForPartner;
    public bool waitingForPartner { get { return _waitingForPartner; } set { _waitingForPartner = value; } }

    private Coroutine _pathingCor;
    public Coroutine pathingCor { get { return _pathingCor; } set { _pathingCor = value; } }

    #endregion

    #region guard

    [SerializeField] private Transform _idlePos;
    public Transform idlePos { get { return _idlePos; } }

    [SerializeField] private Transform _blockingPos;
    public Transform blockingPos { get { return _blockingPos; } }

    private Vector3 _nextGuardPos;
    public Vector3 nextGuardPos { get { return _nextGuardPos; } set { _nextGuardPos = value; } }

    private bool _guardBlocking = false;
    public bool guardBlocking { get { return _guardBlocking; } set { _guardBlocking = value; } }

    #endregion

    #region entertainer

    [Header("Entertainer")]
    [SerializeField] private LayerMask _entertainerMask;
    public LayerMask entertainerMask { get { return _entertainerMask; } }
    [SerializeField] private float _entertainerStopProximity;
    public float entertainerStopProximity { get { return _entertainerStopProximity; } }
    private bool _playingMusic = false;
    public bool playingMusic { get { return _playingMusic; } set { _playingMusic = value; } }

    private List<NPCAIStateManager> _npcsInMusicRange = new List<NPCAIStateManager>();
    public List<NPCAIStateManager> npcsInMusicRange { get { return _npcsInMusicRange; } }

    private List<PlayerStateManager> _playersInMusicRange = new List<PlayerStateManager>();
    public List<PlayerStateManager> playersInMusicRange { get { return _playersInMusicRange; } }

    [SerializeField] private Animator _entertainerAnimP2;
    public Animator entertainerAnimP2 { get { return _entertainerAnimP2; } }

    [SerializeField] private GameObject _corruptEntertainerP1Decal;
    [SerializeField] private GameObject _corruptEntertainerP2Decal;

    [SerializeField] private EventReference _pianoMusic;
    public EventReference pianoMusic { get { return _pianoMusic; } }
    [SerializeField] private EventReference _drumsMusic;
    public EventReference drumsMusic { get { return _drumsMusic; } }

    private EventInstance _entertainerMusicP1;
    public EventInstance entertainerMusicP1 { get { return _entertainerMusicP1; } set { _entertainerMusicP1 = value; } }

    private EventInstance _entertainerMusicP2;
    public EventInstance entertainerMusicP2 { get { return _entertainerMusicP2; } set { _entertainerMusicP2 = value; } }

    #endregion

    #region corrupted

    [Header("Corruption")]
    [SerializeField] private int _corruptDifficulty = 5;
    public int corruptDifficulty { get { return _corruptDifficulty; } }
    [SerializeField] private int _incCorruptDifficulty = 3;
    public int incCorruptDifficulty { get { return _incCorruptDifficulty; } }

    private bool _isCorruptedP1 = false;
    private bool _isCorruptedP2 = false;
    public bool isCorruptedP1 { get { return _isCorruptedP1; } set { _isCorruptedP1 = value; } }
    public bool isCorruptedP2 { get { return _isCorruptedP2; } set { _isCorruptedP2 = value; } }
    private PlayerStateManager _corruptedByPlayer;

    private int _influence = 1;
    public int influence { get { return _influence; } }

    private bool _getCorrupted = false;
    public bool getCorrupted { get { return _getCorrupted; } set { _getCorrupted = value; } }

    private NPCBaseState _backupState;
    public NPCBaseState backupState { get { return _backupState; } set { _backupState = value; } }

    private NavMeshPath _backupPath;
    public NavMeshPath backupPath { get { return _backupPath; } set { _backupPath = value; } }

    [SerializeField] private GameObject _corruptP1Decal;
    [SerializeField] private GameObject _corruptP2Decal;

    [SerializeField] private float _bloodAmount;
    public float bloodAmount { get { return _bloodAmount; } }

    #endregion

    #region AvoidPlayer

    private NPCBaseState _avoidPlayerBackupState;
    public NPCBaseState avoidPlayerBackupState { get { return _avoidPlayerBackupState; } set { _avoidPlayerBackupState = value; } }

    private bool _leftAvoidPlayerArea = false;
    public bool leftAvoidPlayerArea { get { return _leftAvoidPlayerArea; } set { _leftAvoidPlayerArea = value; } }

    private List<PlayerStateManager> _PlayersToAvoid = new List<PlayerStateManager>();
    public List<PlayerStateManager> PlayersToAvoid { get { return _PlayersToAvoid; } set { _PlayersToAvoid = value; } }

    #endregion

    #region animation

    [SerializeField] private int _animInterval;
    public int animInterval { get { return _animInterval; } }

    #endregion

    #region randomMesh

    private int _startIndexP1;
    public int startIndexP1 { get { return _startIndexP1; } set { _startIndexP1 = value; } }

    private int _startIndexP2;
    public int startIndexP2 { get { return _startIndexP2; } set { _startIndexP2 = value; } }

    #endregion

    #region Tutorial

    [SerializeField] private bool _inTutorial;
    public bool inTutorial { get { return _inTutorial; } }

    [SerializeField] private Transform _targetPosition;
    public Transform targetPosition { get { return _targetPosition; } set { _targetPosition = value; } }

    #endregion

    // Debug

    [SerializeField] private NPCBaseState.NPCStates currentStateName;

    private void Awake()
    {
        _states = new NPCStateFactory(this);
        _agent = GetComponent<NavMeshAgent>();
        Type startType = Type.GetType(startStateType);
        _currentState = (NPCBaseState)Activator.CreateInstance(startType, this, _states);
        _currentState.EnterState();

        if (type == NPCType.guard) _obstacle = GetComponent<NavMeshObstacle>();
        if (type != NPCType.janitor) agent.areaMask &= ~(1 << 4);

        if (type == NPCType.boss) _influence = 100;
        else if (type == NPCType.target) _influence = _targetNpcInfluence;
        else if (type == NPCType.normal) _influence = _npcInfluence;
        else _influence = _specialNpcInfluence;

        _MeshLOD = Mesh.GetComponent<SelectRandomMesh>().LOD1Variant;
        _Mesh2LOD = Mesh2.GetComponent<SelectRandomMesh>().LOD1Variant;
    }

    private void Start()
    {
        _animInterval = Random.Range(7, 12);
        if (type == NPCType.security && !inTutorial) AddSecurityPath();

        if(type == NPCType.entertainer)
        {
            entertainerMusicP1 = RuntimeManager.CreateInstance(pianoMusic);
            entertainerMusicP2 = RuntimeManager.CreateInstance(drumsMusic);
            entertainerMusicP1.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
            entertainerMusicP2.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
        }
    }

    void Update()
    {
        _currentState.UpdateStates();
        if (Time.frameCount % animInterval == 0 && type != NPCType.boss && type != NPCType.entertainer)
        {
            UpdateWalkingBlend();
        }

        // Debug
        currentStateName = currentState.ReturnStateName();
    }

    private void FixedUpdate()
    {
        _currentState.FixedUpdateStates();
    }

    private void OnEnable()
    {
        if(type != NPCType.boss && type != NPCType.guard && type != NPCType.entertainer) agent.enabled = true;
        currentState.EnterState();
    }

    private void OnDisable()
    {
        entertainerMusicP1.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    private void OnTriggerEnter(Collider other)
    {
        _currentState.OnTriggerEnter(other);
        if (other.CompareTag("JanitorDoorBack") && type != NPCType.janitor)
        {
            other.GetComponentInParent<JanitorDoor>().OpenFromBack();
        }
        if (other.CompareTag("SecurityDoorBack") && (type == NPCType.security || type == NPCType.guard))
        {
            other.GetComponentInParent<SecurityDoor>().OpenFromBack();
        }
        if (other.CompareTag("SecurityDoorFront") && (type == NPCType.security || type == NPCType.guard))
        {
            other.GetComponentInParent<SecurityDoor>().OpenFromFront();
        }
        if (other.CompareTag("AvoidPlayer"))
        {
            if (type == NPCType.boss) return;
            if (type == NPCType.guard) return;
            if (type == NPCType.security) return;
            if (type == NPCType.entertainer) return;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, (other.transform.position - transform.position).normalized, out hit, Mathf.Infinity, policeLayerMask))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    PlayersToAvoid.Add(hit.collider.GetComponent<PlayerStateManager>());
                    _avoidPlayerBackupState = _currentState;
                    _currentState.ExitState();
                    _currentState = states.AvoidPlayer();
                    _currentState.EnterState();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _currentState.OnTriggerExit(other);
        if (other.CompareTag("AvoidPlayer"))
        {
            if (type == NPCType.guard || type == NPCType.boss || type == NPCType.entertainer) return;

            if (currentState.ReturnStateName() == NPCBaseState.NPCStates.AvoidPlayer)
            {
                PlayersToAvoid.Remove(other.GetComponentInParent<PlayerStateManager>());
            }
        }
        if (other.CompareTag("GateTrigger") && type != NPCType.security)
        {
            passedGate = true;
            agent.areaMask ^= (1 << 5);
            agent.areaMask ^= (1 << 6);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        _currentState.OnCollisionEnter(collision);
    }

    public void StartActionCooldown()
    {
        NextAction nextAction = new NextAction();
        if (actionData != null)
        {
            nextAction = actionData.GetNextAction(currentState.ReturnStateName(), isCorruptedP1 || isCorruptedP2);
        }
        if (nextAction != null) nextActionCoroutine = StartCoroutine(ActionCooldown(nextAction.actionDelay, nextAction.nextAction));
    }

    public GroupManager FormGroupAt(Vector3 groupPos)
    {
        GameObject group;
        group = Instantiate(GroupPrefab, groupPos, Quaternion.identity, GlobalGameManager.Instance.npcManager.GroupParent.transform);
        return group.GetComponent<GroupManager>();
    }

    IEnumerator ActionCooldown(float delay, NPCAction action)
    {
        yield return new WaitForSecondsRealtime(delay);

        selectedAction = action;

        yield return null;
    }

    public void RotateTowardsGroupCenter()
    {
        var totalX = 0f;
        var totalZ = 0f;
        foreach(NPCAIStateManager npc in groupManager.npcs)
        {
            totalX += npc.transform.position.x;
            totalZ += npc.transform.position.z;
        }
        var centerX = totalX / groupManager.npcs.Count;
        var centerZ = totalZ / groupManager.npcs.Count;

        StartCoroutine(RotateTowardsLookAt(new Vector3(centerX, transform.position.y, centerZ)));
    }



    IEnumerator RotateTowardsLookAt(Vector3 lookAt)
    {
        Vector3 relativePos = lookAt - transform.position;
        
        if (relativePos.magnitude > 0.1f)
        {
            Quaternion startRot = transform.rotation;
            Quaternion targetRot = Quaternion.LookRotation(relativePos, Vector3.up);

            float ratio = 0;
            float startTime = Time.time;
            float endTime = 0.3f;

            while (ratio < 1)
            {
                ratio = (Time.time - startTime) / (endTime);
                if (ratio > 1) ratio = 1;

                transform.rotation = Quaternion.Lerp(startRot, targetRot, ratio);
                yield return null;
            }
        } 
    }

    #region getCorrupted

    public int GotCorrupted(PlayerStateManager player, bool isPlayerOne)
    {
        if (_corruptedByPlayer == null)
        {
            _corruptedByPlayer = player;
            if (isPlayerOne) isCorruptedP1 = true;
            else isCorruptedP2 = true;

            StartCoroutine(ActivateBloodyHands());
        }
        else if (player == _corruptedByPlayer)
        {
            return 0;
        }
        else
        {
            _corruptedByPlayer.ReduceInfluence(influence);
            _corruptedByPlayer = player;
            isCorruptedP1 = !isCorruptedP1;
            isCorruptedP2 = !isCorruptedP2;
        }

        if (isCorruptedP1)
        {
            _corruptP1Decal.SetActive(true);
            _corruptP2Decal.SetActive(false);
            if(type == NPCType.entertainer)
            {
                _corruptEntertainerP1Decal.SetActive(true);
                _corruptEntertainerP2Decal.SetActive(false);
            }
        }
        else
        {
            _corruptP1Decal.SetActive(false);
            _corruptP2Decal.SetActive(true);
            if (type == NPCType.entertainer)
            {
                _corruptEntertainerP1Decal.SetActive(false);
                _corruptEntertainerP2Decal.SetActive(true);
            }
        }
        anim.SetTrigger("corrupted");
        CorruptSpecialNPC();

        return influence;
    }

    private void CorruptSpecialNPC()
    {
        switch (type)
        {
            case NPCType.security:
                if (partnerPossessed) return;
                if (!isLeadingPatrol)
                {
                    //agent.speed = speed;
                    //patrolPartnerScript.agent.speed = speed + securitySpeedInc;
                    patrolPartnerScript.isLeadingPatrol = false;
                    isLeadingPatrol = true;
                    //patrolPartnerScript.currentState.EnterState();
                    //currentState.EnterState();
                }
                if (isCorruptedP1 && GlobalGameManager.Instance.Player2 != null) _playerToFollow = GlobalGameManager.Instance.Player2;
                if (isCorruptedP2 && GlobalGameManager.Instance.Player1 != null) _playerToFollow = GlobalGameManager.Instance.Player1;
                if (playerToFollow != null)
                {
                    startPatrol = false;
                    patrolPartnerScript.startPatrol = false;

                    patrolPartnerScript.moveToPlayer = true;
                    moveToPlayer = true;
                    playerTargetRoom = playerToFollow.currentRoom;
                } 
                break;

            case NPCType.guard:

                if (guardBlocking)
                {
                    obstacle.enabled = false;
                    nextGuardPos = idlePos.position;
                }

                break;

            case NPCType.entertainer:

                /*if(playingMusic)
                {
                    currentState = states.EntertainerPlaying();
                    currentState.ExitState();
                    currentState.EnterState();
                } else*/ if (playersInMusicRange.Count >= 2)
                {
                    playingMusic = true;
                }
                else
                {
                    playingMusic = false;
                }

                break;
        }
    }

    public void CorruptPossessStart()
    {
        getCorrupted = true;
        backupState = currentState;
        currentState = states.GetCorrupted();
        if (type == NPCType.guard || type == NPCType.entertainer || type == NPCType.boss) return;
        agent.ResetPath();
        if (type != NPCType.security)
        {
            if (agent.path != null) backupPath = agent.path;
            else backupPath = null;
        }
        if (type == NPCType.security)
        {
            if (!partnerPossessed) patrolPartnerScript.CorruptPossessStartPartner();
        }
    }

    public void CorruptPossessStartPartner()
    {
        getCorrupted = true;
        backupState = currentState;
        currentState = states.GetCorrupted();
        agent.ResetPath();
        if (type != NPCType.security)
        {
            if (agent.path != null) backupPath = agent.path;
            else backupPath = null;
        }
    }

    public void CorruptPossessEnd()
    {
        getCorrupted = false;
        currentState = backupState;
        currentState.EnterState();
        if (type == NPCType.guard) return;
        if (type != NPCType.security && type != NPCType.guard)
        {
            if (backupPath != null) agent.path = backupPath;
        }


        /*if (type == NPCType.security && isLeadingPatrol)
        {
            if (agent.path != null && agent.isActiveAndEnabled) agent.SetDestination(currentPatrolGoal);
        }*/
        if (type == NPCType.security)
        {
            if (!partnerPossessed) patrolPartnerScript.CorruptPossessEndPartner();
        }
    }

    public void CorruptPossessEndPartner()
    {
        getCorrupted = false;
        currentState = backupState;
        currentState.EnterState();
        if (type != NPCType.security)
        {
            if (backupPath != null) agent.path = backupPath;
        }
        
        
        /*if (type == NPCType.security)
        {
            if (agent.path != null) agent.SetDestination(currentPatrolGoal);
        }*/
    }

    public void UncorruptTargetNPC()
    {
        _corruptedByPlayer.ReduceInfluence(influence);
        _corruptedByPlayer = null;
        isCorruptedP1 = false;
        isCorruptedP2 = false;
        StartCoroutine(RemoveBloodyHands());
    }

    IEnumerator ActivateBloodyHands()
    {
        float ratio = 0;
        float startTime = Time.time;
        float endTime = 2;
        float startAmount = 0.25f;
        float finalAmount = _bloodAmount;

        while (ratio < 1)
        {
            ratio = (Time.time - startTime) / endTime;
            if (ratio > 1) ratio = 1;

            float _currentBloodAmount = Mathf.Lerp(startAmount, finalAmount, ratio);

            Mesh.materials[1].SetFloat("_BloodAmount", _currentBloodAmount);
            Mesh2.materials[1].SetFloat("_BloodAmount", _currentBloodAmount);
            _MeshLOD.materials[1].SetFloat("_BloodAmount", _currentBloodAmount);
            _Mesh2LOD.materials[1].SetFloat("_BloodAmount", _currentBloodAmount);

            yield return null;
        }
    }

    IEnumerator RemoveBloodyHands()
    {
        float ratio = 0;
        float startTime = Time.time;
        float endTime = 2;
        float startAmount = _bloodAmount;
        float finalAmount = 0;

        while (ratio < 1)
        {
            ratio = (Time.time - startTime) / endTime;
            if (ratio > 1) ratio = 1;

            float _currentBloodAmount = Mathf.Lerp(startAmount, finalAmount, ratio);

            Mesh.materials[1].SetFloat("_BloodAmount", _currentBloodAmount);
            Mesh2.materials[1].SetFloat("_BloodAmount", _currentBloodAmount);
            _MeshLOD.materials[1].SetFloat("_BloodAmount", _currentBloodAmount);
            _Mesh2LOD.materials[1].SetFloat("_BloodAmount", _currentBloodAmount);

            yield return null;
        }
    }

    #endregion

    #region janitor

    public void JanitorAreaLeft()
    {
        if (type != NPCType.janitor && exitStaffOnly)
        {
            NavMeshHit hit;
            agent.SamplePathPosition(1, 1000, out hit);
            if (!(hit.mask == 1 << 4))
            {
                agent.areaMask &= ~(1 << 4);
                exitStaffOnly = false;
            }
        }
    }

    #endregion

    #region police

    private void OnSuspicionHandler(PlayerStateManager player)
    {
        if (!isLeadingPatrol) return;
        //if (currentState.ReturnStateName() != NPCBaseState.NPCStates.SecurityPatrol) return;
        if ((isCorruptedP1 && player.isPlayerOne) || (isCorruptedP2 && !player.isPlayerOne)) return;


        RaycastHit hit;

        if (Physics.Raycast(transform.position, (player.transform.position - transform.position).normalized, out hit, _policeRange, policeLayerMask))
        {
            if (hit.collider.CompareTag("Player"))
            {
                punishPlayer = true;
                _playerToPunish = player.gameObject;
            }
        }
    }

    private void OnPunishTargetSwitchHandler(GameObject releasedNPC)
    {
        if (!isLeadingPatrol) return;
        if (punishPlayer)
        {
            _playerToPunish = releasedNPC;
        }
    }

    public void PunishPlayer(GameObject player)
    {
        if (!isLeadingPatrol) return;
        PlayerStateManager playerScr = player.GetComponent<PlayerStateManager>();
        if (playerScr != null)
        {
            playerScr.ReduceInfluence(influencePunish);
            playerScr.currentSuspicion = 0;
        }
    }

    public void AddPoliceWatch()
    {
        if (GlobalGameManager.Instance.Player1 != null)
        {
            GlobalGameManager.Instance.Player1.OnSuspicion += OnSuspicionHandler;
            GlobalGameManager.Instance.Player1.OnPossessed += OnPunishTargetSwitchHandler;
        }
        if (GlobalGameManager.Instance.Player2 != null)
        {
            GlobalGameManager.Instance.Player2.OnSuspicion += OnSuspicionHandler;
            GlobalGameManager.Instance.Player2.OnPossessed += OnPunishTargetSwitchHandler;
        }
    }

    #endregion

    #region security

    private void AddSecurityPath()
    {
        foreach (Room room in GlobalGameManager.Instance.npcManager.rooms)
        {
            patrolRooms.Add(room);
        }
        ListShuffle.Shuffle(_patrolRooms);
        if (isLeadingPatrol)
        {
            Vector3 dest = GetNextPatrolPoint();
            if (pathingCor != null) StopCoroutine(pathingCor);
            pathingCor = StartCoroutine(CalculatePatrolPath(dest, 0));
        }
    }

    public Vector3 GetNextPatrolPoint()
    {
        if (isLeadingPatrol)
        {
            Vector3 patrolPoint = NPCManager.GetRandomPoint(patrolRooms[currentPatrolPoint]);
            _currentPatrolPoint++;
            if (_currentPatrolPoint >= patrolRooms.Count) _currentPatrolPoint = 0;
            currentPatrolGoal = patrolPoint;
            patrolPartnerScript.currentPatrolGoal = patrolPoint;
            return patrolPoint;
        } /*else if (!partnerPossessed)
        {
            return patrolPartner.GetComponent<NPCAIStateManager>().currentPatrolGoal;
        }*/
        /*else
        {
            if (partnerPossessed)
            {
                if (patrolPartner.GetComponent<PlayerStateManager>().sprint) agent.speed = sprintSpeed;
                else agent.speed = speed;
            } 
            return patrolPartner.transform.position;
        }*/
        return Vector3.zero;
    }

    public Vector3 SetNextPatrolPoint(Room nextPatrolRoom)
    {
        _currentPatrolPoint = patrolRooms.IndexOf(nextPatrolRoom);
        Vector3 patrolPoint = NPCManager.GetRandomPoint(patrolRooms[currentPatrolPoint]);
        _currentPatrolPoint++;
        if (_currentPatrolPoint >= patrolRooms.Count) _currentPatrolPoint = 0;
        currentPatrolGoal = patrolPoint;
        patrolPartnerScript.currentPatrolGoal = patrolPoint;
        return patrolPoint;
    }

    public void GoalReached()
    {
        if (inTutorial) return;
        if (currentState.ReturnStateName() == NPCBaseState.NPCStates.GoToBoss) return;
        //if (currentState.ReturnStateName() == NPCBaseState.NPCStates.GetCorrupted) return;
        //startPatrol = true;
        //patrolPartnerScript.startPatrol = true;

        waitingForPartner = true;
        if (patrolPartnerScript.waitingForPartner)
        {
            if (isLeadingPatrol)
            {
                Vector3 dest = GetNextPatrolPoint();
                if (pathingCor != null) StopCoroutine(pathingCor);
                pathingCor = StartCoroutine(CalculatePatrolPath(dest, 7));
            }
            else patrolPartnerScript.GoalReached();
        }
    }

    public void CorruptSecuityMove()
    {
        Debug.Log("clear both coroutines");
        agent.ResetPath();
        if (pathingCor != null) StopCoroutine(pathingCor);
        if (isLeadingPatrol)
        {
            Debug.Log("leader sets target");
            Vector3 dest = SetNextPatrolPoint(playerTargetRoom);
            pathingCor = StartCoroutine(CalculatePatrolPath(dest, 0));
        }
    }

    IEnumerator CalculatePatrolPath(Vector3 dest, float delay)
    {
        //Debug.Log("calculate path");
        NavMeshPath path = new NavMeshPath();
        NavMeshPath partnerPath = new NavMeshPath();
        
        agent.CalculatePath(dest, path);
        patrolPartnerScript.agent.CalculatePath(currentPatrolGoal, partnerPath);

        yield return new WaitForSeconds(2 + delay);

        /*while(path.status != NavMeshPathStatus.PathComplete) // || partnerPath.status != NavMeshPathStatus.PathComplete)
        {
            yield return null;
        }*/

        waitingForPartner = false;
        patrolPartnerScript.waitingForPartner = false;

        //Debug.Log("set path");

        agent.SetPath(path);
        patrolPartnerScript.agent.SetPath(partnerPath);

        //Debug.Log(agent.remainingDistance);
        //Debug.Log(patrolPartnerScript.agent.remainingDistance);

        startPatrol = true;
        patrolPartnerScript.startPatrol = true;
        //Debug.Log("resuming");
    }

    public void SyncUp()
    {
        float difference = Vector3.Distance(agent.destination, transform.position) - Vector3.Distance(agent.destination, patrolPartner.transform.position);
        if (difference > 0) // partner closer to destination
        {
            agent.speed = speed + securitySpeedInc * difference;
            patrolPartnerScript.agent.speed = speed;
        }
        else
        {
            agent.speed = speed;
            patrolPartnerScript.agent.speed = speed + securitySpeedInc * Mathf.Abs(difference);
        }

        /*
        if (Mathf.Abs(Vector3.Distance(agent.destination, transform.position) - Vector3.Distance(agent.destination, patrolPartner.transform.position)) > 2)
        {
            if(Vector3.Distance(agent.destination, transform.position) > Vector3.Distance(agent.destination, patrolPartner.transform.position)) // partner closer to destination
            {
                agent.speed = speed + 1.5f * securitySpeedInc;
                patrolPartnerScript.agent.speed = speed;
            }
            else
            {
                agent.speed = speed;
                patrolPartnerScript.agent.speed = speed + 1.5f * securitySpeedInc;
            }
        }
        else
        {
            agent.speed = speed;
            patrolPartnerScript.agent.speed = speed;
        }*/
    }

    public void StopSecurityCoroutine()
    {
        if (pathingCor != null) StopCoroutine(pathingCor);
    }

    #endregion

    #region guard

    public void GuardExit(PlayerStateManager player)
    {
        if ((isCorruptedP1 && !player.isPlayerOne) || (isCorruptedP2 && player.isPlayerOne))
        {
            obstacle.enabled = false;
            nextGuardPos = blockingPos.position;
            guardBlocking = true;
            if (agent.isActiveAndEnabled)
            {
                agent.ResetPath();
                agent.SetDestination(nextGuardPos);
            }
        }
    }

    public void LeaveExit(PlayerStateManager player)
    {
        if ((isCorruptedP1 && !player.isPlayerOne) || (isCorruptedP2 && player.isPlayerOne))
        {
            obstacle.enabled = false;
            nextGuardPos = idlePos.position;
            guardBlocking = false;
            if (agent.isActiveAndEnabled)
            {
                agent.ResetPath();
                agent.SetDestination(nextGuardPos);
            }
        }
    }

    public void RotateTowardsAngle(float yRotation)
    {
        if(yRotation != transform.localEulerAngles.y) StartCoroutine(RotateTowardsYPos(yRotation));
    }

    IEnumerator RotateTowardsYPos(float yRotation)
    {
        float speed = (yRotation > transform.localEulerAngles.y) ? 150 : -150;
        while(Vector3.Distance(transform.localEulerAngles, new Vector3(transform.localEulerAngles.x, yRotation, transform.localEulerAngles.z)) > 5f)
        {
            transform.Rotate(Vector3.up * speed * Time.deltaTime, Space.Self);
            yield return null;
        }
    }

    #endregion

    #region entertainer

    public void ListenToEntertainer(NPCAIStateManager entertainer)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, (entertainer.transform.position - transform.position).normalized, out hit, Mathf.Infinity, entertainerMask))
        {
            if (hit.collider.CompareTag("EntertainerCircle"))
            {
                Vector3 listeningPos = hit.point;
                agent.SetDestination(listeningPos);
                currentState = states.ListenToEntertainer();
                currentState.EnterState();
                if (nextActionCoroutine != null) StopCoroutine(nextActionCoroutine);
                selectedAction = NPCAction.NONE;
            }
        }
    }

    #endregion

    #region boss

    public void ActivateBoss()
    {
        GetComponentInChildren<CapsuleCollider>().enabled = true;
    }

    #endregion

    #region animations

    private void UpdateWalkingBlend()
    {
        float velocity = agent.velocity.magnitude / sprintSpeed;
        anim.SetFloat("velocity", velocity);
    }

    #endregion

    #region tutorial

    [ContextMenu("DoTutorial")]
    public void GoToTutorialState()
    {
        currentState = states.Tutorial();
        currentState.EnterState();
    }

    public void GuardExit()
    {
        //obstacle.enabled = false;
        nextGuardPos = blockingPos.position;
        guardBlocking = true;
        agent.ResetPath();
        agent.SetDestination(nextGuardPos);
    }

    public void LeaveExit()
    {
       //obstacle.enabled = false;
        nextGuardPos = idlePos.position;
        guardBlocking = false;
        agent.ResetPath();
        agent.SetDestination(nextGuardPos);
    }

    #endregion

    #region ending

    public void FollowPlayer(PlayerStateManager player)
    {
        if (!isActiveAndEnabled) return;
        if (currentState.ReturnStateName() == NPCBaseState.NPCStates.FollowPlayer) return;
        if((player.isPlayerOne && isCorruptedP1) || (!player.isPlayerOne && isCorruptedP2))
        {
            posBeforeFollow = transform.position;
            winnerToFollow = player;
            currentState = states.FollowPlayer();
            currentState.EnterState();
        }
    }

    public Vector3 GetFollowPlayerPosition()
    {
        Vector3 direction = (winnerToFollow.transform.position - transform.position).normalized;
        float distance = Vector3.Distance(winnerToFollow.transform.position, transform.position);


        return (direction * (distance - Random.Range(1.7f, 3.2f)) + transform.position);
    }

    #endregion
}
