using DG.Tweening;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class PlayerVisuals : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _MeshP1;
    [SerializeField] private SkinnedMeshRenderer _MeshP2;

    public SkinnedMeshRenderer MeshP1 { get { return _MeshP1; } }
    public SkinnedMeshRenderer MeshP2 { get { return _MeshP2; } }

    [SerializeField] private GameObject Janitor;
    [SerializeField] private GameObject Police;
    [SerializeField] private GameObject Security;
    [SerializeField] private GameObject Target;
    [SerializeField] private GameObject Boss;

    private PlayerStateManager _player;
    public PlayerStateManager player { get { return _player; } }

    private int _startIndexP1;
    public int startIndexP1 { get { return _startIndexP1; } set { _startIndexP1 = value; } }

    private int _startIndexP2;
    public int startIndexP2 { get { return _startIndexP2; } set { _startIndexP2 = value; } }

    [SerializeField] private float _possessStartSpeed;

    [SerializeField] private Color _PunishColor;
    [SerializeField] private Color _PossessColor;

    private void Awake()
    {
        _player = GetComponent<PlayerStateManager>();
    }

    public void SetStartMats(bool isP1)
    {
        /*if (!isP1) MeshP1.material = NPCMat;
        else MeshP2.material = NPCMat;*/
    }

    public void SwapVisuals(NPCAIStateManager.NPCType npc, Mesh meshP1, Mesh meshP2)
    {
        MeshP1.sharedMesh = meshP1;
        MeshP2.sharedMesh = meshP2;


        if(npc == NPCAIStateManager.NPCType.target)
        {
            Target.SetActive(true);
        }
        else
        {
            Target.SetActive(false);
        }
    }

    public void UpdateBloodAmount(float influence)
    {
        StartCoroutine(ActivateBloodyHands());
    }

    public void UpdatePossessGlow(float cooldown)
    {
        if (player.isPlayerOne)
        {
            //Color white
            foreach(Material mat in MeshP1.materials)
            {
                mat.SetColor("_PossessColor", _PossessColor);
                mat.SetFloat("_PossessFade", 1);
                //mat.SetFloat("_PossessPulseSpeed", 0);

                mat.DOFloat(0.3f, "_PossessFade", cooldown).OnComplete(() => mat.SetFloat("_PossessFade", 0));
                //mat.DOFloat(_possessStartSpeed, "_PossessPulseSpeed", cooldown);
            }
        } else
        {
            foreach(Material mat in MeshP2.materials)
            {
                mat.SetColor("_PossessColor", _PossessColor);
                mat.SetFloat("_PossessFade", 1);
                //mat.SetFloat("_PossessPulseSpeed", 0);

                mat.DOFloat(0.3f, "_PossessFade", cooldown).OnComplete(() => mat.SetFloat("_PossessFade", 0));
                //mat.DOFloat(_possessStartSpeed, "_PossessPulseSpeed", cooldown);
            }
        }
            
    }

    public void UpdatePunishGlow(float duration)
    {
        if (player.isPlayerOne)
        {
            foreach (Material mat in MeshP1.materials)
            {
                mat.SetColor("_PossessColor", _PunishColor);
                mat.SetFloat("_PossessFade", 1);
  
                mat.DOFloat(0.3f, "_PossessFade", duration).OnComplete(() => mat.SetFloat("_PossessFade", 0));
            }
        }
        else
        {
            foreach (Material mat in MeshP2.materials)
            {
                mat.SetColor("_PossessColor", _PunishColor);
                mat.SetFloat("_PossessFade", 1);

                mat.DOFloat(0.3f, "_PossessFade", duration).OnComplete(() => mat.SetFloat("_PossessFade", 0));
            }
        }
    }

    public void EmulateNPCBlood(bool isCorrupted, float bloodAmount)
    {
        float _bloodAmount = isCorrupted ? bloodAmount : 0;
        if (player.isPlayerOne)
        {
            MeshP2.materials[1].SetFloat("_BloodAmount", _bloodAmount);
        }
        else
        {
            MeshP1.materials[1].SetFloat("_BloodAmount", _bloodAmount);
        }
    }

    public void RemoveCurrentBlood()
    {
        StartCoroutine(RemoveBloodyHands());
    }

    public void AddCurrentBlood()
    {
        StartCoroutine(ActivateBloodyHands());
    }

    public void SetBloodToZero()
    {
        if (player.npcToPossess != null)
        {
            NPCAIStateManager npc = player.npcToPossess;
            if (npc.isCorruptedP1 || npc.isCorruptedP2) return;
        }

        if (player.isPlayerOne) MeshP1.materials[1].SetFloat("_BloodAmount", 0);
        else MeshP2.materials[1].SetFloat("_BloodAmount", 0);
    }

    IEnumerator RemoveBloodyHands()
    {
        float ratio = 0;
        float startTime = Time.time;
        float endTime = 0.5f;
        float finalAmount = 0.25f;
        float startAmount = player.isPlayerOne ? MeshP1.materials[1].GetFloat("_BloodAmount") : MeshP2.materials[1].GetFloat("_BloodAmount");
        
        if(player.PossessedNPC != null)
        {
            NPCAIStateManager npc = player.PossessedNPC.GetComponent<NPCAIStateManager>();
            if (npc.isCorruptedP1 || npc.isCorruptedP2) finalAmount = npc.bloodAmount;
        }

        while (ratio < 1)
        {
            ratio = (Time.time - startTime) / endTime;
            if (ratio > 1) ratio = 1;

            float _currentBloodAmount = Mathf.Lerp(startAmount, finalAmount, ratio);


            if (player.isPlayerOne) MeshP1.materials[1].SetFloat("_BloodAmount", _currentBloodAmount);
            else MeshP2.materials[1].SetFloat("_BloodAmount", _currentBloodAmount);

            yield return null;
        }
    }

    IEnumerator ActivateBloodyHands()
    {
        float ratio = 0;
        float startTime = Time.time;
        float endTime = 2f;
        float startAmount = player.isPlayerOne ? MeshP1.materials[1].GetFloat("_BloodAmount") : MeshP2.materials[1].GetFloat("_BloodAmount");
        float finalAmount = 0.25f + (0.75f * player.currentInfluence/ 100);

        if (player.npcToPossess != null)
        {
            NPCAIStateManager npc = player.npcToPossess;
            if (npc.isCorruptedP1 || npc.isCorruptedP2) startAmount = npc.bloodAmount;
        }

        while (ratio < 1)
        {
            ratio = (Time.time - startTime) / endTime;
            if (ratio > 1) ratio = 1;

            float _currentBloodAmount = Mathf.Lerp(startAmount, finalAmount, ratio);


            if (player.isPlayerOne) MeshP1.materials[1].SetFloat("_BloodAmount", _currentBloodAmount);
            else MeshP2.materials[1].SetFloat("_BloodAmount", _currentBloodAmount);

            yield return null;
        }
    }
}
