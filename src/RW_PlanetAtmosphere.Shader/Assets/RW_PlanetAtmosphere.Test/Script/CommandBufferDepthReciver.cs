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
        public float sunRadius;
        public float sunDistance;
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
        public Shader SunFlear;
        public Texture2D sunFlareTexture;
        public List<string> texture2DIds = new List<string>();
        public List<Texture2D> texture2DTextures = new List<Texture2D>();
        public List<Def> defs = new List<Def>();


        private Material materialSunFlear;
        private CommandBuffer commandBufferAfter;
#if UNITY_EDITOR
        private CommandBuffer commandBufferAfter_DevCamear;
#endif
        private List<TransparentObject> objects = new List<TransparentObject>();
        
        private static readonly int propId_sunRadius            = Shader.PropertyToID("sunRadius");
        private static readonly int propId_sunDistance          = Shader.PropertyToID("sunDistance");
        private static readonly int propId_sunFlareTexture      = Shader.PropertyToID("sunFlareTexture");
        private static readonly int propId_backgroundTexture    = Shader.PropertyToID("backgroundTexture");

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

                if (SunFlear != null && sunFlareTexture != null)
                {
                    materialSunFlear = new Material(SunFlear);
                    materialSunFlear.SetFloat(propId_sunRadius, sunRadius);
                    materialSunFlear.SetFloat(propId_sunDistance, sunDistance);
                    materialSunFlear.SetTexture(propId_sunFlareTexture, sunFlareTexture);
                }

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
                void BeforeShadow(CommandBuffer cb)
                {
                    if (materialSunFlear)
                    {
                        cb.DrawMesh(TransparentObject.DefaultRenderingMesh, Matrix4x4.identity, materialSunFlear, 0, 0);
                    }
                }
                void AfterTrans(CommandBuffer cb)
                {
                    if (materialSunFlear)
                    {
                        cb.GetTemporaryRT(propId_backgroundTexture, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
                        cb.Blit(BuiltinRenderTextureType.CameraTarget, propId_backgroundTexture);
                    }
                }
                if (cam != null && commandBufferAfter != null)
                {
                    TransparentObject.DrawTransparentObjects(objects, commandBufferAfter, cam, BeforeShadow, null, AfterTrans);
                    if (materialSunFlear)
                    {
                        commandBufferAfter.DrawMesh(TransparentObject.DefaultRenderingMesh, Matrix4x4.identity, materialSunFlear, 0, 1);
                        commandBufferAfter.ReleaseTemporaryRT(propId_backgroundTexture);
                    }
                }
#if UNITY_EDITOR
                Camera camera = SceneView.lastActiveSceneView.camera;
                if (camera != null && commandBufferAfter_DevCamear != null)
                {
                    TransparentObject.DrawTransparentObjects(objects, commandBufferAfter_DevCamear, camera, BeforeShadow, null, AfterTrans);
                    if(materialSunFlear)
                    {
                        commandBufferAfter_DevCamear.DrawMesh(TransparentObject.DefaultRenderingMesh, Matrix4x4.identity, materialSunFlear, 0, 1);
                        commandBufferAfter_DevCamear.ReleaseTemporaryRT(propId_backgroundTexture);
                    }
                }
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