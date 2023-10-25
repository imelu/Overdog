using Cinemachine;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.Shapes;
using TMPro;
using DG.Tweening;

public class PlayerManager : MonoBehaviour
{
    //[SerializeField] private List<Shader> shaders = new List<Shader>();
    [SerializeField] private List<PlayerInput> players = new List<PlayerInput>();
    [SerializeField] private List<Transform> startingPoints;
    [SerializeField] private List<LayerMask> playerLayers;

    [SerializeField] private GameObject StartCam1;
    [SerializeField] private GameObject StartCam2;

    [SerializeField] private GameObject SingleKeyboard;
    [SerializeField] private GameObject SingleKeyboardText;
    // Wait for opponent pulsating
    [SerializeField] private TMP_Text PlayerOneText;
    [SerializeField] private TMP_Text PlayerTwoText;

    [SerializeField] private float textPulseDuration;

    private PlayerInputManager playerInputManager;

    private void Awake()
    {
        playerInputManager = GetComponent<PlayerInputManager>();
    }

    private void OnEnable()
    {
        playerInputManager.onPlayerJoined += AddPlayer;
    }

    private void OnDisable()
    {
        playerInputManager.onPlayerJoined -= AddPlayer;
    }

    private void Start()
    {
        PlayerOneText.DOColor(new Color(1, 1, 1, 0), textPulseDuration).SetLoops(-1, LoopType.Yoyo);
        PlayerTwoText.DOColor(new Color(1, 1, 1, 0), textPulseDuration).SetLoops(-1, LoopType.Yoyo);
        SingleKeyboardText.GetComponent<TMP_Text>().DOColor(new Color(1, 1, 1, 0), textPulseDuration).SetLoops(-1, LoopType.Yoyo);
    }

    public void AddPlayer(PlayerInput player)
    {
        players.Add(player);

        SetPlayerToSpawnpoint(player.transform);

        //Spawnpositions
        Transform playerParent = player.transform.parent;
        /* 
        playerParent.position = startingPoints[players.Count - 1].position;
        */

        int layerToAdd = (int)Mathf.Log(playerLayers[players.Count - 1].value, 2);
        int layerToAdd2 = (int)Mathf.Log(playerLayers[players.Count + 1].value, 2);

        playerParent.GetComponentInChildren<CinemachineFreeLook>().gameObject.layer = layerToAdd;
        
        playerParent.GetComponentInChildren<Camera>().cullingMask |= 1 << layerToAdd;
        playerParent.GetComponentInChildren<Camera>().cullingMask |= 1 << layerToAdd2;
        playerParent.GetComponentInChildren<InputHandler>().horizontal = player.actions.FindAction("Look");

        player.GetComponent<PlayerStateManager>().victoryCam.GetComponent<Camera>().cullingMask |= 1 << layerToAdd;
        player.GetComponent<PlayerStateManager>().victoryCam.GetComponent<Camera>().cullingMask |= 1 << layerToAdd2;
        player.GetComponent<PlayerStateManager>().victoryCam.gameObject.layer = layerToAdd;

        if (players.Count == 1)
        {
            player.GetComponent<PlayerStateManager>().isPlayerOne = true;
            GlobalGameManager.Instance.Player1 = player.GetComponent<PlayerStateManager>();
            playerParent.GetComponentInChildren<Camera>().rect = new Rect(0,0,0.5f,1);
            //StartCam1.SetActive(false);
            PlayerOneText.text = "Wait for Opponent";
            if(!player.GetComponent<PlayerInput>().currentControlScheme.Equals("Gamepad")) PlayerTwoText.text = "Press Start";
        }
        else
        {
            player.GetComponent<PlayerStateManager>().isPlayerOne = false;
            GlobalGameManager.Instance.Player2 = player.GetComponent<PlayerStateManager>();
            StartCam1.SetActive(false); // new start
            StartCam2.SetActive(false);
            GlobalGameManager.Instance.Player1.cam.GetComponent<Camera>().enabled = true;
            GlobalGameManager.Instance.Player2.cam.GetComponent<Camera>().enabled = true;
        }
        SingleKeyboardText.SetActive(false);

        GlobalGameManager.Instance.GetComponent<SplitScreenInteractions>().AddPlayer(player.GetComponent<PlayerStateManager>());
        GlobalGameManager.Instance.soundManager.AddPlayer(player.GetComponent<PlayerStateManager>());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && players.Count == 0)
        {
            playerInputManager.enabled = false;
            SingleKeyboard.SetActive(true);
            StartCam1.SetActive(false);
            StartCam2.SetActive(false);
            SingleKeyboardText.SetActive(false);
            SingleKeyboard.GetComponent<SingleKeyboardInputs>().P1.IH.horizontal = SingleKeyboard.GetComponent<PlayerInput>().actions.FindAction("Look");
            SingleKeyboard.GetComponent<SingleKeyboardInputs>().P2.IH.horizontal = SingleKeyboard.GetComponent<PlayerInput>().actions.FindAction("LookP2");

            SetPlayerToSpawnpoint(SingleKeyboard.GetComponent<SingleKeyboardInputs>().P1.transform);
            SetPlayerToSpawnpoint(SingleKeyboard.GetComponent<SingleKeyboardInputs>().P2.transform);

            GlobalGameManager.Instance.Player1 = SingleKeyboard.GetComponent<SingleKeyboardInputs>().P1.GetComponent<PlayerStateManager>();
            GlobalGameManager.Instance.Player2 = SingleKeyboard.GetComponent<SingleKeyboardInputs>().P2.GetComponent<PlayerStateManager>();

            players.Add(SingleKeyboard.GetComponent<SingleKeyboardInputs>().P1.GetComponent<PlayerInput>());
            players.Add(SingleKeyboard.GetComponent<SingleKeyboardInputs>().P2.GetComponent<PlayerInput>());

            GlobalGameManager.Instance.Player1.cam.GetComponent<Camera>().enabled = true;
            GlobalGameManager.Instance.Player2.cam.GetComponent<Camera>().enabled = true;
        }
    }

    private void SetPlayerToSpawnpoint(Transform player)
    {
        player.GetComponent<CharacterController>().enabled = false;
        Room room = GlobalGameManager.Instance.npcManager.WeightedRandomRoom(); 
        player.transform.position = GlobalGameManager.Instance.npcManager.GetRandomLocationInRoom(room) + new UnityEngine.Vector3(0,1,0);
        player.GetComponent<PlayerStateManager>().camLookAt.position = player.transform.position;
        if (room.isInGate) player.GetComponent<PlayerStateManager>().isInGate = true;
        player.GetComponent<CharacterController>().enabled = true;
    }
}
