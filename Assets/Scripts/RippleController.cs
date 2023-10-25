using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class RippleController : MonoBehaviour
{
    [SerializeField] private NPCAIStateManager npc;
    [SerializeField] private PlayerStateManager player;
    [SerializeField] private ParticleSystem rippleParticleLeft;
    [SerializeField] private ParticleSystem rippleParticleRight;

    [SerializeField] private ParticleSystem rippleParticleLeftP2;
    [SerializeField] private ParticleSystem rippleParticleRightP2;

    [SerializeField] private Color corruptColor;
    [SerializeField] private Color normalColor;
    [SerializeField] private Gradient susGradient = new Gradient();

    ParticleSystem.MainModule mainLeft;
    ParticleSystem.MainModule mainRight;
    ParticleSystem.MainModule mainLeftP2;
    ParticleSystem.MainModule mainRightP2;

    // Start is called before the first frame update
    void Start()
    {
        mainLeft = rippleParticleLeft.main;
        mainRight = rippleParticleRight.main;

        if (rippleParticleLeftP2 != null) mainLeftP2 = rippleParticleLeftP2.main;
        if (rippleParticleRightP2 != null) mainRightP2 = rippleParticleRightP2.main;

        if (npc != null)
        {
            mainLeft.startSize = GetRippleSize();
            mainRight.startSize = GetRippleSize();
        }
        else
        {
            GradientColorKey[] colorKey = new GradientColorKey[2];
            colorKey[0].color = normalColor;
            colorKey[0].time = 0;
            colorKey[1].color = corruptColor;
            colorKey[1].time = 1;

            GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
            alphaKey[0].alpha = 1;
            alphaKey[0].time = 0;
            alphaKey[1].alpha = 1;
            alphaKey[1].time = 1;

            susGradient.SetKeys(colorKey, alphaKey);
        }
    }

    private void Update()
    {
        //if (Input.GetMouseButtonDown(0)) StartRippleLeft();
    }

    public void StartRippleLeft()
    {
        Color col = normalColor;
        if (npc != null)
        {
            if (npc.isCorruptedP1 || npc.isCorruptedP2) col = corruptColor;
            mainLeft.startColor = col;
            rippleParticleLeft.customData.SetVector(ParticleSystemCustomData.Custom1, 0, Time.time);
            rippleParticleLeft.Emit(1);
        }
        else
        {
            col = susGradient.Evaluate(player.currentSuspicion / player.suspicionThreshold);
            if (player.isPlayerOne)
            {
                mainLeft.startSize = GetRippleSize();
                mainLeft.startColor = col;
                SpawnRipple(rippleParticleLeft);
            }
            else
            {
                mainLeftP2.startSize = GetRippleSize();
                mainLeftP2.startColor = col;
                SpawnRipple(rippleParticleLeftP2);
            }
            EmulateNPCRippleLeft();
        }
    }

    public void StartRippleRight()
    {
        Color col = normalColor;
        if (npc != null)
        {
            if (npc.isCorruptedP1 || npc.isCorruptedP2) col = corruptColor;
            mainRight.startColor = col;
            rippleParticleRight.customData.SetVector(ParticleSystemCustomData.Custom1, 0, Time.time);
            rippleParticleRight.Emit(1);
        }
        else
        {
            col = susGradient.Evaluate(player.currentSuspicion / player.suspicionThreshold);
            if (player.isPlayerOne)
            {
                mainRight.startSize = GetRippleSize();
                mainRight.startColor = col;
                SpawnRipple(rippleParticleRight);
            }
            else
            {
                mainRightP2.startSize = GetRippleSize();
                mainRightP2.startColor = col;
                SpawnRipple(rippleParticleRightP2);
            }
            EmulateNPCRippleRight();
        }
        
    }

    private void EmulateNPCRippleLeft()
    {
        int influence = 1;
        Color col = normalColor;

        if (player.PossessedNPC != null)
        {
            NPCAIStateManager npc = player.PossessedNPC.GetComponent<NPCAIStateManager>();
            influence = npc.influence;
            if (npc.isCorruptedP1 || npc.isCorruptedP2) col = corruptColor;
        }

        if (!player.isPlayerOne)
        {
            mainLeft.startSize = GetRippleSize();
            mainLeft.startColor = col;
            SpawnRipple(rippleParticleLeft);
        }
        else
        {
            mainLeftP2.startSize = GetRippleSize();
            mainLeftP2.startColor = col;
            SpawnRipple(rippleParticleLeftP2);
        }
    }

    private void EmulateNPCRippleRight()
    {
        int influence = 1;
        Color col = normalColor;

        if (player.PossessedNPC != null)
        {
            NPCAIStateManager npc = player.PossessedNPC.GetComponent<NPCAIStateManager>();
            influence = npc.influence;
            if (npc.isCorruptedP1 || npc.isCorruptedP2) col = corruptColor;
        }

        if (!player.isPlayerOne)
        {
            mainRight.startSize = GetRippleSize();
            mainRight.startColor = col;
            SpawnRipple(rippleParticleRight);
        }
        else
        {
            mainRightP2.startSize = GetRippleSize();
            mainRightP2.startColor = col;
            SpawnRipple(rippleParticleRightP2);
        }
    }

    private float GetRippleSize()
    {
        if (npc != null) return Mathf.Log10(npc.influence) + 1;
        else return Mathf.Log10(player.currentInfluence) + 1;
    }

    private void SpawnRipple(ParticleSystem particleSystem)
    {
        particleSystem.customData.SetVector(ParticleSystemCustomData.Custom1, 0, Time.time);
        particleSystem.Emit(1);
    }
}
