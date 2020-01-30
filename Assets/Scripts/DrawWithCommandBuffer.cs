using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using Random = System.Random;

[ExecuteInEditMode]
public class DrawWithCommandBuffer : MonoBehaviour
{
    [SerializeField]
    Mesh m_Mesh;

    [SerializeField]
    Material m_Material;

    [SerializeField]
    float m_Radius;

    [SerializeField]
    RenderTexture m_ColorTarget;

    [SerializeField]
    RenderTexture m_DepthTarget;

    [SerializeField]
    int m_NumInstances;
    
    CommandBuffer m_Cmd;
    List<Matrix4x4> m_Transforms = new List<Matrix4x4>();

    void OnGUI()
    {
        if (m_DepthTarget != null)
            GUI.DrawTexture(new Rect(0, 0, Screen.width * 0.5f, Screen.height * 0.5f), m_DepthTarget);
    }

    void OnEnable()
    {
        m_Cmd = new CommandBuffer();
        RenderPipelineManager.beginCameraRendering += Handler;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= Handler;
        m_Cmd.Release();
    }

    void Handler(ScriptableRenderContext context, Camera camera)
    {
        //Render();
    }

    void Update()
    {
        Render();
    }

    void Render()
    {
        if (m_Mesh == null || m_Material == null || m_DepthTarget == null || m_ColorTarget == null)
            return;
        
        m_Cmd.Clear();
        m_Cmd.SetRenderTarget(m_ColorTarget.colorBuffer, m_DepthTarget.colorBuffer);
        m_Cmd.ClearRenderTarget(true, true, Color.gray);

        var camera = Camera.main;
        
        var projectionMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true);

        var viewMatrix = camera.worldToCameraMatrix;
        var viewProjectionMatrix = projectionMatrix * viewMatrix;

        // as we use HDRP we cannot rely on CommandBuffer.SetViewProjectionMatrices(...);
        // we need to manually update uniforms
        m_Cmd.SetGlobalMatrix("_ViewMatrix", viewMatrix);
        m_Cmd.SetGlobalMatrix("_InvViewMatrix", viewMatrix.inverse);
        m_Cmd.SetGlobalMatrix("_ProjMatrix", projectionMatrix);
        m_Cmd.SetGlobalMatrix("_InvProjMatrix", projectionMatrix.inverse);
        m_Cmd.SetGlobalMatrix("_ViewProjMatrix", viewProjectionMatrix);
        m_Cmd.SetGlobalMatrix("_InvViewProjMatrix", viewProjectionMatrix.inverse);
        m_Cmd.SetGlobalMatrix("_CameraViewProjMatrix", viewProjectionMatrix);
        m_Cmd.SetGlobalVector("_WorldSpaceCameraPos", Vector3.zero);


        
        // perf does not matter now
        m_Transforms.Clear();
        for (var i = 0; i < m_NumInstances; ++i)
            m_Transforms.Add(Matrix4x4.Translate(
                UnityEngine.Random.onUnitSphere * m_Radius));
        
        m_Cmd.DrawMeshInstanced(m_Mesh, 0, m_Material, 0, m_Transforms.ToArray());
        
        Graphics.ExecuteCommandBuffer(m_Cmd);
    }

    // hdrp specific uniforms
    static void Set(CommandBuffer cmd)
    {
        
    }
}
