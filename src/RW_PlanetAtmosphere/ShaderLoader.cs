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
using System.Data;
using UnityEngine.Rendering;

namespace RW_PlanetAtmosphere
{
    [StaticConstructorOnStartup]
    public static class ShaderLoader
    {

        #region propsIDs

        public static readonly int propId_gamma             = Shader.PropertyToID("gamma");
        public static readonly int propId_radius            = Shader.PropertyToID("radius");
        public static readonly int propId_sunRadius         = Shader.PropertyToID("sunRadius");
        public static readonly int propId_sunDistance       = Shader.PropertyToID("sunDistance");
        public static readonly int propId_sunFlareTexture   = Shader.PropertyToID("sunFlareTexture");
        public static readonly int propId_backgroundTexture = Shader.PropertyToID("backgroundTexture");

        #endregion

        internal static RenderTexture cameraOverride;

        private static Material materialSunFlear;
        private static Material materialTonemaps;
        private static Material materialWriteDepth;
        private static Material materialRemoveAlpha;
        private static Texture2D sunFlareTexture;
        static ShaderLoader()
        {
            WorldCameraManager.WorldCamera.depthTextureMode = DepthTextureMode.Depth;
            WorldCameraManager.WorldCamera.gameObject.AddComponent<PlanetAtmosphereRenderer>();
            WorldCameraManager.WorldSkyboxCamera.depthTextureMode = DepthTextureMode.Depth;
            WorldCameraManager.WorldSkyboxCamera.backgroundColor = Color.black;
            

            WorldMaterials.Rivers.shader = WorldMaterials.WorldTerrain.shader;
            WorldMaterials.WorldOcean.shader = WorldMaterials.WorldTerrain.shader;
            WorldMaterials.RiversBorder.shader = WorldMaterials.WorldTerrain.shader;
            WorldMaterials.UngeneratedPlanetParts.shader = WorldMaterials.WorldTerrain.shader;

            WorldMaterials.Rivers.renderQueue = 3530;
            WorldMaterials.WorldOcean.renderQueue = 3500;
            WorldMaterials.RiversBorder.renderQueue = 3520;
            WorldMaterials.UngeneratedPlanetParts.renderQueue = 3500;

            // WorldMaterials.Rivers.color = new Color(-65536,-65536,-65536,1);
            // WorldMaterials.RiversBorder.color = new Color(0,0,0,0);


            WorldMaterials.Rivers.mainTexture = ContentFinder<Texture2D>.Get("TerrainReplace/Water");
            WorldMaterials.WorldOcean.mainTexture = ContentFinder<Texture2D>.Get("TerrainReplace/Water");
            WorldMaterials.RiversBorder.mainTexture = ContentFinder<Texture2D>.Get("TerrainReplace/Water");
            WorldMaterials.UngeneratedPlanetParts.mainTexture = ContentFinder<Texture2D>.Get("TerrainReplace/Water");

        }

        static void checkAndUpdate()
        {
            if(AtmosphereSettings.needUpdate)
            {
                AtmosphereSettings.needUpdate = false;
                sunFlareTexture = TransparentObject.GetTexture2D(AtmosphereSettings.sunFlareTexturePath);
                Shader
                shader = TransparentObject.GetShader(@"Assets/RW_PlanetAtmosphere/Shader/SunFlare.shader");
                if(shader)
                {
                    materialSunFlear = new Material(shader);
                    materialSunFlear.SetFloat(propId_sunRadius, AtmosphereSettings.sunRadius);
                    materialSunFlear.SetFloat(propId_sunDistance, AtmosphereSettings.sunDistance);
                    materialSunFlear.SetTexture(propId_sunFlareTexture, sunFlareTexture);
                }
                shader = TransparentObject.GetShader(@"Assets/RW_PlanetAtmosphere/Shader/Tonemaps.shader");
                if(shader)
                {
                    materialTonemaps = new Material(shader);
                    materialTonemaps.SetFloat(propId_gamma, AtmosphereSettings.gamma);
                }
                shader = TransparentObject.GetShader(@"Assets/RW_PlanetAtmosphere/Shader/WriteDepth.shader");
                if(shader)
                {
                    materialWriteDepth = new Material(shader);
                    materialWriteDepth.SetFloat(propId_radius,AtmosphereSettings.planetRadius);
                }
                shader = TransparentObject.GetShader(@"Assets/RW_PlanetAtmosphere/Shader/RemoveAlpha.shader");
                if(shader) materialRemoveAlpha = new Material(shader);
                if(AtmosphereSettings.renderingSizeFactor != 1)
                {
                    int width = (int)(Screen.width * AtmosphereSettings.renderingSizeFactor);
                    int height = (int)(Screen.height * AtmosphereSettings.renderingSizeFactor);
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
            }
        }


        private class PlanetAtmosphereRenderer : MonoBehaviour
        {
            // CommandBuffer commandBufferBeforeTransparent;
            CommandBuffer commandBufferAfterDepth;
            CommandBuffer commandBufferAfterAlpha;
            // public readonly List<Material> materialsTest = new List<Material>();
            // private Transform cachedTransform = null;
            void Start()
            {
                commandBufferAfterDepth = new CommandBuffer();
                commandBufferAfterDepth.name = "RW_PlanetAtmosphere.AfterDepth";
                commandBufferAfterAlpha = new CommandBuffer();
                commandBufferAfterAlpha.name = "RW_PlanetAtmosphere.AfterAlpha";
                Find.WorldCamera.AddCommandBuffer(CameraEvent.AfterDepthTexture,commandBufferAfterDepth);
                Find.WorldCamera.AddCommandBuffer(CameraEvent.AfterForwardAlpha,commandBufferAfterAlpha);

            }
            void Update()
            {
                if (Find.World != null)
                {
                    Shader.SetGlobalVector("_WorldSpaceLightPos0",GenCelestial.CurSunPositionInWorldSpace());
                    Shader.SetGlobalVector("_LightColor0",AtmosphereSettings.sunColor);
                }
                checkAndUpdate();
                if (materialWriteDepth)
                {
                    commandBufferAfterDepth.Clear();
                    commandBufferAfterDepth.DrawMesh(TransparentObject.DefaultRenderingMesh,Matrix4x4.identity,materialWriteDepth, 0, 0);
                }
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
                    AtmosphereSettings.refraction = Math.Abs(AtmosphereSettings.refraction);
                    if (AtmosphereSettings.refraction != 1)
                    {
                        Color color = new Color(AtmosphereSettings.refraction,AtmosphereSettings.refraction,AtmosphereSettings.refraction,1);
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
                    AtmosphereSettings.luminescen = Math.Abs(AtmosphereSettings.luminescen);
                    if(AtmosphereSettings.luminescen != 0)
                    {
                        Color color = new Color(AtmosphereSettings.luminescen,AtmosphereSettings.luminescen,AtmosphereSettings.luminescen,0);
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
                TransparentObject.DrawTransparentObjects(AtmosphereSettings.objects, commandBufferAfterAlpha, WorldCameraManager.WorldCamera, BeforeShadow, BackgroundBlendLumen, AfterTrans);
                if (materialSunFlear)
                {
                    commandBufferAfterAlpha.DrawMesh(TransparentObject.DefaultRenderingMesh, Matrix4x4.identity, materialSunFlear, 0, 1);
                    commandBufferAfterAlpha.ReleaseTemporaryRT(propId_backgroundTexture);
                }
                if (materialTonemaps)
                {
                    materialTonemaps.SetFloat(propId_gamma, AtmosphereSettings.gamma);
                    commandBufferAfterAlpha.GetTemporaryRT(propId_backgroundTexture, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
                    commandBufferAfterAlpha.Blit(BuiltinRenderTextureType.CameraTarget, propId_backgroundTexture);
                    //commandBufferAfter.SetGlobalTexture(propId_backgroundTexture, BuiltinRenderTextureType.CameraTarget);
                    commandBufferAfterAlpha.Blit(null, BuiltinRenderTextureType.CameraTarget, materialTonemaps,(int)AtmosphereSettings.tonemapType);
                    commandBufferAfterAlpha.ReleaseTemporaryRT(propId_backgroundTexture);
                }
                commandBufferAfterAlpha.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            }

        }
    }
}