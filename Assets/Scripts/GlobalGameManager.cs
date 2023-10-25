using DG.Tweening;
using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalGameManager : MonoBehaviour
{
    #region Singleton
    public static GlobalGameManager Instance;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            _soundManager = GetComponent<SoundManager>();
            _splitScreenInteractions = GetComponent<SplitScreenInteractions>();
        }
    }
    #endregion

    private PlayerStateManager _Player1;
    private PlayerStateManager _Player2;
    public PlayerStateManager Player1 { get { return _Player1; } 
        set 
        { 
            _Player1 = value;
            npcManager.PlayerAssigned();
            splitScreenInteractions.AddPlayer(value);
            soundManager.AddPlayer(value);
        } 
    }
    public PlayerStateManager Player2 { get { return _Player2; } 
        set 
        { 
            _Player2 = value;
            npcManager.PlayerAssigned();
            splitScreenInteractions.AddPlayer(value);
            soundManager.AddPlayer(value);

            if (Player1 != null)
            {
                Player1.EnablePlayer();
            }
        } 
    }

    [SerializeField] private GameObject _NPCManagerPrefab;
    [SerializeField] private Image _screenFader;
    [SerializeField] private float _fadeInTime;
    [SerializeField] private float _fadeOutTime;

    [SerializeField] private GameObject _Confetti;

    private NPCManager _npcManager;
    public NPCManager npcManager { get { return _npcManager; } }

    private SplitScreenInteractions _splitScreenInteractions;
    public SplitScreenInteractions splitScreenInteractions { get { return _splitScreenInteractions; } }

    private SoundManager _soundManager;
    public SoundManager soundManager { get { return _soundManager; } }

    private Coroutine _loadSceneCor;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FadeIn();
        if (scene.buildIndex == 1)
        {
            GameObject npcMan = Instantiate(_NPCManagerPrefab, Vector3.zero, Quaternion.identity);
            _npcManager = npcMan.GetComponent<NPCManager>();
            _npcManager.spawnNPCs = false;
        }
        else if (scene.buildIndex == 2)
        {
            GameObject npcMan = Instantiate(_NPCManagerPrefab, Vector3.zero, Quaternion.identity);
            _npcManager = npcMan.GetComponent<NPCManager>();
        }
    }

    public bool IsPlaying(EventInstance instance)
    {
        PLAYBACK_STATE state;
        instance.getPlaybackState(out state);
        return state != PLAYBACK_STATE.STOPPED;
    }

    public void PlayerWon()
    {
        GameObject.FindGameObjectWithTag("Curtain").GetComponent<Animator>().SetTrigger("Fall");
        StartCoroutine(LoadSceneDelay(10, 0));
        Player1.RemovePlayerActions();
        Player2.RemovePlayerActions();
    }

    public void LoadSceneIn(float delay, int buildIndex)
    {
        if (_loadSceneCor != null) StopCoroutine(_loadSceneCor);
        _loadSceneCor = StartCoroutine(LoadSceneDelay(delay, buildIndex));
    }

    IEnumerator LoadSceneDelay(float delay, int buildIndex)
    {
        if(delay - _fadeOutTime > 0) yield return new WaitForSeconds(delay - _fadeOutTime);
        FadeOut();
        yield return new WaitForSeconds(_fadeOutTime);
        if(_Confetti.activeInHierarchy) _Confetti.SetActive(false); //:( no more confetti
        SceneManager.LoadScene(buildIndex);
    }

    public void FadeOut()
    {
        Color faderCol = _screenFader.color;
        _screenFader.DOColor(new Color(faderCol.r, faderCol.g, faderCol.b, 1), _fadeOutTime);
    }

    public void FadeIn()
    {
        Color faderCol = _screenFader.color;
        _screenFader.color = new Color(faderCol.r, faderCol.g, faderCol.b, 1);
        _screenFader.DOColor(new Color(faderCol.r, faderCol.g, faderCol.b, 0), _fadeInTime);
    }

    public void ConfettiDrop()
    {
        _Confetti.SetActive(true);
    }

    private void Start()
    {
        Screen.SetResolution(1920, 1080, true);
        InputSystem.settings.SetInternalFeatureFlag("DISABLE_SHORTCUT_SUPPORT", true);
        Application.targetFrameRate = -1;
    }
}
