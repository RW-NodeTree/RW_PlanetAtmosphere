using UnityEngine;
using System;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;
using Verse;

namespace RW_PlanetAtmosphere
{
    public class TransparentObject_Atmosphere : TransparentObject
    {
        public Vector2 translucentLUTSize       = new Vector2(256, 256);
        public Vector2 outSunLightLUTSize       = new Vector2(256, 256);
        public Vector2 inSunLightLUTSize        = new Vector2(256, 256);
        public Vector4 scatterLUTSize           = new Vector4(128, 32, 8, 32);
        public Vector3 postion                  = Vector3.zero;


        // public bool needUpdate                  = true;
        public float exposure                   = 16;
        public float deltaL                     = 8;
        public float deltaW                     = 2;
        public float lengthL                    = 1;
        public float lengthW                    = 1;
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


        private readonly Dictionary<(long distanceID, float sunRadius), (RenderTexture outSunLightLUT, RenderTexture inSunLightLUT, RenderTexture scatterLUT_Reayleigh, RenderTexture scatterLUT_Mie)> distanceLUTs = new Dictionary<(long distanceID, float sunRadius), (RenderTexture outSunLightLUT, RenderTexture inSunLightLUT, RenderTexture scatterLUT_Reayleigh, RenderTexture scatterLUT_Mie)>();

        // private bool forceReset = true;
        // private object currentCoveredSignal = null;
        // private TransparentObject currentCoveredObject = null;

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
        // private static readonly int propId_ahlwStartInfo         = Shader.PropertyToID("ahlwStartInfo");
        // private static readonly int propId_ahlwEndInfo           = Shader.PropertyToID("ahlwEndInfo");
        // private static readonly int propId_startDepthInfo        = Shader.PropertyToID("startDepthInfo");

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
                    if(needUpdate)
                    {
                        foreach(var value in distanceLUTs.Values)
                        {
                            if(value.inSunLightLUT) GameObject.Destroy(value.inSunLightLUT);
                            if(value.outSunLightLUT) GameObject.Destroy(value.outSunLightLUT);
                            if(value.scatterLUT_Mie) GameObject.Destroy(value.scatterLUT_Mie);
                            if(value.scatterLUT_Reayleigh) GameObject.Destroy(value.scatterLUT_Reayleigh);
                        }
                        distanceLUTs.Clear();
                    }
                    long id = (long)Math.Round(Math.Log(sunDistance, 1.0625f));
                    bool needUpdateLight = !distanceLUTs.TryGetValue((id, sunRadius), out var luts);
                    outSunLightLUT = luts.outSunLightLUT;
                    inSunLightLUT = luts.inSunLightLUT;
                    scatterLUT_Reayleigh = luts.scatterLUT_Reayleigh;
                    scatterLUT_Mie = luts.scatterLUT_Mie;
                    if(IsVolum)
                    {
                        if (!translucentLUT)
                        {
                            translucentLUT = new RenderTexture((int)translucentLUTSize.x, (int)translucentLUTSize.y, 0)
                            {
                                // enableRandomWrite = true,
                                useMipMap = false,
                                format = RenderTextureFormat.ARGBFloat,
                                wrapMode = TextureWrapMode.Clamp,
                            };
                            translucentLUT.Create();
                        }
                        Vector2Int scatterLutSize2D = new Vector2Int((int)scatterLUTSize.x * (int)scatterLUTSize.z, (int)scatterLUTSize.y * (int)scatterLUTSize.w);
                        if (!scatterLUT_Reayleigh)
                        {
                            scatterLUT_Reayleigh = new RenderTexture(scatterLutSize2D.x, scatterLutSize2D.y, 0)
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
                            scatterLUT_Mie = new RenderTexture(scatterLutSize2D.x, scatterLutSize2D.y, 0)
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
                        inSunLightLUT = new RenderTexture((int)inSunLightLUTSize.x, (int)inSunLightLUTSize.y, 0)
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
                        outSunLightLUT = new RenderTexture((int)outSunLightLUTSize.x, (int)outSunLightLUTSize.y, 0)
                        {
                            // enableRandomWrite = true,
                            useMipMap = false,
                            format = RenderTextureFormat.ARGBFloat,
                            wrapMode = TextureWrapMode.Clamp
                        };
                        outSunLightLUT.Create();
                    }
                    distanceLUTs[(id, sunRadius)] = (outSunLightLUT, inSunLightLUT, scatterLUT_Reayleigh, scatterLUT_Mie);

                    if (needUpdate || needUpdateLight)
                    {
                        CommandBuffer blitBuffer = new CommandBuffer();
                        if (IsVolum && needUpdate)
                        {
                            UpdateMaterialLUT(materialTranslucentGenrater);
                            UpdateMaterialStatic(materialTranslucentGenrater);
                            blitBuffer.Blit(null, translucentLUT, materialTranslucentGenrater);
                        }
                        needUpdate = false;

                        UpdateMaterialLUT(materialInSunLightLUTGenrater);
                        UpdateMaterialStatic(materialInSunLightLUTGenrater);
                        blitBuffer.Blit(null, inSunLightLUT, materialInSunLightLUTGenrater);

                        UpdateMaterialLUT(materialOutSunLightLUTGenrater);
                        UpdateMaterialStatic(materialOutSunLightLUTGenrater);
                        blitBuffer.Blit(null, outSunLightLUT, materialOutSunLightLUTGenrater);

                        if (IsVolum)
                        {
                            UpdateMaterialLUT(materialScatterGenrater);
                            UpdateMaterialStatic(materialScatterGenrater);
                            blitBuffer.SetRenderTarget(new RenderTargetIdentifier[] { scatterLUT_Reayleigh.colorBuffer, scatterLUT_Mie.colorBuffer }, scatterLUT_Reayleigh.depthBuffer);
                            blitBuffer.Blit(null,new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive), materialScatterGenrater);
                        }
                        
                        Graphics.ExecuteCommandBuffer(blitBuffer);

                        UpdateMaterialLUT(materialAtmosphereLUT);
                        UpdateMaterialDyn(materialAtmosphereLUT);
                        UpdateMaterialStatic(materialAtmosphereLUT);
                    }
                    return true;
                }

            }
            return false;
        }

        public override void GenBaseColor(CommandBuffer commandBuffer, TransparentObject target, object targetSignal, Camera camera, object signal, RenderTargetIdentifier[] colors, RenderTargetIdentifier depth)
        {
            if(initObject())
            {
                if (!IsVolum) return;
                TransparentObject_Cloud cloud = target as TransparentObject_Cloud;
                if (cloud != null && cloud.targetOpacity <= 0) return;
                TransparentObject_Ring ring = target as TransparentObject_Ring;
                if (ring != null && ring.targetOpacity <= 0) return;
                commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialAtmosphereLUT, 0, 2);
            }
        }

        public override void BlendShadow(CommandBuffer commandBuffer, TransparentObject target, object targetSignal, Camera camera, object signal, RenderTargetIdentifier[] colors, RenderTargetIdentifier depth)
        {
            TransparentObject_Cloud cloud = target as TransparentObject_Cloud;
            if (cloud != null && (cloud.targetOpacity <= 0 || cloud.refraction <= 0)) return;
            TransparentObject_Ring ring = target as TransparentObject_Ring;
            if (ring != null && (ring.targetOpacity <= 0 || ring.refraction <= 0)) return;
            if (initObject())
            {
                commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialAtmosphereLUT, 0, 0);
            }
        }

        public override void BlendLumen(CommandBuffer commandBuffer, TransparentObject target, object targetSignal, Camera camera, object signal, RenderTargetIdentifier[] colors, RenderTargetIdentifier depth)
        {
            initObject();
        }

        public override void BlendTrans(CommandBuffer commandBuffer, TransparentObject target, object targetSignal, Camera camera, object signal, RenderTargetIdentifier[] colors, RenderTargetIdentifier depth)
        {
            if (!IsVolum) return;
            TransparentObject_Cloud cloud = target as TransparentObject_Cloud;
            if (cloud != null && (cloud.targetOpacity <= 0 || (cloud.refraction <= 0 && cloud.luminescen <= 0))) return;
            TransparentObject_Ring ring = target as TransparentObject_Ring;
            if (ring != null && (ring.targetOpacity <= 0 || (ring.refraction <= 0 && ring.luminescen <= 0))) return;
            if (initObject())
            {
                commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialAtmosphereLUT, 0, 1);
            }
        }

        public override float SettingGUI(float posY, float width, Vector2 outFromTo)
        {
            HelperMethod_GUI.GUIVec2(ref posY,ref translucentLUTSize,"translucentLUTSize".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec2(ref posY,ref outSunLightLUTSize,"outSunLightLUTSize".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec2(ref posY,ref inSunLightLUTSize,"inSunLightLUTSize".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec4(ref posY,ref scatterLUTSize,"scatterLUTSize".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec3(ref posY,ref postion,"postion".Translate(),width,outFromTo,6);

            HelperMethod_GUI.GUIFloat(ref posY,ref exposure,"exposure".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY,ref deltaL,"deltaL".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY,ref deltaW,"deltaW".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY,ref lengthL,"lengthL".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY,ref lengthW,"lengthW".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY,ref minh,"minh".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY,ref maxh,"maxh".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY,ref H_Reayleigh,"H_Reayleigh".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY,ref H_Mie,"H_Mie".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY,ref H_OZone,"H_OZone".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY,ref D_OZone,"D_OZone".Translate(),width,outFromTo,6);

            HelperMethod_GUI.GUIVec4(ref posY,ref reayleigh_scatter,"reayleigh_scatter".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec4(ref posY,ref molecule_absorb,"molecule_absorb".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec4(ref posY,ref OZone_absorb,"OZone_absorb".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec4(ref posY,ref mie_scatter,"mie_scatter".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec4(ref posY,ref mie_absorb,"mie_absorb".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec4(ref posY,ref mie_eccentricity,"mie_eccentricity".Translate(),width,outFromTo,6);
            return posY;
        }

        public override void ExposeData()
        {
            HelperMethod_Scribe_Values.SaveAndLoadValueVec2(ref translucentLUTSize,"translucentLUTSize",6,new Vector2(256, 256),true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec2(ref outSunLightLUTSize,"outSunLightLUTSize",6,new Vector2(256, 256),true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec2(ref inSunLightLUTSize,"inSunLightLUTSize",6,new Vector2(256, 256),true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec4(ref scatterLUTSize,"scatterLUTSize",6,new Vector4(128, 32, 8, 32),true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec3(ref postion,"postion",6,Vector3.zero,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref exposure,"exposure",6,16,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref deltaL,"deltaL",6,8,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref deltaW,"deltaW",6,2,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref lengthL,"lengthL",6,1,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref lengthW,"lengthW",6,1,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref minh,"minh",6,63.71393f * AtmosphereSettings.scale,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref maxh,"maxh",6,64.71393f * AtmosphereSettings.scale,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref H_Reayleigh,"H_Reayleigh",6,0.08f * AtmosphereSettings.scale,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref H_Mie,"H_Mie",6,0.02f * AtmosphereSettings.scale,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref H_OZone,"H_OZone",6,0.25f * AtmosphereSettings.scale,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref D_OZone,"D_OZone",6,0.15f * AtmosphereSettings.scale,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec4(ref reayleigh_scatter,"reayleigh_scatter",6,new Vector4(0.46278f, 1.25945f, 3.10319f, 11.69904f) / AtmosphereSettings.scale,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec4(ref molecule_absorb,"molecule_absorb",6,Vector4.zero / AtmosphereSettings.scale,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec4(ref OZone_absorb,"OZone_absorb",6,new Vector4(0.21195f, 0.20962f, 0.01686f, 6.4f) / AtmosphereSettings.scale,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec4(ref mie_scatter,"mie_scatter",6,new Vector4(3.996f, 3.996f, 3.996f, 3.996f) / AtmosphereSettings.scale,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec4(ref mie_absorb,"mie_absorb",6,new Vector4(4.44f, 4.44f, 4.44f, 4.44f) / AtmosphereSettings.scale,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec4(ref mie_eccentricity,"mie_eccentricity",6,new Vector4(0.618f, 0.618f, 0.618f, 0.618f),true);
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