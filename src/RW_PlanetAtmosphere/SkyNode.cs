using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;
using Verse.Noise;
using Verse.AI.Group;
using System.Runtime.InteropServices;
using System;
using UnityEngine.Rendering;

namespace RW_PlanetAtmosphere
{
    public abstract class SkyNode : IExposable
    {
        public abstract float CameraDepth(Vector3 cameraPos);//camera looking for certer(0,0,0)
        public abstract float LightDepth(Vector3 lightDir);

        public abstract void UpdateNode();
        public abstract void ApplyShadow(SkyNode other);
        public abstract void ExposeData();
    }

    public class SkyNode_Cloud : SkyNode
    {
        public float height;
        public override void ApplyShadow(SkyNode other)
        {
        }

        public override float CameraDepth(Vector3 cameraPos)
        {
            height = Math.Abs(height);
            float cameraHeight = cameraPos.magnitude;
            if(cameraHeight > height)
            {
                return cameraHeight - height;
            }
            return cameraHeight + height;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref height, "height");
        }

        public override float LightDepth(Vector3 lightDir)
        {
            return height;
        }

        public override void UpdateNode()
        {
            
        }
    }

    public class CommandBufferDepthReciver : MonoBehaviour
    {
        public Camera camera;
        public Mesh meshTest;
        public Material materialTest;
        public Vector3 pos;
        private RenderTexture target;
        private CommandBuffer commandBufferBefore;
        private CommandBuffer commandBufferAfter;
        private RenderTargetIdentifier cacheColor;
        private RenderTargetIdentifier cacheDepth;
        private RenderTargetIdentifier targetId;
        void Start()
        {
            if(camera != null)
            {
                target = new RenderTexture(1024,1024,32);
                target.useMipMap = false;
                target.format = RenderTextureFormat.RFloat;
                cacheColor = new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive);
                cacheDepth = new RenderTargetIdentifier(BuiltinRenderTextureType.Depth);
                targetId = new RenderTargetIdentifier(target);
                
                commandBufferAfter = new CommandBuffer();
                commandBufferAfter.Blit(cacheDepth,targetId);
                camera.AddCommandBuffer(CameraEvent.AfterForwardAlpha,commandBufferAfter);

                commandBufferBefore = new CommandBuffer();
                commandBufferBefore.SetRenderTarget(cacheColor,cacheDepth);
                camera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, commandBufferBefore);

                // RenderSettings.ambientLight = Color.black;
            }
        }

        // Update is called once per frame
        void Update()
        {
            // if(TranslucentGenrater != null && ScatterGenrater != null && materialSkyLUT != null && materialCloudLUT != null && materialCloudGenrater != null)
            // {
            //     Graphics.Blit(null, atmosphereInfoMapCache, materialCloudGenrater);
            //     Graphics.Blit(atmosphereInfoMapCache, atmosphereInfoMap);
            // }
            if(meshTest != null && materialTest != null)
            {
                Graphics.DrawMesh(meshTest,Matrix4x4.Translate(pos),materialTest,0);
                for(int i = 0; i < materialTest.passCount; i++)
                {
                    Debug.Log(materialTest.GetPassName(i));
                }
                Debug.Log(materialTest.shader.passCount);
            }
        }

        void OnGUI()
        {
            GUI.DrawTexture(new Rect(0, 0, target.width, target.height), target);
            // GUI.DrawTexture(new Rect(0, translucentLUT.height, scatterLUT.width, scatterLUT.height), scatterLUT);
        }
    }
}