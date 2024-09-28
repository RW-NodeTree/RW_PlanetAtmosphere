using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CommandBufferDepthReciver : MonoBehaviour
{
    public Camera cam;
    //public Mesh meshTest;
    public Material materialTest;
    public Vector3 pos;
    private RenderTexture targetC;
    private RenderTexture targetD;
    private CommandBuffer commandBufferBefore;
    private CommandBuffer commandBufferAfter;
    private RenderTargetIdentifier cacheColor;
    private RenderTargetIdentifier cacheDepth;
    private RenderTargetIdentifier targetId;
    void Start()
    {
        //if(cam != null)
        //{
        //    target = new RenderTexture(1024,1024,32);
        //    target.useMipMap = false;
        //    target.format = RenderTextureFormat.RFloat;
        //    cacheColor = new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive);
        //    cacheDepth = new RenderTargetIdentifier(BuiltinRenderTextureType.Depth);
        //    targetId = new RenderTargetIdentifier(target);

        //    commandBufferBefore = new CommandBuffer();
        //    commandBufferBefore.SetRenderTarget(cacheColor,cacheDepth);
        //    cam.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, commandBufferBefore);

        //    commandBufferAfter = new CommandBuffer();
        //    commandBufferAfter.Blit(cacheDepth,targetId);
        //    cam.AddCommandBuffer(CameraEvent.AfterForwardAlpha,commandBufferAfter);

        //    // RenderSettings.ambientLight = Color.black;
        //}
    }

    // Update is called once per frame
    void Update()
    {
        // if(TranslucentGenrater != null && ScatterGenrater != null && materialSkyLUT != null && materialCloudLUT != null && materialCloudGenrater != null)
        // {
        //     Graphics.Blit(null, atmosphereInfoMapCache, materialCloudGenrater);
        //     Graphics.Blit(atmosphereInfoMapCache, atmosphereInfoMap);
        // }
        //if(meshTest != null && materialTest != null)
        //{
        //    Graphics.DrawMesh(meshTest,Matrix4x4.Translate(pos),materialTest,0);
        //    for(int i = 0; i < materialTest.passCount; i++)
        //    {
        //        Debug.Log(materialTest.GetPassName(i));
        //    }
        //    Debug.Log(materialTest.shader.passCount);
        //}
    }

    void OnPreRender()
    {
        if (targetC == null || targetC.width != Screen.width || targetC.height != Screen.height)
        {   
            if (targetC != null) Destroy(targetC);
            targetC = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat);
            targetC.name = "custom color";
        }
        if (targetD == null || targetD.width != Screen.width || targetD.height != Screen.height)
        {
            if (targetD != null) Destroy(targetD);
            targetD = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
            targetD.name = "custom depth";
        }
        //cam.depthTextureMode = DepthTextureMode.Depth;
        cam.SetTargetBuffers(targetC.colorBuffer, targetD.depthBuffer);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(materialTest) Graphics.Blit(targetD, destination, materialTest);
        else Graphics.Blit(targetD, destination);
    }


    void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, targetD.width, targetD.height), targetD);
        // GUI.DrawTexture(new Rect(0, translucentLUT.height, scatterLUT.width, scatterLUT.height), scatterLUT);
    }
}