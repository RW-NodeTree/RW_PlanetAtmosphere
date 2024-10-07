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
    internal static class ShaderLoader
    {
        static LightDriver lightDriver;
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

            GameObject gameObject = new GameObject("LightDriver");
            lightDriver = gameObject.AddComponent<LightDriver>();
            lightDriver.light = gameObject.AddComponent<Light>();
            lightDriver.light.type = LightType.Directional;
            GameObject.DontDestroyOnLoad(gameObject);
        }


        private class PlanetAtmosphereRenderer : MonoBehaviour
        {
            CommandBuffer commandBufferAfterTransparent;
            CommandBuffer commandBufferAfterOpaque;
            // public readonly List<Material> materialsTest = new List<Material>();
            private Transform cachedTransform = null;
            void Update()
            {
                // WorldCameraManager.WorldCamera.AddCommandBuffer()
            }

        }

        private class LightDriver : MonoBehaviour
        {
            // public readonly List<Material> materialsTest = new List<Material>();
            public Light light;
            private Transform cachedTransform = null;
            void Update()
            {

            }

        }
    }
}