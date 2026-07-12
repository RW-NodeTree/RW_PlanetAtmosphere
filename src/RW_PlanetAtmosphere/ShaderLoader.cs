using System;
using System.Data;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
#if !UNITY
using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;
#endif

namespace RW_PlanetAtmosphere
{
#if !UNITY
    [StaticConstructorOnStartup]
#endif
    public static class ShaderLoader
    {

        #region propsIDs

        public static readonly int propId_gamma             = Shader.PropertyToID("gamma");
        public static readonly int propId_radius            = Shader.PropertyToID("radius");
        // public static readonly int propId_sunRadius         = Shader.PropertyToID("sunRadius");
        // public static readonly int propId_sunDistance       = Shader.PropertyToID("sunDistance");
        public static readonly int propId_sunFlareTexture   = Shader.PropertyToID("sunFlareTexture");
        public static readonly int propId_backgroundTexture = Shader.PropertyToID("backgroundTexture");

        #endregion

        internal static RenderTexture cameraOverride;

        private static Material materialSunFlear;
        private static Material materialTonemaps;
        private static Material materialWriteDepth;
        private static Material materialRemoveAlpha;
        private static Texture2D sunFlareTexture;
        
#if UNITY
        public static bool StaticConstructorTriger = true;
#endif
        static ShaderLoader()
        {
#if UNITY
            AtmosphereSettings.Current.TargetCamera.depthTextureMode = DepthTextureMode.Depth;
            AtmosphereSettings.Current.TargetCamera.gameObject.AddComponent<PlanetAtmosphereRenderer>();
#else
            WorldCameraManager.WorldCamera.depthTextureMode = DepthTextureMode.Depth;
            WorldCameraManager.WorldCamera.gameObject.AddComponent<PlanetAtmosphereRenderer>();
            WorldCameraManager.WorldSkyboxCamera.depthTextureMode = DepthTextureMode.Depth;
            WorldCameraManager.WorldSkyboxCamera.backgroundColor = Color.black;
            

            // WorldMaterials.Rivers.shader = WorldMaterials.WorldTerrain.shader;
            // WorldMaterials.WorldOcean.shader = WorldMaterials.WorldTerrain.shader;
            // WorldMaterials.RiversBorder.shader = WorldMaterials.WorldTerrain.shader;
            // WorldMaterials.UngeneratedPlanetParts.shader = WorldMaterials.WorldTerrain.shader;

            // WorldMaterials.Rivers.renderQueue = 3530;
            // WorldMaterials.WorldOcean.renderQueue = 3500;
            // WorldMaterials.RiversBorder.renderQueue = 3520;
            // WorldMaterials.UngeneratedPlanetParts.renderQueue = 3500;

            // WorldMaterials.Rivers.color = new Color(-65536,-65536,-65536,1);
            // WorldMaterials.RiversBorder.color = new Color(0,0,0,0);


            // WorldMaterials.Rivers.mainTexture = ContentFinder<Texture2D>.Get("TerrainReplace/Water");
            // WorldMaterials.WorldOcean.mainTexture = ContentFinder<Texture2D>.Get("TerrainReplace/Water");
            // WorldMaterials.RiversBorder.mainTexture = ContentFinder<Texture2D>.Get("TerrainReplace/Water");
            // WorldMaterials.UngeneratedPlanetParts.mainTexture = ContentFinder<Texture2D>.Get("TerrainReplace/Water");
#endif

        }

        static void checkAndUpdate()
        {
            AtmosphereSettings current = AtmosphereSettings.Current;
            if(current.needUpdate)
            {
                current.needUpdate = false;
                sunFlareTexture = TransparentObject.GetTexture2D(current.sunFlareTexturePath);
                Shader
                shader = TransparentObject.GetShader(@"Assets/RW_PlanetAtmosphere/Resources/Shader/SunFlare.shader");
                if(shader)
                {
                    materialSunFlear = new Material(shader);
                    materialSunFlear.SetTexture(propId_sunFlareTexture, sunFlareTexture);
                }
                shader = TransparentObject.GetShader(@"Assets/RW_PlanetAtmosphere/Resources/Shader/Tonemaps.shader");
                if(shader)
                {
                    materialTonemaps = new Material(shader);
                    materialTonemaps.SetFloat(propId_gamma, current.gamma);
                }
                shader = TransparentObject.GetShader(@"Assets/RW_PlanetAtmosphere/Resources/Shader/WriteDepth.shader");
                if(shader)
                {
                    materialWriteDepth = new Material(shader);
                    materialWriteDepth.SetFloat(propId_radius,current.planetRadius);
                }
                shader = TransparentObject.GetShader(@"Assets/RW_PlanetAtmosphere/Resources/Shader/RemoveAlpha.shader");
                if(shader) materialRemoveAlpha = new Material(shader);
            }
            if(current.renderingSizeFactor != 1)
            {
                int width = (int)(Screen.width * current.renderingSizeFactor);
                int height = (int)(Screen.height * current.renderingSizeFactor);
                if(cameraOverride == null || cameraOverride.width != width || cameraOverride.height != height)
                {
                    if(cameraOverride != null) GameObject.Destroy(cameraOverride);
                    cameraOverride = new RenderTexture(width,height,24,RenderTextureFormat.ARGBFloat)
                    {
                        useMipMap = false
                    };
                    cameraOverride.Create();
                }
            }
            else
            {
                if(cameraOverride != null) GameObject.Destroy(cameraOverride);
                cameraOverride = null;
            }
#if UNITY
            current.TargetCamera.targetTexture = cameraOverride;
#else
            WorldCameraManager.WorldCamera.targetTexture = cameraOverride;
            WorldCameraManager.WorldSkyboxCamera.targetTexture = cameraOverride;
#endif
        }


        private class PlanetAtmosphereRenderer : MonoBehaviour
        {
            // CommandBuffer commandBufferBeforeTransparent;
            float luminescenVel = 0;
            float targetLuminescen = 0;
            float refractionVel = 0;
            float targetRefraction = 0;
            CommandBuffer commandBufferAfterDepth;
            CommandBuffer commandBufferAfterAlpha;
#if UNITY
            CommandBuffer commandBufferAfterDepth_EditorCamera;
            CommandBuffer commandBufferAfterAlpha_EditorCamera;
#endif
            // public readonly List<Material> materialsTest = new List<Material>();
            // private Transform cachedTransform = null;
            void Start()
            {
                AtmosphereSettings current = AtmosphereSettings.Current;
                commandBufferAfterDepth = new CommandBuffer();
                commandBufferAfterDepth.name = "RW_PlanetAtmosphere.AfterDepth";
                commandBufferAfterAlpha = new CommandBuffer();
                commandBufferAfterAlpha.name = "RW_PlanetAtmosphere.AfterAlpha";
#if UNITY
                current.TargetCamera.AddCommandBuffer(CameraEvent.AfterDepthTexture,commandBufferAfterDepth);
                current.TargetCamera.AddCommandBuffer(CameraEvent.AfterForwardAlpha,commandBufferAfterAlpha);
#else
                Find.WorldCamera.AddCommandBuffer(CameraEvent.AfterDepthTexture,commandBufferAfterDepth);
                Find.WorldCamera.AddCommandBuffer(CameraEvent.AfterForwardAlpha,commandBufferAfterAlpha);
#endif
            }
            void Update()
            {
                AtmosphereSettings current = AtmosphereSettings.Current;
#if !UNITY
                if (Find.World != null)
                {
                    Shader.SetGlobalVector("_WorldSpaceLightPos0",GenCelestial.CurSunPositionInWorldSpace());
                    Shader.SetGlobalVector("_LightColor0",current.sunColor);
                }
#endif
                commandBufferAfterDepth.Clear();
                commandBufferAfterAlpha.Clear();
#if !UNITY
    #if V13 || V14 || V15
                if(Find.World?.renderer == null || !WorldRendererUtility.WorldRenderedNow) return;
    #else
                if(Find.World?.renderer == null || !WorldRendererUtility.WorldRendered) return;
    #endif
                if(Find.PlaySettings.usePlanetDayNightSystem)
#endif
                    
                {
                    checkAndUpdate();
                    current.refraction = Math.Abs(current.refraction);
                    current.luminescen = Math.Abs(current.luminescen);
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
                        targetRefraction = TransparentObject.LuminescenTransaction(targetRefraction, current.refraction, current.luminescen, ref refractionVel);
                        if (targetRefraction != 1)
                        {
                            Color color = new Color(targetRefraction, targetRefraction, targetRefraction, 1);
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
                            cb.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                            cb.DrawMesh(TransparentObject.DefaultRenderingMesh,Matrix4x4.identity,materialWriteDepth, 0, 1);
                        }
                    }
                    void BackgroundBlendLumen(CommandBuffer cb)
                    {
                        
                        targetLuminescen = TransparentObject.LuminescenTransaction(targetLuminescen, current.luminescen, current.refraction, ref luminescenVel);
                        if (targetLuminescen != 0)
                        {
                            Color color = new Color(targetLuminescen, targetLuminescen, targetLuminescen, 0);
                            cb.SetGlobalTexture(TransparentObject.ColorTex, propId_backgroundTexture);
                            cb.SetGlobalColor(TransparentObject.MainColor, color);
                            cb.Blit(null, BuiltinRenderTextureType.CameraTarget, TransparentObject.AddToTargetMaterial, 3);
                        }
                        cb.ReleaseTemporaryRT(propId_backgroundTexture);
                    }
                    void AfterTrans(CommandBuffer cb)
                    {
                        if (!materialSunFlear) return;
                        cb.GetTemporaryRT(propId_backgroundTexture, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
                        cb.Blit(BuiltinRenderTextureType.CameraTarget, propId_backgroundTexture);
                    }
                    if (materialWriteDepth)
                    {
                        commandBufferAfterDepth.DrawMesh(TransparentObject.DefaultRenderingMesh,Matrix4x4.identity,materialWriteDepth, 0, 0);
                    }
                    TransparentObject.sunRadius = current.sunRadius;
                    TransparentObject.sunDistance = current.sunDistance;
#if UNITY
                    TransparentObject.DrawTransparentObjects(current.objects, commandBufferAfterAlpha, current.TargetCamera, BeforeShadow, BackgroundBlendLumen, AfterTrans);
#else
                    TransparentObject.DrawTransparentObjects(current.objects, commandBufferAfterAlpha, WorldCameraManager.WorldCamera, BeforeShadow, BackgroundBlendLumen, AfterTrans);
#endif
                    if (materialSunFlear)
                    {
                        commandBufferAfterAlpha.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                        commandBufferAfterAlpha.DrawMesh(TransparentObject.DefaultRenderingMesh, Matrix4x4.identity, materialSunFlear, 0, 1);
                        commandBufferAfterAlpha.ReleaseTemporaryRT(propId_backgroundTexture);
                    }
                    if (materialTonemaps)
                    {
                        materialTonemaps.SetFloat(propId_gamma, current.gamma);
                        commandBufferAfterAlpha.GetTemporaryRT(propId_backgroundTexture, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
                        commandBufferAfterAlpha.Blit(BuiltinRenderTextureType.CameraTarget, propId_backgroundTexture);
                        //commandBufferAfter.SetGlobalTexture(propId_backgroundTexture, BuiltinRenderTextureType.CameraTarget);
                        commandBufferAfterAlpha.Blit(null, BuiltinRenderTextureType.CameraTarget, materialTonemaps,(int)current.tonemapType);
                        commandBufferAfterAlpha.ReleaseTemporaryRT(propId_backgroundTexture);
                    }
                    commandBufferAfterAlpha.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
#if UNITY
                    if (current.EditorCamera)
                    {
                        if (commandBufferAfterDepth_EditorCamera == null)
                        {
                            commandBufferAfterDepth_EditorCamera = new CommandBuffer();
                            commandBufferAfterDepth_EditorCamera.name = "RW_PlanetAtmosphere.AfterDepth.EditorCamera";
                            current.EditorCamera.AddCommandBuffer(CameraEvent.AfterDepthTexture,commandBufferAfterDepth_EditorCamera);
                        }
                        if (commandBufferAfterAlpha_EditorCamera == null)
                        {
                            commandBufferAfterAlpha_EditorCamera = new CommandBuffer();
                            commandBufferAfterAlpha_EditorCamera.name = "RW_PlanetAtmosphere.AfterAlpha.EditorCamera";
                            current.EditorCamera.AddCommandBuffer(CameraEvent.AfterForwardAlpha,commandBufferAfterAlpha_EditorCamera);
                        }
                        commandBufferAfterDepth_EditorCamera.Clear();
                        commandBufferAfterAlpha_EditorCamera.Clear();
                        if (materialWriteDepth)
                        {
                            commandBufferAfterDepth_EditorCamera.DrawMesh(TransparentObject.DefaultRenderingMesh,Matrix4x4.identity,materialWriteDepth, 0, 0);
                        }
                        TransparentObject.DrawTransparentObjects(current.objects, commandBufferAfterAlpha_EditorCamera, current.TargetCamera, BeforeShadow, BackgroundBlendLumen, AfterTrans);
                        if (materialSunFlear)
                        {
                            commandBufferAfterAlpha_EditorCamera.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                            commandBufferAfterAlpha_EditorCamera.DrawMesh(TransparentObject.DefaultRenderingMesh, Matrix4x4.identity, materialSunFlear, 0, 1);
                            commandBufferAfterAlpha_EditorCamera.ReleaseTemporaryRT(propId_backgroundTexture);
                        }
                        if (materialTonemaps)
                        {
                            materialTonemaps.SetFloat(propId_gamma, current.gamma);
                            commandBufferAfterAlpha_EditorCamera.GetTemporaryRT(propId_backgroundTexture, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
                            commandBufferAfterAlpha_EditorCamera.Blit(BuiltinRenderTextureType.CameraTarget, propId_backgroundTexture);
                            //commandBufferAfter.SetGlobalTexture(propId_backgroundTexture, BuiltinRenderTextureType.CameraTarget);
                            commandBufferAfterAlpha_EditorCamera.Blit(null, BuiltinRenderTextureType.CameraTarget, materialTonemaps,(int)current.tonemapType);
                            commandBufferAfterAlpha_EditorCamera.ReleaseTemporaryRT(propId_backgroundTexture);
                        }
                        commandBufferAfterAlpha_EditorCamera.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                    }
#endif

                    
                }
            }

        }
    }
}