using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{

    private EventInstance _music;
    [SerializeField] private EventReference _musicEvent;

    enum MusicStates
    {
        menu,
        neutral,
        airport,
        prison
    }

    private MusicStates currentState;
    private float currentIntesity = 0;

    [SerializeField] private int differenceToSwitch;
    [SerializeField] private int differenceToSwitchBack;

    [SerializeField] private bool _debugLogs;

    [SerializeField] private bool _playMusic;

    #region Singleton
    public static SoundManager Instance;

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
            if (_playMusic)
            {
                _music = RuntimeManager.CreateInstance(_musicEvent);
                _music.start();
            }
        }
    }
    #endregion

    private void OnDisable()
    {
        if (GlobalGameManager.Instance.Player1 != null) GlobalGameManager.Instance.Player1.OnInfluence -= OnInflunceHandler;
        if (GlobalGameManager.Instance.Player2 != null) GlobalGameManager.Instance.Player2.OnInfluence -= OnInflunceHandler;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.buildIndex)
        {
            case 0:
                currentState = MusicStates.menu;
                break;

            case 1:
                currentState = MusicStates.menu;
                break;

            case 2:
                currentState = MusicStates.neutral;
                break;
        }
        _music.setParameterByName("State", (int)currentState);
    }


    public void AddPlayer(PlayerStateManager player)
    {
        player.OnInfluence += OnInflunceHandler;
    }

    private void OnInflunceHandler()
    {
        if (GlobalGameManager.Instance.Player1 == null || GlobalGameManager.Instance.Player2 == null) return;
        int influenceP1 = GlobalGameManager.Instance.Player1.currentInfluence;
        int influenceP2 = GlobalGameManager.Instance.Player2.currentInfluence;
        int difference = influenceP1 - influenceP2;

        if(_debugLogs) Debug.Log("difference:" + difference);

        if (currentState == MusicStates.neutral && Mathf.Abs(difference) >= differenceToSwitch)
        {
            currentState = difference > 0 ? MusicStates.airport : MusicStates.prison;
            _music.setParameterByName("State", (int)currentState);
            if (_debugLogs) Debug.Log("music state: " + currentState);
        }
        else if ((currentState == MusicStates.prison || currentState == MusicStates.airport) && Mathf.Abs(difference) <= differenceToSwitchBack)
        {
            currentState = MusicStates.neutral;
            _music.setParameterByName("State", (int)currentState);
            if (_debugLogs) Debug.Log("music state: " + currentState);
        }

        float higherInfluence = difference > 0 ? influenceP1 : influenceP2;
        currentIntesity = higherInfluence / 100;
        if (_debugLogs) Debug.Log("intensity: " + currentIntesity);
        _music.setParameterByName("Intensity", currentIntesity);
    }
}
