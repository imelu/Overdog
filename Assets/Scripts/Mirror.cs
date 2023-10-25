using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using WaterSystem.Rendering;

public class Mirror : MonoBehaviour
{
    private PlanarReflections _planarReflections;
    private static readonly int _planarReflectionTextureId = Shader.PropertyToID("_PlanarReflectionTexture");

    void Start()
    {
        
    }

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
        //PlanarReflections.m_planeOffset = transform.position.y;
        //PlanarReflections.m_settings = settingsData.planarSettings;
        PlanarReflections.m_settings.m_ClipPlaneOffset = 0;
        //PlanarReflections.m_settings.m_ResolutionCustom = new int2(320 ,180);
        PlanarReflections.m_settings.m_ResolutionMode = PlanarReflections.ResolutionModes.Full;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
        Shader.SetGlobalTexture(_planarReflectionTextureId, null);
    }

    private void BeginCameraRendering(ScriptableRenderContext src, Camera cam)
    {
        PlanarReflections.Execute(src, cam, transform);
    }
}
