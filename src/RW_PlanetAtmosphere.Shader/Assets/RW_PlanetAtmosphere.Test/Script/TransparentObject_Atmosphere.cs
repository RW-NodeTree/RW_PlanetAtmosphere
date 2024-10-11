using UnityEngine;
using System;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;

namespace RW_PlanetAtmosphere
{
    public class TransparentObject_Atmosphere : TransparentObject
    {
        public Vector2Int translucentLUTSize    = new Vector2Int(256, 256);
        public Vector2Int outSunLightLUTSize    = new Vector2Int(256, 256);
        public Vector2Int inSunLightLUTSize     = new Vector2Int(256, 256);
        public Vector4 scatterLUTSize           = new Vector4(128, 32, 8, 32);
        public Vector3 postion                  = Vector3.zero;


        public bool needUpdate                  = true;
        public float exposure                   = 16;
        public float deltaL                     = 8;
        public float deltaW                     = 2;
        public float lengthL                    = 1;
        public float lengthW                    = 1;
        public float sunRadius                  = 6960;
        public float sunDistance                = 1495978.92f;
        public float minh                       = 63.71393f;
        public float maxh                       = 64.21393f;
        public float H_Reayleigh                = 0.08f;
        public float H_Mie                      = 0.02f;
        public float H_OZone                    = 0.25f;
        public float D_OZone                    = 0.15f;
        public Vector4 reayleigh_scatter        = new Vector4(0.46278f, 1.25945f, 3.10319f, 11.69904f);
        public Vector4 molecule_absorb          = Vector4.zero;
        public Vector4 OZone_absorb             = new Vector4(0.21195f, 0.20962f, 0.01686f, 6.4f);
        public Vector4 mie_scatter              = new Vector4(3.996f, 3.996f, 3.996f, 3.996f);
        public Vector4 mie_absorb               = new Vector4(4.44f, 4.44f, 4.44f, 4.44f);
        public Vector4 mie_eccentricity         = new Vector4(0.618f, 0.618f, 0.618f, 0.618f);

        private Material materialAtmosphereLUT = null;
        private Material materialTranslucentGenrater = null;
        private Material materialOutSunLightLUTGenrater = null;
        private Material materialInSunLightLUTGenrater = null;
        private Material materialScatterGenrater = null;
        private RenderTexture translucentLUT = null;
        private RenderTexture outSunLightLUT = null;
        private RenderTexture inSunLightLUT = null;
        private RenderTexture scatterLUT_Reayleigh = null;
        private RenderTexture scatterLUT_Mie = null;

        private static Shader AtmosphereLUT = null;
        private static Shader TranslucentGenrater = null;
        private static Shader OutSunLightLUTGenrater = null;
        private static Shader InSunLightLUTGenrater = null;
        private static Shader ScatterGenrater = null;
        // Start is called before the first frame update


        #region propsIDs

        private static readonly int propId_exposure              = Shader.PropertyToID("exposure");
        private static readonly int propId_deltaL                = Shader.PropertyToID("deltaL");
        private static readonly int propId_deltaW                = Shader.PropertyToID("deltaW");
        private static readonly int propId_lengthL               = Shader.PropertyToID("lengthL");
        private static readonly int propId_lengthW               = Shader.PropertyToID("lengthW");
        private static readonly int propId_sunRadius             = Shader.PropertyToID("sunRadius");
        private static readonly int propId_sunDistance           = Shader.PropertyToID("sunDistance");
        private static readonly int propId_minh                  = Shader.PropertyToID("minh");
        private static readonly int propId_maxh                  = Shader.PropertyToID("maxh");
        private static readonly int propId_H_Reayleigh           = Shader.PropertyToID("H_Reayleigh");
        private static readonly int propId_H_Mie                 = Shader.PropertyToID("H_Mie");
        private static readonly int propId_H_OZone               = Shader.PropertyToID("H_OZone");
        private static readonly int propId_D_OZone               = Shader.PropertyToID("D_OZone");
        private static readonly int propId_reayleigh_scatter     = Shader.PropertyToID("reayleigh_scatter");
        private static readonly int propId_molecule_absorb       = Shader.PropertyToID("molecule_absorb");
        private static readonly int propId_OZone_absorb          = Shader.PropertyToID("OZone_absorb");
        private static readonly int propId_mie_scatter           = Shader.PropertyToID("mie_scatter");
        private static readonly int propId_mie_absorb            = Shader.PropertyToID("mie_absorb");
        private static readonly int propId_mie_eccentricity      = Shader.PropertyToID("mie_eccentricity");
        private static readonly int propId_scatterLUTSize        = Shader.PropertyToID("scatterLUTSize");
        private static readonly int propId_translucentLUT        = Shader.PropertyToID("translucentLUT");
        private static readonly int propId_outSunLightLUT        = Shader.PropertyToID("outSunLightLUT");
        private static readonly int propId_inSunLightLUT         = Shader.PropertyToID("inSunLightLUT");
        private static readonly int propId_scatterLUT_Reayleigh  = Shader.PropertyToID("scatterLUT_Reayleigh");
        private static readonly int propId_scatterLUT_Mie        = Shader.PropertyToID("scatterLUT_Mie");

        #endregion

        public TransparentObject_Atmosphere(){}

        public TransparentObject_Atmosphere(AtmosphereDef atmosphereDef)
        {
            if(atmosphereDef!= null)
            {
                translucentLUTSize  = atmosphereDef.translucentLUTSize;
                outSunLightLUTSize  = atmosphereDef.outSunLightLUTSize;
                inSunLightLUTSize   = atmosphereDef.inSunLightLUTSize;
                scatterLUTSize      = atmosphereDef.scatterLUTSize;
                postion             = atmosphereDef.postion;
                exposure            = atmosphereDef.exposure;
                deltaL              = atmosphereDef.deltaL;
                deltaW              = atmosphereDef.deltaW;
                lengthL             = atmosphereDef.lengthL;
                lengthW             = atmosphereDef.lengthW;
                sunRadius           = atmosphereDef.sunRadius;
                sunDistance         = atmosphereDef.sunDistance;
                minh                = atmosphereDef.minh;
                maxh                = atmosphereDef.maxh;
                H_Reayleigh         = atmosphereDef.H_Reayleigh;
                H_Mie               = atmosphereDef.H_Mie;
                H_OZone             = atmosphereDef.H_OZone;
                D_OZone             = atmosphereDef.D_OZone;
                reayleigh_scatter   = atmosphereDef.reayleigh_scatter;
                molecule_absorb     = atmosphereDef.molecule_absorb;
                OZone_absorb        = atmosphereDef.OZone_absorb;
                mie_scatter         = atmosphereDef.mie_scatter;
                mie_absorb          = atmosphereDef.mie_absorb;
                mie_eccentricity    = atmosphereDef.mie_eccentricity;
            }
        }

        public override bool IsVolum => maxh > minh;

        public override int Order => 0;

        void UpdateMaterialDyn(Material material)
        {
            if (material == null) return;

            material.SetFloat(propId_exposure, exposure);

            material.SetVector(propId_mie_eccentricity, mie_eccentricity);
        }

        void UpdateMaterialStatic(Material material)
        {
            if (material == null) return;

            material.SetFloat(propId_deltaL, deltaL);
            material.SetFloat(propId_deltaW, deltaW);
            material.SetFloat(propId_lengthL, lengthL);
            material.SetFloat(propId_lengthW, lengthW);
            material.SetFloat(propId_sunRadius, sunRadius);
            material.SetFloat(propId_sunDistance, sunDistance);
            material.SetFloat(propId_minh, minh);
            material.SetFloat(propId_maxh, maxh);
            material.SetFloat(propId_H_Reayleigh, H_Reayleigh);
            material.SetFloat(propId_H_Mie, H_Mie);
            material.SetFloat(propId_H_OZone, H_OZone);
            material.SetFloat(propId_D_OZone, D_OZone);

            material.SetVector(propId_reayleigh_scatter, reayleigh_scatter);
            material.SetVector(propId_molecule_absorb, molecule_absorb);
            material.SetVector(propId_OZone_absorb, OZone_absorb);
            material.SetVector(propId_mie_scatter, mie_scatter);
            material.SetVector(propId_mie_absorb, mie_absorb);
        }

        void UpdateMaterialLUT(Material material)
        {
            if (material == null) return;

            material.SetVector(propId_scatterLUTSize, new Vector4((int)scatterLUTSize.x, (int)scatterLUTSize.y, (int)scatterLUTSize.z, (int)scatterLUTSize.w));
            if(translucentLUT)          material.SetTexture(propId_translucentLUT       , translucentLUT        );
            if(outSunLightLUT)          material.SetTexture(propId_outSunLightLUT       , outSunLightLUT        );
            if(inSunLightLUT)           material.SetTexture(propId_inSunLightLUT        , inSunLightLUT         );
            if(scatterLUT_Reayleigh)    material.SetTexture(propId_scatterLUT_Reayleigh , scatterLUT_Reayleigh  );
            if(scatterLUT_Mie)          material.SetTexture(propId_scatterLUT_Mie       , scatterLUT_Mie        );
        }

        private static bool init()
        {
            if(!AtmosphereLUT)
                AtmosphereLUT = GetShader(@"Assets/RW_PlanetAtmosphere/Shader/Atmosphere/AtmosphereLUT.shader");
            if (!TranslucentGenrater)
                TranslucentGenrater = GetShader(@"Assets/RW_PlanetAtmosphere/Shader/TranslucentGenrater.shader");
            if (!OutSunLightLUTGenrater)
                OutSunLightLUTGenrater = GetShader(@"Assets/RW_PlanetAtmosphere/Shader/OutSunLightLUTGenrater.shader");
            if (!InSunLightLUTGenrater)
                InSunLightLUTGenrater = GetShader(@"Assets/RW_PlanetAtmosphere/Shader/InSunLightLUTGenrater.shader");
            if (!ScatterGenrater)
                ScatterGenrater = GetShader(@"Assets/RW_PlanetAtmosphere/Shader/ScatterGenrater.shader");
            return AtmosphereLUT && TranslucentGenrater && OutSunLightLUTGenrater && InSunLightLUTGenrater && ScatterGenrater;

        }

        private bool initObject()
        {
            if(init())
            {
                float minh = Math.Min(this.minh,this.maxh);
                float maxh = Math.Max(this.minh,this.maxh);
                this.minh = minh;
                this.maxh = maxh;
                if (!materialAtmosphereLUT)
                    materialAtmosphereLUT = new Material(AtmosphereLUT);
                if (!materialTranslucentGenrater)
                    materialTranslucentGenrater = new Material(TranslucentGenrater);
                if (!materialOutSunLightLUTGenrater)
                    materialOutSunLightLUTGenrater = new Material(OutSunLightLUTGenrater);
                if (!materialInSunLightLUTGenrater)
                    materialInSunLightLUTGenrater = new Material(InSunLightLUTGenrater);
                if (!materialScatterGenrater)
                    materialScatterGenrater = new Material(ScatterGenrater);
                if(
                    materialAtmosphereLUT           &&
                    materialTranslucentGenrater     &&
                    materialOutSunLightLUTGenrater  &&
                    materialInSunLightLUTGenrater   &&
                    materialScatterGenrater
                    )
                {
                    if(IsVolum)
                    {
                        if (!translucentLUT)
                        {
                            translucentLUT = new RenderTexture(translucentLUTSize.x, translucentLUTSize.y, 0)
                            {
                                // enableRandomWrite = true,
                                useMipMap = false,
                                format = RenderTextureFormat.ARGBFloat,
                                wrapMode = TextureWrapMode.Clamp
                            };
                            translucentLUT.Create();
                        }
                        Vector2Int scatterLUTSize2D = new Vector2Int((int)scatterLUTSize.x * (int)scatterLUTSize.z, (int)scatterLUTSize.y * (int)scatterLUTSize.w);
                        if (!scatterLUT_Reayleigh)
                        {
                            scatterLUT_Reayleigh = new RenderTexture(scatterLUTSize2D.x, scatterLUTSize2D.y, 0)
                            {
                                // enableRandomWrite = true,
                                useMipMap = false,
                                format = RenderTextureFormat.ARGBHalf,
                                wrapMode = TextureWrapMode.Clamp
                            };
                            scatterLUT_Reayleigh.Create();
                        }
                        if (!scatterLUT_Mie)
                        {
                            scatterLUT_Mie = new RenderTexture(scatterLUTSize2D.x, scatterLUTSize2D.y, 0)
                            {
                                // enableRandomWrite = true,
                                useMipMap = false,
                                format = RenderTextureFormat.ARGBHalf,
                                wrapMode = TextureWrapMode.Clamp
                            };
                            scatterLUT_Mie.Create();
                        }
                    }
                    if (!inSunLightLUT)
                    {
                        inSunLightLUT = new RenderTexture(inSunLightLUTSize.x, inSunLightLUTSize.y, 0)
                        {
                            // enableRandomWrite = true,
                            useMipMap = false,
                            format = RenderTextureFormat.ARGBHalf,
                            wrapMode = TextureWrapMode.Clamp
                        };
                        inSunLightLUT.Create();
                    }
                    if (!outSunLightLUT)
                    {
                        outSunLightLUT = new RenderTexture(outSunLightLUTSize.x, outSunLightLUTSize.y, 0)
                        {
                            // enableRandomWrite = true,
                            useMipMap = false,
                            format = RenderTextureFormat.ARGBFloat,
                            wrapMode = TextureWrapMode.Clamp
                        };
                        outSunLightLUT.Create();
                    }

                    if (needUpdate)
                    {
                        needUpdate = false;
                        if (IsVolum)
                        {
                            UpdateMaterialLUT(materialTranslucentGenrater);
                            UpdateMaterialStatic(materialTranslucentGenrater);
                            Graphics.Blit(null, translucentLUT, materialTranslucentGenrater);
                        }

                        UpdateMaterialLUT(materialInSunLightLUTGenrater);
                        UpdateMaterialStatic(materialInSunLightLUTGenrater);
                        Graphics.Blit(null, inSunLightLUT, materialInSunLightLUTGenrater);

                        UpdateMaterialLUT(materialOutSunLightLUTGenrater);
                        UpdateMaterialStatic(materialOutSunLightLUTGenrater);
                        Graphics.Blit(null, outSunLightLUT, materialOutSunLightLUTGenrater);

                        if (IsVolum)
                        {
                            UpdateMaterialLUT(materialScatterGenrater);
                            UpdateMaterialStatic(materialScatterGenrater);
                            RenderTexture actived = RenderTexture.active;
                            Graphics.SetRenderTarget(new RenderBuffer[] { scatterLUT_Reayleigh.colorBuffer, scatterLUT_Mie.colorBuffer }, scatterLUT_Reayleigh.depthBuffer);
                            Graphics.Blit(null, materialScatterGenrater);
                            RenderTexture.active = actived;
                        }

                        UpdateMaterialLUT(materialAtmosphereLUT);
                        UpdateMaterialDyn(materialAtmosphereLUT);
                        UpdateMaterialStatic(materialAtmosphereLUT);
                    }
                    return true;
                }

            }
            return false;
        }

        public override void GenBaseColor(CommandBuffer commandBuffer, Camera camera, object signal)
        {
            if(initObject())
            {
                if (!IsVolum) return;
                //commandBuffer.ClearRenderTarget(false, true,new Color(0,0,0,1));
                commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialAtmosphereLUT, 0, 2);
            }
        }

        public override void BlendShadow(CommandBuffer commandBuffer, TransparentObject target, Camera camera, object signal)
        {
            if (initObject())
            {
                commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialAtmosphereLUT, 0, 0);
            }
        }

        public override void BlendLumen(CommandBuffer commandBuffer, Camera camera, object signal)
        {
            initObject();
        }

        public override void BlendTrans(CommandBuffer commandBuffer, TransparentObject target, Camera camera, object signal)
        {
            if (initObject())
            {
                if (!IsVolum) return;
                commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialAtmosphereLUT, 0, 1);
            }
        }

        ~TransparentObject_Atmosphere()
        {
            if(materialAtmosphereLUT) GameObject.Destroy(materialAtmosphereLUT);
            if(materialTranslucentGenrater) GameObject.Destroy(materialTranslucentGenrater);
            if(materialOutSunLightLUTGenrater) GameObject.Destroy(materialOutSunLightLUTGenrater);
            if(materialInSunLightLUTGenrater) GameObject.Destroy(materialInSunLightLUTGenrater);
            if(materialScatterGenrater) GameObject.Destroy(materialScatterGenrater);
            if(translucentLUT) GameObject.Destroy(translucentLUT);
            if(outSunLightLUT) GameObject.Destroy(outSunLightLUT);
            if(inSunLightLUT) GameObject.Destroy(inSunLightLUT);
            if(scatterLUT_Reayleigh) GameObject.Destroy(scatterLUT_Reayleigh);
            if(scatterLUT_Mie) GameObject.Destroy(scatterLUT_Mie);
        }
    }

}