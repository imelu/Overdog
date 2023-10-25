using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplitScreenInteractions : MonoBehaviour
{
    [SerializeField] private bool enableSplitScreenShift;
    [SerializeField] private int minInflDifference;
    [SerializeField] private float factor;

    [SerializeField] private float maxShift;

    [SerializeField] private float shiftSpeed;

    private static readonly int _splitscreenScaleDiffP1 = Shader.PropertyToID("_splitscreenScaleDiffP1");
    private static readonly int _splitscreenScaleDiffP2 = Shader.PropertyToID("_splitscreenScaleDiffP2");

    private Camera camP1;
    private Camera camP2;

    private Coroutine cor;

    private void Awake()
    {
        Shader.SetGlobalFloat(_splitscreenScaleDiffP1, 0.5f);
        Shader.SetGlobalFloat(_splitscreenScaleDiffP2, 0.5f);
    }

    private void OnDisable()
    {
        if (GlobalGameManager.Instance.Player1 != null) GlobalGameManager.Instance.Player1.OnInfluence -= OnInflunceHandler;
        if (GlobalGameManager.Instance.Player2 != null) GlobalGameManager.Instance.Player2.OnInfluence -= OnInflunceHandler;
    }

    public void AddPlayer(PlayerStateManager player)
    {
        if (!enableSplitScreenShift) return;
        player.OnInfluence += OnInflunceHandler;
        if (player.isPlayerOne) camP1 = player.cam.GetComponent<Camera>();
        else camP2 = player.cam.GetComponent<Camera>();
    }

    private void OnInflunceHandler()
    {
        if (GlobalGameManager.Instance.Player1 == null || GlobalGameManager.Instance.Player2 == null) return;
        int influenceP1 = GlobalGameManager.Instance.Player1.currentInfluence;
        int influenceP2 = GlobalGameManager.Instance.Player2.currentInfluence;
        /*Debug.Log("P1 " +influenceP1);
        Debug.Log("P2 " +influenceP2);*/
        int differnce = influenceP1 - influenceP2;
        if (Mathf.Abs(differnce) < minInflDifference) return;

        float shift = differnce * factor;
        if (Mathf.Abs(shift) > maxShift)
        {
            if (shift < 0) shift = -maxShift;
            else shift = maxShift;
        }

        StopAllCoroutines();
        StartCoroutine(ShiftScreens(shift));
        
        /*
        // change cameras
        camP1.rect = new Rect(0, 0, 0.5f + shift, 1);
        camP2.rect = new Rect(0.5f + shift, 0, 0.5f - shift, 1);

        // change floor mats

        Shader.SetGlobalFloat(_splitscreenScaleDiffP1, shift + 0.5f);
        Shader.SetGlobalFloat(_splitscreenScaleDiffP2, shift + 0.5f);*/
    }
    
    IEnumerator ShiftScreens(float shift)
    {
        float ratio = 0;
        float startShift = camP1.rect.width - 0.5f;
        float endShift = shift;

        while (ratio < 1)
        {
            ratio += Time.deltaTime * shiftSpeed;
            if (ratio > 1) ratio = 1;

            if(Time.frameCount % 2 == 0)
            {
                float currentShift = Mathf.Lerp(startShift, endShift, ratio);
                //Debug.Log(currentShift);
                // change cameras
                camP1.rect = new Rect(0, 0, 0.5f + currentShift, 1);
                camP2.rect = new Rect(0.5f + currentShift, 0, 0.5f - currentShift, 1);

                // change floor mats
                Shader.SetGlobalFloat(_splitscreenScaleDiffP1, currentShift + 0.5f);
                Shader.SetGlobalFloat(_splitscreenScaleDiffP2, currentShift + 0.5f);
            }
            
            yield return new WaitForEndOfFrame();
        }
    }
}
