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

namespace RW_PlanetAtmosphere
{
    [StaticConstructorOnStartup]
    internal static class ShaderLoader
    {
        public readonly static Material materialSkyLUT = null;
        public readonly static Material materialTranslucentGenrater = null;
        public readonly static Material materialScatterGenrater = null;
        public readonly static List<Material> materialCloudLUTs = new List<Material>();
        internal static Mesh mesh = null;
        internal static Shader SkyBox_LUT = null;
        internal static Shader SkyBoxCloud_LUT = null;
        internal static Shader TranslucentGenrater = null;
        internal static Shader ScatterGenrater = null;
        internal static GameObject sky = null;
        internal static MeshFilter meshFilter = null;
        internal static MeshRenderer meshRenderer = null;
        internal static RenderTexture translucentLUT = null;
        internal static RenderTexture scatterLUT_Reayleigh = null;
        internal static RenderTexture scatterLUT_Mie = null;
        internal static ModContentPack modAsset = null;


        private static float maxh = minh;
        private static float minh => 100f;


#region propsIDs

        private static int propId_exposure              = Shader.PropertyToID("exposure");
        private static int propId_ground_refract        = Shader.PropertyToID("ground_refract");
        private static int propId_ground_light          = Shader.PropertyToID("ground_light");
        private static int propId_deltaL                = Shader.PropertyToID("deltaL");
        private static int propId_deltaW                = Shader.PropertyToID("deltaW");
        private static int propId_lengthL               = Shader.PropertyToID("lengthL");
        private static int propId_lengthW               = Shader.PropertyToID("lengthW");
        private static int propId_minh                  = Shader.PropertyToID("minh");
        private static int propId_maxh                  = Shader.PropertyToID("maxh");
        private static int propId_H_Reayleigh           = Shader.PropertyToID("H_Reayleigh");
        private static int propId_H_Mie                 = Shader.PropertyToID("H_Mie");
        private static int propId_H_OZone               = Shader.PropertyToID("H_OZone");
        private static int propId_D_OZone               = Shader.PropertyToID("D_OZone");
        private static int propId_reayleigh_scatter     = Shader.PropertyToID("reayleigh_scatter");
        private static int propId_molecule_absorb       = Shader.PropertyToID("molecule_absorb");
        private static int propId_OZone_absorb          = Shader.PropertyToID("OZone_absorb");
        private static int propId_mie_scatter           = Shader.PropertyToID("mie_scatter");
        private static int propId_mie_absorb            = Shader.PropertyToID("mie_absorb");
        private static int propId_mie_eccentricity      = Shader.PropertyToID("mie_eccentricity");
        private static int propId_sunColor              = Shader.PropertyToID("sunColor");
        private static int propId_scatterLUT_Size       = Shader.PropertyToID("scatterLUT_Size");
        private static int propId_translucentLUT        = Shader.PropertyToID("translucentLUT");
        private static int propId_scatterLUT_Reayleigh  = Shader.PropertyToID("scatterLUT_Reayleigh");
        private static int propId_scatterLUT_Mie        = Shader.PropertyToID("scatterLUT_Mie");
        
#endregion

        public static bool isEnable =>  materialSkyLUT              != null && (materialSkyLUT.shader?.isSupported ?? false)                && 
                                        materialTranslucentGenrater != null && (materialTranslucentGenrater.shader?.isSupported ?? false)   && 
                                        materialScatterGenrater     != null && (materialScatterGenrater.shader?.isSupported ?? false)       && 
                                        SkyBoxCloud_LUT             != null && SkyBoxCloud_LUT.isSupported;



        static ShaderLoader()
        {
            uint loadedCount = 0;
            SkyBox_LUT = GetShader(@"Assets/RW_PlanetAtmosphere/Shader/SkyBoxLUT.shader");
            if(SkyBox_LUT != null) loadedCount++;
            SkyBoxCloud_LUT = GetShader(@"Assets/RW_PlanetAtmosphere/Shader/SkyBoxCloudLUT.shader");
            if(SkyBoxCloud_LUT != null) loadedCount++;
            TranslucentGenrater = GetShader(@"Assets/RW_PlanetAtmosphere/Shader/TranslucentGenrater.shader");
            if(TranslucentGenrater != null) loadedCount++;
            ScatterGenrater = GetShader(@"Assets/RW_PlanetAtmosphere/Shader/ScatterGenrater.shader");
            if(ScatterGenrater != null) loadedCount++;
            if (loadedCount >= 4)
            {
                materialSkyLUT = new Material(SkyBox_LUT)
                {
                    renderQueue = 3555
                };
                materialTranslucentGenrater = new Material(TranslucentGenrater);
                materialScatterGenrater = new Material(ScatterGenrater);

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
                var planetAtmosphere = sky.AddComponent<PlanetAtmosphere>();
                GameObject.DontDestroyOnLoad(sky);
                sky.layer = WorldCameraManager.WorldLayer;
                meshFilter.mesh = mesh;
                meshRenderer.material = materialSkyLUT;
                // meshRenderer.sortingOrder
                // Graphics.DrawMesh
                // WorldCameraManager.WorldCamera.fieldOfView = 20;
                // WorldCameraManager.WorldSkyboxCamera.fieldOfView = 20;
                WorldCameraManager.WorldCamera.depthTextureMode = DepthTextureMode.Depth;
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

        public static Shader GetShader(string path)
        {
            if(modAsset == null)
            {
                foreach (ModContentPack pack in LoadedModManager.RunningModsListForReading)
                {
                    if (pack.PackageId.Equals("rwnodetree.rwplanetatmosphere") && !pack.assetBundles.loadedAssetBundles.NullOrEmpty())
                    {
                        modAsset = pack;
                    }
                }
            }
            if(modAsset != null)
            {
                foreach (AssetBundle assetBundle in modAsset.assetBundles.loadedAssetBundles)
                {
                    // Log.Message($"Loading shader in {assetBundle.name}");
                    Shader shader = assetBundle.LoadAsset<Shader>(path);
                    if (shader != null && shader.isSupported) return shader;
                }
            }
            return null;
        }


        static void UpdateMaterialDyn(Material material, float ground_refract, float ground_light)
        {
            if(material == null) return;
            
            try
            {
                material.SetFloat(propId_exposure, AtmosphereSettings.exposure);
                material.SetFloat(propId_ground_refract, ground_refract);
                material.SetFloat(propId_ground_light, ground_light);
                material.SetVector(propId_sunColor, AtmosphereSettings.sunColor);
                material.SetVector(propId_mie_eccentricity, AtmosphereSettings.mie_eccentricity);
            }
            catch(Exception ex)
            {
                Log.Error(
$@"error report : UpdateMaterialDyn error
material : {material}
exposure : {AtmosphereSettings.exposure}
ground_refract : {ground_refract}
ground_light : {ground_light}
sunColor : {AtmosphereSettings.sunColor}
mie_eccentricity : {AtmosphereSettings.mie_eccentricity}
Exception : {ex}"
                );
            }
        }
        
        static void UpdateMaterialStatic(Material material)
        {
            if(material == null) return;

            try
            {
                material.SetFloat(propId_deltaL, AtmosphereSettings.deltaL);
                material.SetFloat(propId_deltaW, AtmosphereSettings.deltaW);
                material.SetFloat(propId_lengthL, AtmosphereSettings.lengthL);
                material.SetFloat(propId_lengthW, AtmosphereSettings.lengthW);
                material.SetFloat(propId_minh, minh);
                material.SetFloat(propId_maxh, maxh);
                material.SetFloat(propId_H_Reayleigh, AtmosphereSettings.H_Reayleigh);
                material.SetFloat(propId_H_Mie, AtmosphereSettings.H_Mie);
                material.SetFloat(propId_H_OZone, AtmosphereSettings.H_OZone);
                material.SetFloat(propId_D_OZone, AtmosphereSettings.D_OZone);
                material.SetVector(propId_reayleigh_scatter, AtmosphereSettings.reayleigh_scatter);
                material.SetVector(propId_molecule_absorb, AtmosphereSettings.molecule_absorb);
                material.SetVector(propId_OZone_absorb, AtmosphereSettings.OZone_absorb);
                material.SetVector(propId_mie_scatter, AtmosphereSettings.mie_scatter);
                material.SetVector(propId_mie_absorb, AtmosphereSettings.mie_absorb);
            }
            catch(Exception ex)
            {
                Log.Error(
$@"error report : UpdateMaterialStatic error
material : {material}
deltaL : {AtmosphereSettings.deltaL}
deltaW : {AtmosphereSettings.deltaW}
lengthL : {AtmosphereSettings.lengthL}
lengthW : {AtmosphereSettings.lengthW}
minh : {minh}
maxh : {maxh}
H_Reayleigh : {AtmosphereSettings.H_Reayleigh}
H_Mie : {AtmosphereSettings.H_Mie}
H_OZone : {AtmosphereSettings.H_OZone}
D_OZone : {AtmosphereSettings.D_OZone}
reayleigh_scatter : {AtmosphereSettings.reayleigh_scatter}
mie_scatter : {AtmosphereSettings.mie_scatter}
mie_absorb : {AtmosphereSettings.mie_absorb}
OZone_absorb : {AtmosphereSettings.OZone_absorb}
Exception : {ex}"
                );
            }
        }
        
        static void UpdateMaterialLUT(Material material)
        {
            if(material == null) return;
            Vector4 scatterLUT_Size = AtmosphereSettings.scatterLUT_Size * 16;
            try
            {
                material.SetVector(propId_scatterLUT_Size, new Vector4((int)scatterLUT_Size.x, (int)scatterLUT_Size.y, (int)scatterLUT_Size.z, (int)scatterLUT_Size.w));
                material.SetTexture(propId_translucentLUT, translucentLUT);
                material.SetTexture(propId_scatterLUT_Reayleigh, scatterLUT_Reayleigh);
                material.SetTexture(propId_scatterLUT_Mie, scatterLUT_Mie);
            }
            catch(Exception ex)
            {
                Log.Error(
$@"error report : UpdateMaterialLUT error
material : {material}
propId_scatterLUT_Size : {scatterLUT_Size}
translucentLUT : {translucentLUT}
scatterLUT_Reayleigh : {scatterLUT_Reayleigh}
scatterLUT_Mie : {scatterLUT_Mie}
Exception : {ex}"
                );
            }
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
                            AtmosphereSettings.reayleigh_scatter.x * AtmosphereSettings.sunColor.x,
                            AtmosphereSettings.reayleigh_scatter.y * AtmosphereSettings.sunColor.y,
                            AtmosphereSettings.reayleigh_scatter.z * AtmosphereSettings.sunColor.z,
                            AtmosphereSettings.reayleigh_scatter.w * AtmosphereSettings.sunColor.w
                        ) * AtmosphereSettings.H_Reayleigh),
                        -Mathf.Log(0.00001f)*(Mathf.Max
                        (
                            (AtmosphereSettings.mie_scatter.x + AtmosphereSettings.mie_absorb.x) * AtmosphereSettings.sunColor.x,
                            (AtmosphereSettings.mie_scatter.y + AtmosphereSettings.mie_absorb.y) * AtmosphereSettings.sunColor.y,
                            (AtmosphereSettings.mie_scatter.z + AtmosphereSettings.mie_absorb.z) * AtmosphereSettings.sunColor.z,
                            (AtmosphereSettings.mie_scatter.w + AtmosphereSettings.mie_absorb.w) * AtmosphereSettings.sunColor.w
                        ) * AtmosphereSettings.H_Mie)
                    );
                    cachedTransform = cachedTransform ?? transform;
                    cachedTransform.localScale = Vector3.one * maxh / minh;

                    Vector4 scatterLUTSize = AtmosphereSettings.scatterLUT_Size * 16;
                    Vector2Int translucentLUTSize = Vector2Int.FloorToInt(AtmosphereSettings.translucentLUT_Size) * 16;
                    Vector2Int scatterLUTSize2D = new Vector2Int((int)scatterLUTSize.x * (int)scatterLUTSize.z, (int)scatterLUTSize.y * (int)scatterLUTSize.w);
                    
                    if(translucentLUT == null || translucentLUT.width != translucentLUTSize.x || translucentLUT.height != translucentLUTSize.y)
                    {
                        if (translucentLUT != null) Destroy(translucentLUT);
                        translucentLUT = new RenderTexture(translucentLUTSize.x, translucentLUTSize.y, 0)
                        {
                            // enableRandomWrite = true,
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
                            // enableRandomWrite = true,
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
                            // enableRandomWrite = true,
                            useMipMap = false,
                            format = RenderTextureFormat.ARGBHalf,
                            wrapMode = TextureWrapMode.Clamp
                        };
                        scatterLUT_Mie.Create();
                    }


                    try
                    {
                        UpdateMaterialStatic(materialTranslucentGenrater);
                        UpdateMaterialLUT(materialTranslucentGenrater);
                        Graphics.Blit(null, translucentLUT, materialTranslucentGenrater);
                    
                    }
                    catch(Exception ex)
                    {
                        Log.Error(
$@"error report : translucent LUT generate error
materialTranslucentGenrater : {materialTranslucentGenrater}
translucentLUT : {translucentLUT}
Exception : {ex}"
                        );
                    }


                    RenderTexture cached = RenderTexture.active;
                    try
                    {
                        UpdateMaterialStatic(materialScatterGenrater);
                        UpdateMaterialLUT(materialScatterGenrater);
                        Graphics.SetRenderTarget(new RenderBuffer[] { scatterLUT_Reayleigh.colorBuffer, scatterLUT_Mie.colorBuffer }, scatterLUT_Reayleigh.depthBuffer);
                        Graphics.Blit(null, materialScatterGenrater);
                    }
                    catch(Exception ex)
                    {
                        Log.Error(
$@"error report : scatter LUT generate error
materialScatterGenrater : {materialScatterGenrater}
scatterLUT_Reayleigh : {scatterLUT_Reayleigh}
scatterLUT_Mie : {scatterLUT_Mie}
scatterLUT_Reayleigh.colorBuffer : {scatterLUT_Reayleigh.colorBuffer}
scatterLUT_Mie.colorBuffer : {scatterLUT_Mie.colorBuffer}
Exception : {ex}"
                        );
                    }
                    RenderTexture.active = cached;

                    UpdateMaterialStatic(materialSkyLUT);
                    UpdateMaterialLUT(materialSkyLUT);

                    
                    
                    try
                    {
                        AtmosphereSettings.cloudTexPath     = AtmosphereSettings.cloudTexPath   ??  new List<string>();
                        AtmosphereSettings.cloudTexValue    = AtmosphereSettings.cloudTexValue  ??  new List<Vector4>();
                        AtmosphereSettings.noiseTexPath     = AtmosphereSettings.noiseTexPath   ??  new List<string>();
                        AtmosphereSettings.noiseTexValue    = AtmosphereSettings.noiseTexValue  ??  new List<Vector2>();
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
                    }
                    catch(Exception ex)
                    {
                        Log.Error(
$@"error report : cloud update error
AtmosphereSettings.cloudTexPath.Count   :   {AtmosphereSettings.cloudTexPath.Count }
AtmosphereSettings.cloudTexValue.Count  :   {AtmosphereSettings.cloudTexValue.Count}
AtmosphereSettings.noiseTexPath.Count   :   {AtmosphereSettings.noiseTexPath.Count }
AtmosphereSettings.noiseTexValue.Count  :   {AtmosphereSettings.noiseTexValue.Count}
Exception : {ex}"
                        );
                    }
                    AtmosphereSettings.updated = true;
                }
            }
            void Update()
            {
                cachedTransform = cachedTransform ?? transform;
                if(isEnable)
                {
                    parmUpdated();
                    UpdateMaterialDyn(materialSkyLUT,AtmosphereSettings.ground_refract,AtmosphereSettings.ground_light);
                    for(int i = 0; i < materialCloudLUTs.Count; i++)
                    {
                        Material cloud = materialCloudLUTs[i];
                        cloud.SetFloat(propId_exposure, AtmosphereSettings.exposure);
                        cloud.SetVector(propId_mie_eccentricity, AtmosphereSettings.mie_eccentricity);
                    }
                    if(Find.World != null)
                    {
                        Shader.SetGlobalVector("_WorldSpaceLightPos0",GenCelestial.CurSunPositionInWorldSpace());
                        cachedTransform.localScale = Vector3.one * (Find.PlaySettings.usePlanetDayNightSystem ? (maxh / minh) : 0f);
                    }
                }
            }

        }
    }
}