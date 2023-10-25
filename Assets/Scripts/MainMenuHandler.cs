using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using Unity.VisualScripting;
using FMODUnity;
using FMOD;
using FMOD.Studio;
using UnityEngine.InputSystem.HID;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private Transform _ButtonParent;

    private List<TMP_Text> _buttonTexts = new List<TMP_Text>();

    private int _buttonIndex = 0;

    [SerializeField] private GameObject _MainMenuCamera;
    [SerializeField] private GameObject _CreditsCam1;
    [SerializeField] private GameObject _CreditsCam2;

    [SerializeField] private GameObject _CreditsText2;

    [SerializeField] private VideoPlayer _introVideo;


    private bool _videoPlaying = false;

    private bool _abilitiesEnabled = false;

    [SerializeField] private GameObject _attackParticle;
    private bool _attackOnCD;
    private bool _attacking;

    [SerializeField] private GameObject _abilityNPCs;
    [SerializeField] private NPCAIStateManager _npcToCorrupt;

    [SerializeField] private GameObject _possessCam;

    #region Tutorial

    [SerializeField] private Transform _TutorialCamParent;

    private List<GameObject> _tutorialCams = new List<GameObject>();

    [SerializeField] private Transform _TutorialTextParent;

    private List<GameObject> _tutorialTexts = new List<GameObject>();

    [SerializeField] private GameObject _tutorialJanitorPrefab;
    [SerializeField] private Transform _janitorSpawnPos;
    [SerializeField] private Transform _janitorTarget;
    private NPCAIStateManager _tutorialJanitor;
    [SerializeField] private GameObject _tutorialNPCs;

    [SerializeField] private GameObject _securityPrefab;
    private Transform _security;
    [SerializeField] private Transform _securityTarget;

    [SerializeField] private NPCAIStateManager _tutorialGuard;

    [SerializeField] private GameObject _audienceNPCsPrefab;
    private GameObject _audienceNPCs;
    [SerializeField] private NPCAIStateManager _entertainer;

    [SerializeField] private Transform _ButtonR1;
    [SerializeField] private Transform _ButtonR2;
    [SerializeField] private Transform _ButtonL2;
    [SerializeField] private Transform _ButtonSquare; //tween stuff

    private Vector3 ButtonLRSize;
    private Vector3 ButtonSquareSize;

    private bool _L2CD;
    private bool _R2CD;
    private bool _R1CD;
    private bool _SquareCD;

    private bool _inCredits;
    private int _creditPos;

    private bool _inTutorial;
    private int _tutorialPos;

    private bool _transitioning;

    #endregion

    #region Sound

    [SerializeField] private EventReference PossessSuccess;

    [SerializeField] private EventReference CorruptSuccess;

    [SerializeField] private EventReference AttackTarget;

    [SerializeField] private EventReference ButtonPress;

    private EventInstance Button;

    #endregion

    PlayerInput input;

    private void Awake()
    {
        _introVideo.loopPointReached += IntroComplete;

        _introVideo.targetTexture.Release();

        Button = RuntimeManager.CreateInstance(ButtonPress);
    }

    private void Start()
    {
        foreach (Transform child in _ButtonParent) _buttonTexts.Add(child.GetComponent<TMP_Text>());
        foreach (Transform child in _TutorialCamParent) _tutorialCams.Add(child.gameObject);
        foreach (Transform child in _TutorialTextParent) _tutorialTexts.Add(child.gameObject);

        _buttonTexts[_buttonIndex].fontStyle = FontStyles.Underline | FontStyles.UpperCase;
        input = GetComponent<PlayerInput>();

        ButtonLRSize = _ButtonR1.localScale;
        ButtonSquareSize = _ButtonSquare.localScale;
    }

    #region Input

    public void OnLeft()
    {
        //OnCancel();
    }

    public void OnRight()
    {
        //OnConfirm();
    }

    public void OnUp()
    {
        if (_transitioning || _videoPlaying) return;
        _buttonTexts[_buttonIndex].fontStyle = FontStyles.Normal | FontStyles.UpperCase;
        _buttonIndex--;
        if (_buttonIndex < 0) _buttonIndex = _buttonTexts.Count - 1;
        _buttonTexts[_buttonIndex].fontStyle = FontStyles.Underline | FontStyles.UpperCase;

        RuntimeManager.PlayOneShot(ButtonPress);
    }

    public void OnDown()
    {
        if (_transitioning || _videoPlaying) return;
        Button.start();
        _buttonTexts[_buttonIndex].fontStyle = FontStyles.Normal | FontStyles.UpperCase;
        _buttonIndex++;
        if (_buttonIndex >= _buttonTexts.Count) _buttonIndex = 0;
        _buttonTexts[_buttonIndex].fontStyle = FontStyles.Underline | FontStyles.UpperCase;

        //RuntimeManager.PlayOneShot(ButtonPress);
        Button.start();
    }

    public void OnConfirm()
    {
        if (_transitioning || _videoPlaying) return;
        RuntimeManager.PlayOneShot(ButtonPress);
        if (_inCredits)
        {
            switch (_creditPos)
            {
                case 1:
                    // composer credits
                    _CreditsCam2.SetActive(true);
                    _creditPos++;
                    StartCoroutine(Transitioning());
                    break;

                case 2:
                    _CreditsCam1.SetActive(false);
                    _CreditsCam2.SetActive(false);
                    _creditPos = 0;
                    _inCredits = false;
                    FadeOutText(_CreditsText2);
                    StartCoroutine(Transitioning());
                    break;
            }
            return;
        }
        if (_inTutorial)
        {
            if (_tutorialPos >= _tutorialCams.Count)
            {
                foreach (GameObject Camera in _tutorialCams) Camera.SetActive(false);
                foreach (Transform child in _audienceNPCs.transform)
                {
                    child.GetComponent<NPCAIStateManager>().agent.SetDestination(_janitorTarget.position);
                    child.GetComponent<NPCAIStateManager>().GoToTutorialState();
                }
                FadeOutText(_tutorialTexts[_tutorialPos - 1]);
                _tutorialPos = 0;
                _inTutorial = false;
                _tutorialNPCs.SetActive(false);
                _entertainer.anim.SetBool("playPiano", false);
            }
            else
            {
                FadeOutText(_tutorialTexts[_tutorialPos - 1]);
                _tutorialCams[_tutorialPos].SetActive(true);
                _tutorialPos++;
            }
            StartCoroutine(Transitioning());
            return;
        }
        switch (_buttonIndex)
        {
            case 0:
                _introVideo.gameObject.SetActive(true);
                _videoPlaying = true;
                GlobalGameManager.Instance.LoadSceneIn((float)_introVideo.clip.length, 2);
                break;

            case 1:
                // tutorial
                _inTutorial = true;
                _tutorialNPCs.SetActive(true);

                // our credits
                _tutorialCams[_tutorialPos].SetActive(true);
                _tutorialPos++;
                StartCoroutine(Transitioning());
                break;

            case 2:
                // show credits
                _inCredits = true;
                _creditPos++;

                // our credits
                _CreditsCam1.SetActive(true);

                StartCoroutine(Transitioning());
                break;

            case 3:
                Application.Quit();
                break;
        }
    }

    /*
    public void OnCancel()
    {
        if (_transitioning || _videoPlaying) return;
        if (_inCredits)
        {
            switch (_creditPos)
            {
                case 1:
                    // composer credits
                    _CreditsCam1.SetActive(false);
                    _creditPos--;
                    _inCredits = false;

                    StartCoroutine(Transitioning());
                    break;

                case 2:
                    _CreditsCam2.SetActive(false);
                    _creditPos--;

                    StartCoroutine(Transitioning());
                    break;
            }
            return;
        }
        if (_inTutorial)
        {
            _tutorialPos--;
            FadeOutText(_tutorialTexts[_tutorialPos]);
            _tutorialCams[_tutorialPos].SetActive(false);
            StartCoroutine(Transitioning());
            if(_tutorialPos <= 0)
            {
                _tutorialPos = 0;
                _inTutorial = false;
                _tutorialNPCs.SetActive(false);
            }
            return;
        }
    }
    */


    public void OnCorruptDown()
    {
        if (!_abilitiesEnabled) return;
        _npcToCorrupt.Mesh.materials[1].DOKill();
        if (!_L2CD)
        {
            _L2CD = true;
            _ButtonL2.DOPunchScale(ButtonLRSize * 0.5f, 0.4f, 5, 0.5f).OnComplete(()=>_L2CD = false); //tween
        }

        _npcToCorrupt.Mesh.materials[1].DOFloat(0.825f, "_BloodAmount", 1);
    }

    public void OnCorruptUp()
    {
        if (!_abilitiesEnabled) return;
        _npcToCorrupt.Mesh.materials[1].DOKill();
        _npcToCorrupt.Mesh.materials[1].DOFloat(0, "_BloodAmount", 1);
    }

    public void OnPossessDown()
    {
        if (!_abilitiesEnabled) return;
        if (!_R2CD)
        {
            _R2CD = true;
            _ButtonR2.DOPunchScale(ButtonLRSize * 0.5f, 0.4f, 5, 0.5f).OnComplete(() => _R2CD = false); //tween
        }
        _possessCam.SetActive(true);
    }

    public void OnPossessUp()
    {
        if (!_abilitiesEnabled) return;
        _possessCam.SetActive(false);
    }

    public void OnAttackDown()
    {
        if (!_abilitiesEnabled || _attackOnCD) return;
        if (!_R1CD)
        {
            _R1CD = true;
            _ButtonR1.DOPunchScale(ButtonLRSize * 0.5f, 0.4f, 5, 0.5f).OnComplete(() => _R1CD = false); //tween
        }
        _attackParticle.SetActive(true);
        _attacking = true;
    }

    public void OnAttackUp()
    {
        if (!_attacking) return;
        ParticleSystem ps = _attackParticle.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = ps.main;

        main.startColor = new Color(1, 1, 1, 0.01f);
        _attackParticle.transform.DOScale(Vector3.zero, 7).OnComplete(() => ResetAttackVisuals(ps.main));
        _attacking = false;
        _attackOnCD = true;
    }


    public void OnSprintDown()
    { 
        if (!_abilitiesEnabled) return;
        if (!_SquareCD)
        {
            _SquareCD = true;
            _ButtonSquare.DOPunchScale(ButtonSquareSize * 0.5f, 0.4f, 5, 0.5f).OnComplete(() => _SquareCD = false); //tween
        }
    }

    public void OnSkip()
    {
        if (_videoPlaying)
        {
            GlobalGameManager.Instance.LoadSceneIn(0, 2);
            //SceneManager.LoadScene(2);
        }
    }

    #endregion

    private void ResetAttackVisuals(ParticleSystem.MainModule main)
    {
        _attackParticle.SetActive(false);
        _attackParticle.transform.localScale = Vector3.one;
        main.startColor = new Color(1, 1, 1, 1);
        _attackOnCD = false;
    }

    private void IntroComplete(VideoPlayer player)
    {
        //SceneManager.LoadScene(2);
    }

    private void FadeInText(GameObject Text)
    {
        foreach(Transform child in Text.transform){
            if (child.GetComponent<TMP_Text>() != null)
            {
                TMP_Text txt = child.GetComponent<TMP_Text>();
                Color col = txt.color;
                txt.color = new Color(col.r, col.g, col.b, 0);
                Text.SetActive(true);
                txt.DOColor(new Color(col.r, col.g, col.b, 1), 1f);
            } else if(child.GetComponent<DecalProjector>() != null)
            {
                DecalProjector decal = child.GetComponent<DecalProjector>();
                StartCoroutine(FadeDecal(decal, 0, 1));
            } else if(child.GetComponent<SpriteRenderer>() != null)
            {
                SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
                spriteRenderer.DOColor(new Color(1, 1, 1, 1), 1f);
            }
        }
        if (Text.GetComponent<SpriteRenderer>() != null)
        {
            SpriteRenderer spriteRenderer = Text.GetComponent<SpriteRenderer>();
            spriteRenderer.DOColor(new Color(1, 1, 1, 1), 1f);
        }
    }

    IEnumerator FadeDecal(DecalProjector decal, float startVal, float endVal)
    {
        for(float t = 0; t <= 1; t += Time.deltaTime)
        {
            decal.fadeFactor = Mathf.Lerp(startVal, endVal, t);
            yield return null;
        }
    }

    private void FadeOutText(GameObject Text)
    {
        foreach (Transform child in Text.transform)
        {
            if (child.GetComponent<TMP_Text>() != null)
            {
                TMP_Text txt = child.GetComponent<TMP_Text>();
                Color col = txt.color;
                //txt.color = new Color(col.r, col.g, col.b, 1);
                txt.DOColor(new Color(col.r, col.g, col.b, 0), 0.5f).OnComplete(() => Text.SetActive(false));
            }
            else if (child.GetComponent<DecalProjector>() != null)
            {
                DecalProjector decal = child.GetComponent<DecalProjector>();
                StartCoroutine(FadeDecal(decal, 1, 0));
            }
            else if (child.GetComponent<SpriteRenderer>() != null)
            {
                SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
                spriteRenderer.DOColor(new Color(1, 1, 1, 0), 1f);
            }
        }
        if (Text.GetComponent<SpriteRenderer>() != null)
        {
            SpriteRenderer spriteRenderer = Text.GetComponent<SpriteRenderer>();
            spriteRenderer.DOColor(new Color(1, 1, 1, 0), 1f);
        }
    }

    IEnumerator Transitioning()
    {
        _transitioning = true;
        yield return new WaitForSeconds(2);
        _transitioning = false;
        if (_inTutorial && _tutorialPos >= 1) FadeInText(_tutorialTexts[_tutorialPos - 1]);

        switch (_creditPos)
        {
            case 1:
                break;

            case 2:
                FadeInText(_CreditsText2);
                break;
        }

        switch (_tutorialPos)
        {
            case 1:
                // boss action?
                _abilityNPCs.SetActive(true);
                break;

            case 2:
                _abilitiesEnabled = true;
                break;

            case 3:
                _abilitiesEnabled = false;
                _abilityNPCs.SetActive(false);
                var janitor = Instantiate(_tutorialJanitorPrefab, _janitorSpawnPos);
                _tutorialJanitor = janitor.GetComponent<NPCAIStateManager>();
                _tutorialJanitor.targetPosition = _janitorTarget;
                break;

            case 4:
                _tutorialNPCs.SetActive(false);

                StartCoroutine(ActivateTutorialAction(_tutorialJanitor, 2));

                var security = Instantiate(_securityPrefab, transform);
                _security = security.transform;
                foreach (Transform child in _security)
                {
                    child.GetComponent<NPCAIStateManager>().targetPosition = _securityTarget;
                }
                break;

            case 5:
                for (int d = 0; d < _security.childCount; d++)
                {
                    var child = _security.GetChild(d);
                    StartCoroutine(ActivateTutorialAction(child.GetComponent<NPCAIStateManager>(), 2-d));

                }
                /*foreach (Transform child in _security)
                {
                    StartCoroutine(ActivateTutorialAction(child.GetComponent<NPCAIStateManager>(), delay));
                }*/
                break;

            case 6:
                StartCoroutine(ActivateTutorialAction(_tutorialGuard, 1));
                break;

            case 7:
                _entertainer.anim.SetBool("playPiano", true);
                _audienceNPCs = Instantiate(_audienceNPCsPrefab, transform);
                foreach (Transform child in _audienceNPCs.transform) child.GetComponent<NPCAIStateManager>().ListenToEntertainer(_entertainer);
                break;
        }
    }

    IEnumerator ActivateTutorialAction(NPCAIStateManager npc, float delay)
    {
        yield return new WaitForSeconds(delay);
        if(npc.type == NPCAIStateManager.NPCType.guard)
        {
            npc.GuardExit();
            yield return new WaitForSeconds(2.5f);
            npc.LeaveExit();
        }
        else if (npc.type == NPCAIStateManager.NPCType.security)
        {
            npc.anim.SetTrigger("look");
            yield return new WaitForSeconds(7.5f);
            npc.GoToTutorialState();
        }
        else
        {
            npc.GoToTutorialState();
        }        
    }

    private void OnDisable()
    {
        input.actions = null;
    }
}
