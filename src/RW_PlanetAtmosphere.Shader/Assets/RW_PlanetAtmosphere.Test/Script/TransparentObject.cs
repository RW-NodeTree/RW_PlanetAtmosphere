using System;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;

namespace RW_PlanetAtmosphere
{
    public abstract class TransparentObject : IEnumerable//, IExposable
    {
        public static float sunRadius = 6960;
        public static float sunDistance = 1495978.92f;
        public static readonly int Reflection = Shader.PropertyToID("SimpleSpaceFlight_Reflection");
        public static readonly int DepthTexel = Shader.PropertyToID("SimpleSpaceFlight_DepthTexel");
        public static readonly int Reflection_volum = Shader.PropertyToID("SimpleSpaceFlight_Reflection_volum");
        public static readonly int DepthTexel_volum = Shader.PropertyToID("SimpleSpaceFlight_DepthTexel_volum");
        public static readonly int CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
        public static readonly int ColorTex = Shader.PropertyToID("ColorTex");
        public static readonly int DepthTex = Shader.PropertyToID("DepthTex");
        public static readonly int MainColor = Shader.PropertyToID("MainColor");
        public static readonly int propId_sunRadius = Shader.PropertyToID("sunRadius");
        public static readonly int propId_sunDistance = Shader.PropertyToID("sunDistance");
        public static readonly Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();
        public static readonly Dictionary<string, Texture2D> texture2Ds = new Dictionary<string, Texture2D>();
        private static Mesh defaultRenderingMesh;
        private static Material copyToDepthMaterial;
        private static Material addToTargetMaterial;



        public static Mesh DefaultRenderingMesh
        {
            get
            {
                if (!defaultRenderingMesh)
                {
                    defaultRenderingMesh = new Mesh();
                    defaultRenderingMesh.name = "Default";
                    defaultRenderingMesh.vertices = new Vector3[]
                    {
                    new Vector3( 1f, 1f),
                    new Vector3( 1f,-1f),
                    new Vector3(-1f,-1f),
                    new Vector3(-1f, 1f),
                    };
                    defaultRenderingMesh.triangles = new int[]
                    {
                    0, 1, 2,
                    0, 2, 3,
                    };
                    defaultRenderingMesh.RecalculateBounds();
                    defaultRenderingMesh.RecalculateNormals();
                    defaultRenderingMesh.RecalculateTangents();
                    defaultRenderingMesh.UploadMeshData(false);
                }
                return defaultRenderingMesh;
            }
        }

        public static Material CopyToDepthMaterial
        {
            get
            {
                if (!copyToDepthMaterial)
                {
                    Shader shader = GetShader(@"Assets/RW_PlanetAtmosphere/Shader/CopyToDepth.shader");
                    copyToDepthMaterial = new Material(shader);
                }
                return copyToDepthMaterial;
            }
        }

        public static Material AddToTargetMaterial
        {
            get
            {
                if (!addToTargetMaterial)
                {
                    Shader shader = GetShader(@"Assets/RW_PlanetAtmosphere/Shader/AddToTarget.shader");
                    addToTargetMaterial = new Material(shader);
                }
                return addToTargetMaterial;
            }
        }

        public abstract bool IsVolum { get; }

        public abstract int Order { get; }

        public virtual void BeforeRendering(CommandBuffer commandBuffer, Camera camera){}
        // addition
        public abstract void GenBaseColor(CommandBuffer commandBuffer, TransparentObject target, object targetSignal, Camera camera, object signal, RenderTargetIdentifier[] colors, RenderTargetIdentifier depth);
        // multiplication
        public abstract void BlendShadow(CommandBuffer commandBuffer, TransparentObject target, object targetSignal, Camera camera, object signal, RenderTargetIdentifier[] colors, RenderTargetIdentifier depth);
        // addition
        public abstract void BlendLumen(CommandBuffer commandBuffer, TransparentObject target, object targetSignal, Camera camera, object signal, RenderTargetIdentifier[] colors, RenderTargetIdentifier depth);
        // multiplication
        public abstract void BlendTrans(CommandBuffer commandBuffer, TransparentObject target, object targetSignal, Camera camera, object signal, RenderTargetIdentifier[] colors, RenderTargetIdentifier depth);
        
        public virtual void AfterRendering(CommandBuffer commandBuffer, Camera camera){}

        public virtual IEnumerator GetEnumerator()
        {
            yield return 0;
        }


        public static Shader GetShader(string path)
        {
            //if (modAsset == null)
            //{
            //    foreach (ModContentPack pack in LoadedModManager.RunningModsListForReading)
            //    {
            //        if (pack.PackageId.Equals("rwnodetree.rwplanetatmosphere") && !pack.assetBundles.loadedAssetBundles.NullOrEmpty())
            //        {
            //            modAsset = pack;
            //        }
            //    }
            //}
            //if (modAsset != null)
            //{
            //    foreach (AssetBundle assetBundle in modAsset.assetBundles.loadedAssetBundles)
            //    {
            //        // Log.Message($"Loading shader in {assetBundle.name}");
            //        Shader shader = assetBundle.LoadAsset<Shader>(path);
            //        if (shader != null && shader.isSupported) return shader;
            //    }
            //}
            //return null;
            return shaders.TryGetValue(path, out var shader) ? shader : null;
        }
        public static Texture2D GetTexture2D(string path)
        {
            //if (modAsset == null)
            //{
            //    foreach (ModContentPack pack in LoadedModManager.RunningModsListForReading)
            //    {
            //        if (pack.PackageId.Equals("rwnodetree.rwplanetatmosphere") && !pack.assetBundles.loadedAssetBundles.NullOrEmpty())
            //        {
            //            modAsset = pack;
            //        }
            //    }
            //}
            //if (modAsset != null)
            //{
            //    foreach (AssetBundle assetBundle in modAsset.assetBundles.loadedAssetBundles)
            //    {
            //        // Log.Message($"Loading shader in {assetBundle.name}");
            //        Shader shader = assetBundle.LoadAsset<Shader>(path);
            //        if (shader != null && shader.isSupported) return shader;
            //    }
            //}
            //return null;
            return texture2Ds.TryGetValue(path, out var texture2D) ? texture2D : null;
        }


        public static void DrawTransparentObjects(List<TransparentObject> transparentObjects, CommandBuffer commandBuffer, Camera camera, Action<CommandBuffer> beforeBackgroundBlendShadow = null, Action<CommandBuffer> backgroundBlendLumen = null, Action<CommandBuffer> afterBackgroundBlendTrans = null)
        {
            if (transparentObjects != null && commandBuffer != null && camera)
            {
                transparentObjects = new List<TransparentObject>(transparentObjects);
                transparentObjects.RemoveAll(x => x == null);
                transparentObjects.Sort((x,y)=>x.Order - y.Order);
                commandBuffer.Clear();
                commandBuffer.GetTemporaryRT(Reflection, -1, -1, 24, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
                commandBuffer.GetTemporaryRT(DepthTexel, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
                commandBuffer.GetTemporaryRT(Reflection_volum, -1, -1, 24, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
                commandBuffer.GetTemporaryRT(DepthTexel_volum, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);

                transparentObjects.ForEach(x => x.BeforeRendering(commandBuffer, camera));
                BuiltinRenderTextureType cameraDepth = camera.actualRenderingPath > RenderingPath.Forward ? BuiltinRenderTextureType.ResolvedDepth : BuiltinRenderTextureType.Depth;

                commandBuffer.SetGlobalFloat(propId_sunRadius, sunRadius);
                commandBuffer.SetGlobalFloat(propId_sunDistance, sunDistance);
                commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                commandBuffer.SetGlobalTexture(CameraDepthTexture, cameraDepth);
                if(beforeBackgroundBlendShadow != null) beforeBackgroundBlendShadow(commandBuffer);
                for (int i = 0; i < transparentObjects.Count; i++)
                {
                    TransparentObject obj0 = transparentObjects[i];
                    foreach (object t0 in obj0)
                    {
                        commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                        commandBuffer.SetGlobalTexture(CameraDepthTexture, cameraDepth);
                        obj0.BlendShadow(commandBuffer, null, null, camera, t0, new RenderTargetIdentifier[] { BuiltinRenderTextureType.CameraTarget }, BuiltinRenderTextureType.CameraTarget);
                    }
                }
                commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                commandBuffer.SetGlobalTexture(CameraDepthTexture, cameraDepth);
                if (backgroundBlendLumen != null) backgroundBlendLumen(commandBuffer);
                for (int i = 0; i < transparentObjects.Count; i++)
                {
                    TransparentObject obj0 = transparentObjects[i];
                    foreach (object t0 in obj0)
                    {
                        commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                        commandBuffer.SetGlobalTexture(CameraDepthTexture, cameraDepth);
                        obj0.BlendTrans(commandBuffer, null, null, camera, t0, new RenderTargetIdentifier[] { BuiltinRenderTextureType.CameraTarget }, BuiltinRenderTextureType.CameraTarget);
                    }
                }
                commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                commandBuffer.SetGlobalTexture(CameraDepthTexture, cameraDepth);
                if (afterBackgroundBlendTrans != null) afterBackgroundBlendTrans(commandBuffer);
                for (int i = 0; i < transparentObjects.Count; i++)
                {
                    TransparentObject obj0 = transparentObjects[i];
                    foreach (object t0 in obj0)
                    {
                        commandBuffer.SetRenderTarget(new RenderTargetIdentifier[] { Reflection, DepthTexel }, Reflection);
                        commandBuffer.SetGlobalTexture(CameraDepthTexture, cameraDepth);
                        commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.identity, CopyToDepthMaterial, 0, 0);
                        obj0.GenBaseColor(commandBuffer, null, null, camera, t0, new RenderTargetIdentifier[] { Reflection, DepthTexel }, Reflection);
                        for (int j = 0; j < transparentObjects.Count; j++)
                        {
                            TransparentObject obj1 = transparentObjects[j];
                            foreach (object t1 in obj1)
                            {
                                if (t0 != null && (i != j || !t0.Equals(t1)))
                                {
                                    commandBuffer.SetRenderTarget(Reflection);
                                    commandBuffer.SetGlobalTexture(CameraDepthTexture, DepthTexel);
                                    obj1.BlendShadow(commandBuffer, obj0, t0, camera, t1, new RenderTargetIdentifier[] { Reflection }, Reflection);
                                }
                            }
                        }
                        commandBuffer.SetRenderTarget(new RenderTargetIdentifier[] { Reflection, DepthTexel }, Reflection);
                        commandBuffer.SetGlobalTexture(CameraDepthTexture, cameraDepth);
                        obj0.BlendLumen(commandBuffer, null, null, camera, t0, new RenderTargetIdentifier[] { Reflection, DepthTexel }, Reflection);
                        for (int j = 0; j < transparentObjects.Count; j++)
                        {
                            TransparentObject obj1 = transparentObjects[j];
                            foreach (object t1 in obj1)
                            {
                                if (t0 != null && (i != j || !t0.Equals(t1)))
                                {
                                    commandBuffer.SetRenderTarget(Reflection);
                                    commandBuffer.SetGlobalTexture(CameraDepthTexture, DepthTexel);
                                    obj1.BlendTrans(commandBuffer, obj0, t0, camera, t1, new RenderTargetIdentifier[] { Reflection }, Reflection);
                                }
                            }
                        }
                        for (int j = 0; j < transparentObjects.Count; j++)
                        {
                            TransparentObject STLayer = transparentObjects[j];
                            if (STLayer.IsVolum)
                            {
                                foreach (object t1 in STLayer)
                                {
                                    if (t0 != null && (i != j || !t0.Equals(t1)))
                                    {
                                        commandBuffer.SetRenderTarget(new RenderTargetIdentifier[] { Reflection_volum, DepthTexel_volum }, Reflection_volum);
                                        commandBuffer.SetGlobalTexture(CameraDepthTexture, DepthTexel);
                                        commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.identity, CopyToDepthMaterial, 0, 0);
                                        STLayer.GenBaseColor(commandBuffer, obj0, t0, camera, t1, new RenderTargetIdentifier[] { Reflection_volum, DepthTexel_volum }, Reflection_volum);
                                        for (int k = 0; k < transparentObjects.Count; k++)
                                        {
                                            TransparentObject obj2 = transparentObjects[k];
                                            foreach (object t2 in obj2)
                                            {
                                                if (t1 != null && (j != k || !t1.Equals(t2)))
                                                {
                                                    commandBuffer.SetRenderTarget(Reflection_volum);
                                                    commandBuffer.SetGlobalTexture(CameraDepthTexture, DepthTexel_volum);
                                                    obj2.BlendShadow(commandBuffer, STLayer, t1, camera, t2, new RenderTargetIdentifier[] { Reflection_volum }, Reflection_volum);
                                                }
                                            }
                                        }
                                        commandBuffer.SetRenderTarget(new RenderTargetIdentifier[] { Reflection_volum, DepthTexel_volum }, Reflection_volum);
                                        commandBuffer.SetGlobalTexture(CameraDepthTexture, DepthTexel);
                                        STLayer.BlendLumen(commandBuffer, obj0, t0, camera, t1, new RenderTargetIdentifier[] { Reflection_volum, DepthTexel_volum }, Reflection_volum);
                                        for (int k = 0; k < transparentObjects.Count; k++)
                                        {
                                            TransparentObject obj2 = transparentObjects[k];
                                            foreach (object t2 in obj2)
                                            {
                                                if (t1 != null && (j != k || !t1.Equals(t2)))
                                                {
                                                    commandBuffer.SetRenderTarget(Reflection_volum);
                                                    commandBuffer.SetGlobalTexture(CameraDepthTexture, DepthTexel_volum);
                                                    obj2.BlendTrans(commandBuffer, STLayer, t1, camera, t2, new RenderTargetIdentifier[] { Reflection_volum }, Reflection_volum);
                                                }
                                            }
                                        }
                                        //commandBuffer.SetRenderTarget(Reflection);
                                        //commandBuffer.SetGlobalTexture(CameraDepthTexture, DepthTexel);
                                        commandBuffer.SetGlobalTexture(ColorTex, Reflection_volum);
                                        commandBuffer.SetGlobalTexture(DepthTex, DepthTexel_volum);
                                        commandBuffer.SetGlobalColor(MainColor, Color.white);
                                        commandBuffer.Blit(null, Reflection, AddToTargetMaterial, 0);
                                    }
                                }
                            }
                        }
                        //commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                        //commandBuffer.SetGlobalTexture(CameraDepthTexture, cameraDepth);
                        commandBuffer.SetGlobalTexture(ColorTex, Reflection);
                        commandBuffer.SetGlobalTexture(DepthTex, DepthTexel);
                        commandBuffer.SetGlobalColor(MainColor, Color.white);
                        commandBuffer.Blit(null, BuiltinRenderTextureType.CameraTarget, AddToTargetMaterial, obj0.IsVolum ? 0 : 1);
                    }
                }
                commandBuffer.ReleaseTemporaryRT(Reflection);
                commandBuffer.ReleaseTemporaryRT(DepthTexel);
                commandBuffer.ReleaseTemporaryRT(Reflection_volum);
                commandBuffer.ReleaseTemporaryRT(DepthTexel_volum);
                transparentObjects.ForEach(x => x.AfterRendering(commandBuffer, camera));
            }
        }

        // public abstract void ExposeData();
    }

}