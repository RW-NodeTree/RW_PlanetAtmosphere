using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;
using Verse.Noise;
using Verse.AI.Group;

namespace RW_PlanetAtmosphere
{
    [StaticConstructorOnStartup]
    internal static class ShaderLoader
    {
        public readonly static Material materialSkyLUT = null;
        private static Mesh mesh = null;
        private static Shader SkyBox_LUT = null;
        private static Shader SkyBoxCloud_LUT = null;
        private static Shader TranslucentGenrater = null;
        private static Shader ScatterGenrater = null;
        private static Material materialTranslucentGenrater = null;
        private static Material materialScatterGenrater = null;
        private static GameObject sky = null;
        private static MeshFilter meshFilter = null;
        private static MeshRenderer meshRenderer = null;
        private readonly static List<Material> materialCloudLUTs = new List<Material>();
        internal static RenderTexture translucentLUT = null;
        internal static RenderTexture scatterLUT_Reayleigh = null;
        internal static RenderTexture scatterLUT_Mie = null;


        private static float maxh = minh;
        private static float minh => 100f;


#region propsIDs
        private static int propId_exposure              ;
        private static int propId_ground_refract        ;
        private static int propId_ground_light          ;
        private static int propId_deltaAHLW_L           ;
        private static int propId_deltaAHLW_W           ;
        private static int propId_lengthAHLW_L          ;
        private static int propId_lengthAHLW_W          ;
        private static int propId_minh                  ;
        private static int propId_maxh                  ;
        private static int propId_H_Reayleigh           ;
        private static int propId_H_Mie                 ;
        private static int propId_H_OZone               ;
        private static int propId_D_OZone               ;
        private static int propId_SunColor              ;
        private static int propId_mie_eccentricity      ;
        private static int propId_scatterLUT_Size       ;
        private static int propId_reayleigh_scatter     ;
        private static int propId_mie_scatter           ;
        private static int propId_mie_absorb            ;
        private static int propId_OZone_absorb          ;
        private static int propId_translucentLUT        ;
        private static int propId_scatterLUT_Reayleigh  ;
        private static int propId_scatterLUT_Mie        ;
        
#endregion

        public static bool isEnable => materialSkyLUT != null && (materialSkyLUT.shader?.isSupported ?? false);
        static ShaderLoader()
        {
            uint loadedCount = 0;
            List<ModContentPack> runningModsListForReading = LoadedModManager.RunningModsListForReading;
            foreach (ModContentPack pack in runningModsListForReading)
            {
                //Log.Message($"{pack.PackageId},{pack.assetBundles.loadedAssetBundles?.Count}");
                if (pack.PackageId.Equals("rwnodetree.rwplanetatmosphere") && !pack.assetBundles.loadedAssetBundles.NullOrEmpty())
                {
                    //Log.Message($"{pack.PackageId} found, try to load shader");
                    foreach (AssetBundle assetBundle in pack.assetBundles.loadedAssetBundles)
                    {
                        // Log.Message($"Loading shader in {assetBundle.name}");
                        SkyBox_LUT = assetBundle.LoadAsset<Shader>(@"Assets\Data\RWNodeTree.RWPlanetAtmosphere\SkyBoxLUT.shader");
                        if (SkyBox_LUT != null && SkyBox_LUT.isSupported)
                        {
                            loadedCount++;
                            break;
                        }
                    }
                    foreach (AssetBundle assetBundle in pack.assetBundles.loadedAssetBundles)
                    {
                        // Log.Message($"Loading shader in {assetBundle.name}");
                        SkyBoxCloud_LUT = assetBundle.LoadAsset<Shader>(@"Assets\Data\RWNodeTree.RWPlanetAtmosphere\SkyBoxCloudLUT.shader");
                        if (SkyBoxCloud_LUT != null && SkyBoxCloud_LUT.isSupported)
                        {
                            loadedCount++;
                            break;
                        }
                    }
                    foreach (AssetBundle assetBundle in pack.assetBundles.loadedAssetBundles)
                    {
                        // Log.Message($"Loading shader in {assetBundle.name}");
                        TranslucentGenrater = assetBundle.LoadAsset<Shader>(@"Assets\Data\RWNodeTree.RWPlanetAtmosphere\TranslucentGenrater.shader");
                        if (TranslucentGenrater != null && TranslucentGenrater.isSupported)
                        {
                            loadedCount++;
                            break;
                        }
                    }
                    foreach (AssetBundle assetBundle in pack.assetBundles.loadedAssetBundles)
                    {
                        // Log.Message($"Loading shader in {assetBundle.name}");
                        ScatterGenrater = assetBundle.LoadAsset<Shader>(@"Assets\Data\RWNodeTree.RWPlanetAtmosphere\ScatterGenrater.shader");
                        if (ScatterGenrater != null && ScatterGenrater.isSupported)
                        {
                            loadedCount++;
                            break;
                        }
                    }
                    break;
                }
            }
            if (loadedCount >= 4)
            {
                propId_exposure              = Shader.PropertyToID("exposure");
                propId_ground_refract        = Shader.PropertyToID("ground_refract");
                propId_ground_light          = Shader.PropertyToID("ground_light");
                propId_deltaAHLW_L           = Shader.PropertyToID("deltaAHLW_L");
                propId_deltaAHLW_W           = Shader.PropertyToID("deltaAHLW_W");
                propId_lengthAHLW_L          = Shader.PropertyToID("lengthAHLW_L");
                propId_lengthAHLW_W          = Shader.PropertyToID("lengthAHLW_W");
                propId_minh                  = Shader.PropertyToID("minh");
                propId_maxh                  = Shader.PropertyToID("maxh");
                propId_H_Reayleigh           = Shader.PropertyToID("H_Reayleigh");
                propId_H_Mie                 = Shader.PropertyToID("H_Mie");
                propId_H_OZone               = Shader.PropertyToID("H_OZone");
                propId_D_OZone               = Shader.PropertyToID("D_OZone");
                propId_SunColor              = Shader.PropertyToID("SunColor");
                propId_mie_eccentricity      = Shader.PropertyToID("mie_eccentricity");
                propId_scatterLUT_Size       = Shader.PropertyToID("scatterLUT_Size");
                propId_reayleigh_scatter     = Shader.PropertyToID("reayleigh_scatter");
                propId_mie_scatter           = Shader.PropertyToID("mie_scatter");
                propId_mie_absorb            = Shader.PropertyToID("mie_absorb");
                propId_OZone_absorb          = Shader.PropertyToID("OZone_absorb");
                propId_translucentLUT        = Shader.PropertyToID("translucentLUT");
                propId_scatterLUT_Reayleigh  = Shader.PropertyToID("scatterLUT_Reayleigh");
                propId_scatterLUT_Mie        = Shader.PropertyToID("scatterLUT_Mie");
                materialSkyLUT = new Material(SkyBox_LUT)
                {
                    renderQueue = 3555
                };

                mesh = new Mesh();
                SphereGenerator.Generate(6, minh, Vector3.forward, 360f, out var outVerts, out var outIndices);
                mesh.vertices = outVerts.ToArray();
                mesh.triangles = outIndices.ToArray();
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();
                mesh.UploadMeshData(false);
                
                sky = new GameObject("RW_PlanetAtmosphere");
                meshFilter = sky.AddComponent<MeshFilter>();
                meshRenderer = sky.AddComponent<MeshRenderer>();
                sky.AddComponent<PlanetAtmosphere>();
                Object.DontDestroyOnLoad(sky);
                sky.layer = WorldCameraManager.WorldLayer;
                meshFilter.mesh = mesh;
                meshRenderer.material = materialSkyLUT;
                // WorldCameraManager.WorldCamera.fieldOfView = 20;
                // WorldCameraManager.WorldSkyboxCamera.fieldOfView = 20;
                WorldCameraManager.WorldCamera.depthTextureMode = DepthTextureMode.Depth;
                WorldCameraManager.WorldSkyboxCamera.depthTextureMode = DepthTextureMode.Depth;

                WorldMaterials.Rivers.shader = WorldMaterials.WorldTerrain.shader;
                WorldMaterials.WorldOcean.shader = WorldMaterials.WorldTerrain.shader;
                // WorldMaterials.RiversBorder.shader = WorldMaterials.WorldTerrain.shader;
                WorldMaterials.UngeneratedPlanetParts.shader = WorldMaterials.WorldTerrain.shader;

                WorldMaterials.Rivers.renderQueue = 3530;
                WorldMaterials.WorldOcean.renderQueue = 3500;
                // WorldMaterials.RiversBorder.renderQueue = 3520;
                WorldMaterials.UngeneratedPlanetParts.renderQueue = 3500;

                // WorldMaterials.Rivers.color = new Color(-65536,-65536,-65536,1);
                // WorldMaterials.RiversBorder.color = new Color(0,0,0,0);


                WorldMaterials.Rivers.mainTexture = ContentFinder<Texture2D>.Get("TerrainReplace/Water");
                WorldMaterials.WorldOcean.mainTexture = ContentFinder<Texture2D>.Get("TerrainReplace/Water");
                // WorldMaterials.RiversBorder.mainTexture = ContentFinder<Texture2D>.Get("TerrainReplace/Water");
                WorldMaterials.UngeneratedPlanetParts.mainTexture = ContentFinder<Texture2D>.Get("TerrainReplace/Water");

                // planetAtmosphere.materialsTest.Add(WorldMaterials.WorldOcean);
                // planetAtmosphere.materialsTest.Add(WorldMaterials.UngeneratedPlanetParts);
                // planetAtmosphere.materialsTest.Add(WorldMaterials.Stars);
                // planetAtmosphere.materialsTest.Add(WorldMaterials.Rivers);
                // planetAtmosphere.materialsTest.Add(WorldMaterials.RiversBorder);
                // planetAtmosphere.materialsTest.Add(WorldMaterials.WorldTerrain);
                // planetAtmosphere.materialsTest.Add(WorldMaterials.WorldIce);

                // WorldMaterials.WorldOcean.color = new Color32(1,2,4,255);
                // WorldMaterials.UngeneratedPlanetParts.color = new Color32(1,2,4,255);
                // WorldMaterials.Rivers.color = new Color32(1,2,4,255);
            }
        }


        static void UpdateMaterialDyn(Material material, float ground_refract, float ground_light)
        {
            material.SetFloat(propId_exposure, AtmosphereSettings.exposure);
            material.SetFloat(propId_ground_refract, ground_refract);
            material.SetFloat(propId_ground_light, ground_light);
            material.SetVector(propId_SunColor, AtmosphereSettings.SunColor);
            material.SetVector(propId_mie_eccentricity, AtmosphereSettings.mie_eccentricity);
        }
        
        static void UpdateMaterialStatic(Material material)
        {
            material.SetFloat(propId_deltaAHLW_L, AtmosphereSettings.deltaAHLW_L);
            material.SetFloat(propId_deltaAHLW_W, AtmosphereSettings.lengthAHLW_L);
            material.SetFloat(propId_lengthAHLW_L, AtmosphereSettings.deltaAHLW_W);
            material.SetFloat(propId_lengthAHLW_W, AtmosphereSettings.lengthAHLW_W);
            material.SetFloat(propId_minh, minh);
            material.SetFloat(propId_maxh, maxh);
            material.SetFloat(propId_H_Reayleigh, AtmosphereSettings.H_Reayleigh);
            material.SetFloat(propId_H_Mie, AtmosphereSettings.H_Mie);
            material.SetFloat(propId_H_OZone, AtmosphereSettings.H_OZone);
            material.SetFloat(propId_D_OZone, AtmosphereSettings.D_OZone);
            material.SetVector(propId_reayleigh_scatter, AtmosphereSettings.reayleigh_scatter);
            material.SetVector(propId_mie_scatter, AtmosphereSettings.mie_scatter);
            material.SetVector(propId_mie_absorb, AtmosphereSettings.mie_absorb);
            material.SetVector(propId_OZone_absorb, AtmosphereSettings.OZone_absorb);
        }
        
        static void UpdateMaterialLUT(Material material)
        {
            material.SetVector(propId_scatterLUT_Size, AtmosphereSettings.scatterLUT_Size);
            material.SetTexture(propId_translucentLUT, translucentLUT);
            material.SetTexture(propId_scatterLUT_Reayleigh, scatterLUT_Reayleigh);
            material.SetTexture(propId_scatterLUT_Mie, scatterLUT_Mie);
        }

        private class PlanetAtmosphere : MonoBehaviour
        {
            // public readonly List<Material> materialsTest = new List<Material>();
            private Transform cachedTransform = null;
            void parmUpdated()
            {
                if(!AtmosphereSettings.updated && isEnable)
                {
                    MeshRenderer[] renderers = this.GetComponentsInChildren<MeshRenderer>();
                    for(int i= 0; i < renderers.Length; i++)
                    {
                        if(renderers[i].transform == transform) continue;
                        GameObject.Destroy(renderers[i].gameObject);
                    }
                    for(int i = 0; i < materialCloudLUTs.Count; i++)
                    {
                        GameObject.Destroy(materialCloudLUTs[i]);
                    }
                    materialCloudLUTs.Clear();
                    maxh = minh + Mathf.Max
                    (
                        AtmosphereSettings.H_OZone + AtmosphereSettings.D_OZone,
                        -Mathf.Log(0.00001f)*(Mathf.Max
                        (
                            AtmosphereSettings.reayleigh_scatter.x * AtmosphereSettings.SunColor.x,
                            AtmosphereSettings.reayleigh_scatter.y * AtmosphereSettings.SunColor.y,
                            AtmosphereSettings.reayleigh_scatter.z * AtmosphereSettings.SunColor.z,
                            AtmosphereSettings.reayleigh_scatter.w * AtmosphereSettings.SunColor.w
                        ) * AtmosphereSettings.H_Reayleigh),
                        -Mathf.Log(0.00001f)*(Mathf.Max
                        (
                            (AtmosphereSettings.mie_scatter.x + AtmosphereSettings.mie_absorb.x) * AtmosphereSettings.SunColor.x,
                            (AtmosphereSettings.mie_scatter.y + AtmosphereSettings.mie_absorb.y) * AtmosphereSettings.SunColor.y,
                            (AtmosphereSettings.mie_scatter.z + AtmosphereSettings.mie_absorb.z) * AtmosphereSettings.SunColor.z,
                            (AtmosphereSettings.mie_scatter.w + AtmosphereSettings.mie_absorb.w) * AtmosphereSettings.SunColor.w
                        ) * AtmosphereSettings.H_Mie)
                    );
                    cachedTransform = cachedTransform ?? transform;
                    cachedTransform.localScale = Vector3.one * maxh / minh;

                    Vector4 scatterLUTSize = AtmosphereSettings.scatterLUTSize * 16;
                    Vector2Int translucentLUTSize = Vector2Int.FloorToInt(AtmosphereSettings.translucentLUTSize) * 16;
                    Vector2Int scatterLUTSize2D = new Vector2Int((int)scatterLUTSize.x * (int)scatterLUTSize.z, (int)scatterLUTSize.y * (int)scatterLUTSize.w);
                    
                    if(translucentLUT == null || translucentLUT.width != translucentLUTSize.x || translucentLUT.height != translucentLUTSize.y)
                    {
                        if (translucentLUT != null) Destroy(translucentLUT);
                        translucentLUT = new RenderTexture(translucentLUTSize.x, translucentLUTSize.y, 0)
                        {
                            enableRandomWrite = true,
                            useMipMap = false,
                            format = RenderTextureFormat.ARGBFloat,
                            wrapMode = TextureWrapMode.Clamp
                        };
                        translucentLUT.Create();
                    }
                    if(scatterLUT_Reayleigh == null || scatterLUT_Reayleigh.width != scatterLUTSize2D.x || scatterLUT_Reayleigh.height != scatterLUTSize2D.y)
                    {
                        if (scatterLUT_Reayleigh != null) Destroy(scatterLUT_Reayleigh);
                        scatterLUT_Reayleigh = new RenderTexture(scatterLUTSize2D.x, scatterLUTSize2D.y, 0)
                        {
                            enableRandomWrite = true,
                            useMipMap = false,
                            format = RenderTextureFormat.ARGBHalf,
                            wrapMode = TextureWrapMode.Clamp
                        };
                        scatterLUT_Reayleigh.Create();
                    }
                    if(scatterLUT_Mie == null || scatterLUT_Mie.width != scatterLUTSize2D.x || scatterLUT_Mie.height != scatterLUTSize2D.y)
                    {
                        if (scatterLUT_Mie != null) Destroy(scatterLUT_Mie);
                        scatterLUT_Mie = new RenderTexture(scatterLUTSize2D.x, scatterLUTSize2D.y, 0)
                        {
                            enableRandomWrite = true,
                            useMipMap = false,
                            format = RenderTextureFormat.ARGBHalf,
                            wrapMode = TextureWrapMode.Clamp
                        };
                        scatterLUT_Mie.Create();
                    }

                    materialTranslucentGenrater = materialTranslucentGenrater ?? new Material(TranslucentGenrater);
                    materialScatterGenrater = materialScatterGenrater ?? new Material(ScatterGenrater);

                    UpdateMaterialStatic(materialTranslucentGenrater);
                    UpdateMaterialLUT(materialTranslucentGenrater);
                    Graphics.Blit(null, translucentLUT, materialTranslucentGenrater);
                    
                    UpdateMaterialStatic(materialScatterGenrater);
                    UpdateMaterialLUT(materialScatterGenrater);
                    Graphics.SetRenderTarget(new RenderBuffer[] { scatterLUT_Reayleigh.colorBuffer, scatterLUT_Mie.colorBuffer }, scatterLUT_Reayleigh.depthBuffer);
                    Graphics.Blit(null, materialScatterGenrater);
                    RenderTexture.active = null;
                    UpdateMaterialStatic(materialSkyLUT);
                    UpdateMaterialLUT(materialSkyLUT);

                    
                    
                    AtmosphereSettings.cloudTexPath = AtmosphereSettings.cloudTexPath ?? new List<string>();
                    AtmosphereSettings.cloudTexValue = AtmosphereSettings.cloudTexValue ?? new List<Vector4>();
                    AtmosphereSettings.noiseTexPath= AtmosphereSettings.noiseTexPath ?? new List<string>();
                    AtmosphereSettings.noiseTexValue = AtmosphereSettings.noiseTexValue ?? new List<Vector2>();
                    AtmosphereSettings.cloudTexPath.RemoveAll(x => x.NullOrEmpty());
                    for(int i = AtmosphereSettings.cloudTexValue.Count; i < AtmosphereSettings.cloudTexPath.Count; i++)
                    {
                        AtmosphereSettings.cloudTexValue.Add(new Vector4(1.0f,0.0f,0.5f,0.05f));
                    }
                    if(AtmosphereSettings.cloudTexValue.Count > AtmosphereSettings.cloudTexPath.Count) AtmosphereSettings.cloudTexValue.RemoveRange(AtmosphereSettings.cloudTexPath.Count, AtmosphereSettings.cloudTexValue.Count - AtmosphereSettings.cloudTexPath.Count);
                    for(int i = AtmosphereSettings.noiseTexPath.Count; i < AtmosphereSettings.noiseTexPath.Count; i++)
                    {
                        AtmosphereSettings.noiseTexPath.Add("EarthCloudTex/noise");
                    }
                    if(AtmosphereSettings.noiseTexPath.Count > AtmosphereSettings.cloudTexPath.Count) AtmosphereSettings.noiseTexPath.RemoveRange(AtmosphereSettings.cloudTexPath.Count, AtmosphereSettings.noiseTexPath.Count - AtmosphereSettings.cloudTexPath.Count);
                    for(int i = AtmosphereSettings.noiseTexValue.Count; i < AtmosphereSettings.cloudTexPath.Count; i++)
                    {
                        AtmosphereSettings.noiseTexValue.Add(new Vector2(0.0f,0.015625f));
                    }
                    if(AtmosphereSettings.noiseTexValue.Count > AtmosphereSettings.cloudTexPath.Count) AtmosphereSettings.noiseTexValue.RemoveRange(AtmosphereSettings.cloudTexPath.Count, AtmosphereSettings.noiseTexValue.Count - AtmosphereSettings.cloudTexPath.Count);

                    materialCloudLUTs.Capacity = AtmosphereSettings.cloudTexPath.Count;
                    for(int i = 0; i < AtmosphereSettings.cloudTexPath.Count; i++)
                    {
                        if(AtmosphereSettings.cloudTexPath[i] == null) continue;
                        Texture2D cloudTex = ContentFinder<Texture2D>.Get(AtmosphereSettings.cloudTexPath[i]);
                        if(cloudTex == null) continue;
                        Texture2D noiseTex = ContentFinder<Texture2D>.Get(AtmosphereSettings.noiseTexPath[i]);
                        Material cloud = new Material(SkyBoxCloud_LUT)
                        {
                            renderQueue = 3556
                        };
                        Vector4 cloudParm = AtmosphereSettings.cloudTexValue[i];
                        Vector2 noideParm = AtmosphereSettings.noiseTexValue[i];
                        UpdateMaterialStatic(cloud);
                        UpdateMaterialDyn(cloud, cloudParm.x, cloudParm.y);
                        UpdateMaterialLUT(cloud);
                        cloud.SetFloat("opacity", cloudParm.z);
                        cloud.SetFloat("flowDir", noideParm.x);
                        cloud.SetFloat("playRange", (noiseTex != null) ? noideParm.y : 0);
                        cloud.SetTexture("cloudTexture", cloudTex);
                        if(noiseTex != null) cloud.SetTexture("noiseTexture", noiseTex);
                        GameObject gameObject = new GameObject();
                        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
                        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
                        Transform transform = gameObject.transform;
                        filter.mesh = mesh;
                        renderer.material = cloud;
                        transform.parent = this.transform;
                        transform.localScale = Vector3.one * (cloudParm.w * (maxh - minh) + minh) / maxh; 
                        gameObject.layer = WorldCameraManager.WorldLayer;
                        materialCloudLUTs.Add(cloud);
                    }
                    AtmosphereSettings.updated = true;
                }
            }
            void Update()
            {
                cachedTransform = cachedTransform ?? transform;
                if(isEnable && Find.World != null)
                {
                    parmUpdated();
                    UpdateMaterialDyn(materialSkyLUT,AtmosphereSettings.ground_refract,AtmosphereSettings.ground_light);
                    for(int i = 0; i < materialCloudLUTs.Count; i++)
                    {
                        Material cloud = materialCloudLUTs[i];
                        cloud.SetFloat(propId_exposure, AtmosphereSettings.exposure);
                        cloud.SetVector(propId_mie_eccentricity, AtmosphereSettings.mie_eccentricity);
                    }
                    Shader.SetGlobalVector("_WorldSpaceLightPos0",GenCelestial.CurSunPositionInWorldSpace());
                    cachedTransform.localScale = Vector3.one * (Find.PlaySettings.usePlanetDayNightSystem ? (maxh / minh) : 0f);
                }
            }

        }
    }
}