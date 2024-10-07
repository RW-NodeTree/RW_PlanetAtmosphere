using RW_PlanetAtmosphere;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace RW_PlanetAtmosphere
{

    public class CommandBufferDepthReciver : MonoBehaviour
    {
        public Camera cam;
        public Shader CopyToDepth;
        public Shader AddToTarget;
        public Shader AtmosphereLUT;
        public Shader TranslucentGenrater;
        public Shader OutSunLightLUTGenrater;
        public Shader InSunLightLUTGenrater;
        public Shader ScatterGenrater;
        public Shader SkyBoxCloud;
        public Shader BasicRing;
        public List<string> texture2DIds = new List<string>();
        public List<Texture2D> texture2DTextures = new List<Texture2D>();
        public List<Def> defs = new List<Def>();
        private List<TransparentObject> objects = new List<TransparentObject>();


        private CommandBuffer commandBufferAfter;
#if UNITY_EDITOR
        private CommandBuffer commandBufferAfter_DevCamear;
#endif

        void Start()
        {
            if (
                cam != null && CopyToDepth != null &&
                AddToTarget != null && AtmosphereLUT != null &&
                TranslucentGenrater != null && OutSunLightLUTGenrater != null &&
                InSunLightLUTGenrater != null && ScatterGenrater != null &&
                BasicRing != null
            )
            {
                cam.depthTextureMode = DepthTextureMode.Depth;
                Application.targetFrameRate = 10000;
                //Debug.Log(Shader.PropertyToID("RW_PlanetAtmosphere_Reflection"));
                objects.Capacity = defs.Count;
                foreach (Def obj in defs)
                {
                    if(obj != null) objects.Add(obj.TransparentObject);
                }
#if UNITY_EDITOR
                Debug.Log(PlayerSettings.MTRendering);
                Debug.Log(SystemInfo.renderingThreadingMode);
                //PlayerSettings.MTRendering = true;
#endif

                TransparentObject.shaders.Add("Assets/RW_PlanetAtmosphere/Shader/CopyToDepth.shader", CopyToDepth);
                TransparentObject.shaders.Add("Assets/RW_PlanetAtmosphere/Shader/AddToTarget.shader", AddToTarget);
                TransparentObject.shaders.Add("Assets/RW_PlanetAtmosphere/Shader/Atmosphere/AtmosphereLUT.shader", AtmosphereLUT);
                TransparentObject.shaders.Add("Assets/RW_PlanetAtmosphere/Shader/TranslucentGenrater.shader", TranslucentGenrater);
                TransparentObject.shaders.Add("Assets/RW_PlanetAtmosphere/Shader/OutSunLightLUTGenrater.shader", OutSunLightLUTGenrater);
                TransparentObject.shaders.Add("Assets/RW_PlanetAtmosphere/Shader/InSunLightLUTGenrater.shader", InSunLightLUTGenrater);
                TransparentObject.shaders.Add("Assets/RW_PlanetAtmosphere/Shader/ScatterGenrater.shader", ScatterGenrater);
                TransparentObject.shaders.Add("Assets/RW_PlanetAtmosphere/Shader/Cloud/SkyBoxCloud.shader", SkyBoxCloud);
                TransparentObject.shaders.Add("Assets/RW_PlanetAtmosphere/Shader/Ring/BasicRing.shader", BasicRing);
                for (int i = 0; i < texture2DIds.Count && i < texture2DTextures.Count; i++)
                {
                    TransparentObject.texture2Ds.Add(texture2DIds[i], texture2DTextures[i]);
                }

                commandBufferAfter = new CommandBuffer();
                commandBufferAfter.name = "commandBufferAfter";
                cam.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBufferAfter);

#if UNITY_EDITOR
                Camera camera = SceneView.lastActiveSceneView.camera;
                if (camera != null)
                {
                    commandBufferAfter_DevCamear = new CommandBuffer();
                    commandBufferAfter_DevCamear.name = "commandBufferAfter_DevCamear";
                    camera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBufferAfter_DevCamear);
                }
#endif
                //cam.AddCommandBuffer(CameraEvent.AfterEverything, commandBufferAfter);

                // RenderSettings.ambientLight = Color.black;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (objects != null)
            {
                if(cam != null && commandBufferAfter != null) TransparentObject.DrawTransparentObjects(objects, commandBufferAfter, cam);
#if UNITY_EDITOR
                Camera camera = SceneView.lastActiveSceneView.camera;
                if (camera != null && commandBufferAfter_DevCamear != null) TransparentObject.DrawTransparentObjects(objects, commandBufferAfter_DevCamear, camera);
#endif
            }
        }

        //void OnPreRender()
        //{
        //    if (targetC == null || targetC.width != Screen.width || targetC.height != Screen.height)
        //    {   
        //        if (targetC != null) Destroy(targetC);
        //        targetC = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat);
        //        targetC.name = "custom color";
        //    }
        //    if (targetD == null || targetD.width != Screen.width || targetD.height != Screen.height)
        //    {
        //        if (targetD != null) Destroy(targetD);
        //        targetD = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
        //        targetD.name = "custom depth";
        //    }
        //    //cam.depthTextureMode = DepthTextureMode.Depth;
        //    cam.SetTargetBuffers(targetC.colorBuffer, targetD.depthBuffer);
        //}

        //void OnRenderImage(RenderTexture source, RenderTexture destination)
        //{
        //    if(materialTest) Graphics.Blit(source, destination, materialTest);
        //    else Graphics.Blit(source, destination);
        //}

        private void OnDestroy()
        {
            if (cam != null && commandBufferAfter != null)
            {
                cam.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, commandBufferAfter);
            }
#if UNITY_EDITOR
            Camera camera = SceneView.lastActiveSceneView.camera;
            if (camera != null && commandBufferAfter_DevCamear != null)
            {
                camera.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, commandBufferAfter_DevCamear);
            }
#endif
        }

        void OnGUI()
        {
            //GUI.DrawTexture(new Rect(0, 0, targetD.width, targetD.height), targetD);
            // GUI.DrawTexture(new Rect(0, translucentLUT.height, scatterLUT.width, scatterLUT.height), scatterLUT);
            //GUI.Label(new Rect(0, 0, 128, 32), 1f / Time.deltaTime + "FPS");
            if (Input.GetKeyUp(KeyCode.S) && Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
                ScreenCapture.CaptureScreenshot($"Screenshot-{DateTime.Now.ToString("yyyy-MM-dd-")}{DateTime.Now.Ticks - DateTime.Today.Ticks}.png");
        }
    }
}