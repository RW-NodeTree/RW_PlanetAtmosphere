using RW_PlanetAtmosphere;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SocialPlatforms;

namespace RW_PlanetAtmosphere
{

    public class CommandBufferDepthReciver : MonoBehaviour
    {
        public enum TonemapType
        {
            SEUSTonemap = 0,
            HableTonemap = 1,
            UchimuraTonemap = 2,
            ACESTonemap = 3,
            ACESTonemap2 = 4,
            LottesTonemap = 5,
        }

        public bool updateCommandBuffer = true;
        public float textRoat = 0;
        public float gamma = 1;
        public float sunRadius;
        public float sunDistance;
        public float refraction = 1;
        public float luminescen = 0;
        public float planetRadius = 63.71393f;
        public TonemapType tonemapType;
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
        public Shader Tonemaps;
        public Shader WriteDepth;
        public Shader RemoveAlpha;
        public Texture2D sunFlareTexture;
        public RenderTexture screenTexture;
        public List<string> texture2DIds = new List<string>();
        public List<Texture2D> texture2DTextures = new List<Texture2D>();
        public List<Def> defs = new List<Def>();


        private Material materialSunFlear;
        private Material materialTonemaps;
        private Material materialWriteDepth;
        private Material materialRemoveAlpha;
        private CommandBuffer commandBufferDepth;
        private CommandBuffer commandBufferAfter;
#if UNITY_EDITOR
        private CommandBuffer commandBufferDepth_DevCamear;
        private CommandBuffer commandBufferAfter_DevCamear;
#endif
        private List<TransparentObject> objects = new List<TransparentObject>();
        
        private static readonly int propId_gamma                = Shader.PropertyToID("gamma");
        private static readonly int propId_radius               = Shader.PropertyToID("radius");
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

                if (Tonemaps != null)
                {
                    materialTonemaps = new Material(Tonemaps);
                    materialTonemaps.SetFloat(propId_gamma, gamma);
                }

                if (WriteDepth != null)
                {
                    materialWriteDepth = new Material(WriteDepth);
                    materialWriteDepth.SetFloat(propId_radius, planetRadius);
                }

                if (RemoveAlpha != null)
                {
                    materialRemoveAlpha = new Material(RemoveAlpha);
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
                commandBufferDepth = new CommandBuffer();
                commandBufferDepth.name = "commandBufferDepth";

                cam.AddCommandBuffer(CameraEvent.AfterDepthTexture, commandBufferDepth);
                cam.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBufferAfter);

#if UNITY_EDITOR
                Camera camera = SceneView.lastActiveSceneView?.camera;
                if (camera != null)
                {
                    commandBufferAfter_DevCamear = new CommandBuffer();
                    commandBufferAfter_DevCamear.name = "commandBufferAfter_DevCamear";
                    commandBufferDepth_DevCamear = new CommandBuffer();
                    commandBufferDepth_DevCamear.name = "commandBufferDepth";
                    camera.AddCommandBuffer(CameraEvent.AfterDepthTexture, commandBufferDepth_DevCamear);
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
            if (cam != null) cam.targetTexture = screenTexture;
            if (objects != null && updateCommandBuffer)
            {
                void BeforeShadow(CommandBuffer cb)
                {
                    cb.GetTemporaryRT(propId_backgroundTexture, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
                    cb.Blit(BuiltinRenderTextureType.CameraTarget, propId_backgroundTexture);
                    if (materialRemoveAlpha)
                    {
                        cb.Blit(null, BuiltinRenderTextureType.CameraTarget, materialRemoveAlpha, 0);
                        cb.Blit(BuiltinRenderTextureType.CameraTarget, propId_backgroundTexture);
                        // cb.ReleaseTemporaryRT(propId_backgroundTexture);
                    }
                    if(refraction != 1)
                    {
                        Color color = new Color(refraction, refraction, refraction, 1);
                        cb.SetGlobalTexture(TransparentObject.ColorTex, propId_backgroundTexture);
                        cb.SetGlobalColor(TransparentObject.MainColor, color);
                        cb.Blit(null, BuiltinRenderTextureType.CameraTarget, TransparentObject.AddToTargetMaterial, 2);
                    }
                    if (materialSunFlear)
                    {
                        cb.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                        cb.DrawMesh(TransparentObject.DefaultRenderingMesh, Matrix4x4.identity, materialSunFlear, 0, 0);
                    }
                    if (materialWriteDepth)
                    {
                        cb.SetRenderTarget( BuiltinRenderTextureType.CameraTarget);
                        cb.DrawMesh(TransparentObject.DefaultRenderingMesh, Matrix4x4.identity, materialWriteDepth, 0, 1);
                    }
                }
                void BackgroundBlendLumen(CommandBuffer cb)
                {
                    if(luminescen != 0)
                    {
                        Color color = new Color(luminescen, luminescen, luminescen, 0);
                        cb.SetGlobalTexture(TransparentObject.ColorTex, propId_backgroundTexture);
                        cb.SetGlobalColor(TransparentObject.MainColor, color);
                        cb.Blit(null, BuiltinRenderTextureType.CameraTarget, TransparentObject.AddToTargetMaterial, 3);
                    }
                    cb.ReleaseTemporaryRT(propId_backgroundTexture);
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
                    if (materialWriteDepth && commandBufferDepth != null)
                    {
                        commandBufferDepth.Clear();
                        //commandBufferAfter.SetRenderTarget(BuiltinRenderTextureType.Depth, BuiltinRenderTextureType.CameraTarget);
                        commandBufferDepth.DrawMesh(TransparentObject.DefaultRenderingMesh, Matrix4x4.identity, materialWriteDepth, 0, 0);
                    }
                    TransparentObject.DrawTransparentObjects(objects, commandBufferAfter, cam, BeforeShadow, BackgroundBlendLumen, AfterTrans);
                    if (materialSunFlear)
                    {
                        commandBufferAfter.DrawMesh(TransparentObject.DefaultRenderingMesh, Matrix4x4.identity, materialSunFlear, 0, 1);
                        commandBufferAfter.ReleaseTemporaryRT(propId_backgroundTexture);
                    }
                    if (materialTonemaps)
                    {
                        materialTonemaps.SetFloat(propId_gamma, gamma);
                        commandBufferAfter.GetTemporaryRT(propId_backgroundTexture, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
                        commandBufferAfter.Blit(BuiltinRenderTextureType.CameraTarget, propId_backgroundTexture);
                        //commandBufferAfter.SetGlobalTexture(propId_backgroundTexture, BuiltinRenderTextureType.CameraTarget);
                        commandBufferAfter.Blit(null, BuiltinRenderTextureType.CameraTarget, materialTonemaps,(int)tonemapType);
                        commandBufferAfter.ReleaseTemporaryRT(propId_backgroundTexture);
                    }
                }
#if UNITY_EDITOR
                Camera camera = SceneView.lastActiveSceneView?.camera;
                if (camera != null && commandBufferAfter_DevCamear != null)
                {
                    if (materialWriteDepth && commandBufferDepth_DevCamear != null)
                    {
                        commandBufferDepth_DevCamear.Clear();
                        //commandBufferAfter.SetRenderTarget(BuiltinRenderTextureType.Depth, BuiltinRenderTextureType.CameraTarget);
                        commandBufferDepth_DevCamear.DrawMesh(TransparentObject.DefaultRenderingMesh, Matrix4x4.identity, materialWriteDepth, 0, 0);
                    }
                    TransparentObject.DrawTransparentObjects(objects, commandBufferAfter_DevCamear, camera, BeforeShadow, BackgroundBlendLumen, AfterTrans);
                    if(materialSunFlear)
                    {
                        commandBufferAfter_DevCamear.DrawMesh(TransparentObject.DefaultRenderingMesh, Matrix4x4.identity, materialSunFlear, 0, 1);
                        commandBufferAfter_DevCamear.ReleaseTemporaryRT(propId_backgroundTexture);
                    }
                    if (materialTonemaps)
                    {
                        commandBufferAfter_DevCamear.GetTemporaryRT(propId_backgroundTexture, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
                        commandBufferAfter_DevCamear.Blit(BuiltinRenderTextureType.CameraTarget, propId_backgroundTexture);
                        //commandBufferAfter_DevCamear.SetGlobalTexture(propId_backgroundTexture, BuiltinRenderTextureType.CameraTarget);
                        commandBufferAfter_DevCamear.Blit(null, BuiltinRenderTextureType.CameraTarget, materialTonemaps, (int)tonemapType);
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
            Camera camera = SceneView.lastActiveSceneView?.camera;
            if (camera != null && commandBufferAfter_DevCamear != null)
            {
                camera.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, commandBufferAfter_DevCamear);
            }
#endif
        }
        public static void GUIEnum<T>(ref float posY, T value, string name, float width, Vector2 outFromTo, Action<T> setter, float sizeY = 32) where T : struct, Enum
        {
            if (
                posY < outFromTo.y &&
                outFromTo.x < posY + sizeY
            )
            {
                GUI.Label(new Rect(0, posY, width * 0.5f, sizeY), name);
                if (GUI.Button(new Rect(width * 0.5f, posY, width * 0.5f, sizeY), value.ToString()))
                {
                    Array all = Enum.GetValues(typeof(T));
                    foreach (T val in all)
                    {
                        Debug.Log(val.ToString());
                    }
                }
            }
            posY += sizeY;
        }

        void OnGUI()
        {
            if (screenTexture) GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), screenTexture);
            //GUI.DrawTexture(new Rect(0, 0, targetD.width, targetD.height), targetD);
            // GUI.DrawTexture(new Rect(0, translucentLUT.height, scatterLUT.width, scatterLUT.height), scatterLUT);
            //GUI.Label(new Rect(0, 0, 128, 32), 1f / Time.deltaTime + "FPS");
            if (Input.GetKeyUp(KeyCode.S) && Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
                ScreenCapture.CaptureScreenshot($"Screenshot-{DateTime.Now.ToString("yyyy-MM-dd-")}{DateTime.Now.Ticks - DateTime.Today.Ticks}.png");

            //Matrix4x4 matrix = GUI.matrix;
            //Rect dropDownMark = new Rect(0,0,512,512);
            //GUIUtility.RotateAroundPivot(textRoat, dropDownMark.center);
            //GUI.skin.button.richText = true;
            //GUI.skin.button.fontSize = 512;
            //GUI.Button(dropDownMark, "▼");
            //GUI.matrix = matrix;
            //GUI.skin.button.fontSize = 16;
            //GUI.skin.label.fontSize = 16;
            //float posY = 256;
            //GUIEnum(ref posY, tonemapType, "tonemap type", 512, new Vector2(posY, 512), null);
        }
    }
}