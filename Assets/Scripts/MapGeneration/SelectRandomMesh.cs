using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


[Serializable]
public class LODMeshs
{
    public Mesh LOD0;
    public Mesh LOD1;

    public LODMeshs(Mesh _LOD0, Mesh _LOD1)
    {
        LOD0 = _LOD0;
        LOD1 = _LOD1;
    }
}

public class SelectRandomMesh : MonoBehaviour
{
    [SerializeField] private List<LODMeshs> meshs = new List<LODMeshs>();
    public SkinnedMeshRenderer LOD1Variant;

    [SerializeField] private bool _isLayerP1;

    private NPCAIStateManager _npc;
    private PlayerStateManager _player;

    private void Awake()
    {
        
    }

    void Start()
    {
        if (GetComponentInParent<NPCAIStateManager>() != null)
        {
            _npc = GetComponentInParent<NPCAIStateManager>();
        }
        else if(GetComponentInParent<PlayerStateManager>() != null)
        {
            _player = GetComponentInParent<PlayerStateManager>();
        }
        AssignRandomMesh();
    }

    private void AssignRandomMesh()
    {
        int meshIndex = Random.Range(0, meshs.Count);
        if (_player == null)
        {
            if(_npc != null)
            {
                if (_npc.startGeneration)
                {
                    GetComponent<SkinnedMeshRenderer>().sharedMesh = meshs[meshIndex].LOD0;
                }
                else
                {
                    if (_isLayerP1) meshIndex = _npc.startIndexP1;
                    else meshIndex = _npc.startIndexP2;
                    GetComponent<SkinnedMeshRenderer>().sharedMesh = meshs[meshIndex].LOD0;
                }
            }
            else
            {
                GetComponent<SkinnedMeshRenderer>().sharedMesh = meshs[meshIndex].LOD0;
            }
        }
        else
        {
            GetComponent<SkinnedMeshRenderer>().sharedMesh = meshs[meshIndex].LOD0;

            if (_isLayerP1) _player.visuals.startIndexP1 = meshIndex;
            else _player.visuals.startIndexP2 = meshIndex;
        }

        if (LOD1Variant != null)
        {
            LOD1Variant.sharedMesh = meshs[meshIndex].LOD1;
        }
    }
}
