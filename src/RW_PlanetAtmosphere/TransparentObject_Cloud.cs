using UnityEngine;
using System;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;
using Verse;
using RimWorld.Planet;

namespace RW_PlanetAtmosphere
{
    public class TransparentObject_Cloud : TransparentObject
    {
        float opacityVel = 0;
        float targetOpacity = 0;
        public bool renderingShadow     = true;
        public float refraction         = 2;
        public float luminescen         = 0;
        public float opacity            = 1;
        //public float playRange          = 0.015625f;
        //public float flowDir            = 0;
        public float radius             = 63.76393f;
        public float diffusePower       = 16;
        public Vector3 normal           = Vector3.up;
        public Vector3 tangent          = Vector3.right;
        public Vector3 postion          = Vector3.zero;
        public string cloudTexturePath  = null;
        //public string noiseTexturePath  = null;

        private Texture2D cloudTexture;
        //private Texture2D noiseTexture;
        private Material materialSkyBoxCloud;

        private static Shader SkyBoxCloud;

        #region propsIDs

        private static readonly int propId_refraction   = Shader.PropertyToID("refraction");
        private static readonly int propId_luminescen   = Shader.PropertyToID("luminescen");
        private static readonly int propId_opacity      = Shader.PropertyToID("opacity");
        //private static readonly int propId_playRange    = Shader.PropertyToID("playRange");
        //private static readonly int propId_flowDir      = Shader.PropertyToID("flowDir");
        private static readonly int propId_radius       = Shader.PropertyToID("radius");
        private static readonly int propId_diffusePower = Shader.PropertyToID("diffusePower");
        private static readonly int propId_normal       = Shader.PropertyToID("normal");
        private static readonly int propId_tangent      = Shader.PropertyToID("tangent");
        private static readonly int propId_cloudTexture = Shader.PropertyToID("cloudTexture");

        #endregion

        public TransparentObject_Cloud() { }

        public TransparentObject_Cloud(CloudDef cloudDef)
        {
            if (cloudDef != null)
            {
                renderingShadow     = cloudDef.renderingShadow;
                refraction          = cloudDef.refraction;
                luminescen          = cloudDef.luminescen;
                opacity             = cloudDef.opacity;
                radius              = cloudDef.radius;
                diffusePower        = cloudDef.diffusePower;
                normal              = cloudDef.normal;
                tangent             = cloudDef.tangent;
                postion             = cloudDef.postion;
                cloudTexturePath    = cloudDef.cloudTexturePath;
            }
        }
        public override bool IsVolum => false;

        public override int Order => 0;


        void UpdateMaterial(Material material)
        {
            if (material == null) return;

            material.SetFloat(propId_refraction, refraction);
            material.SetFloat(propId_luminescen, luminescen);
#if V13 || V14 || V15
            material.SetFloat(propId_opacity, opacity);
#else
            material.SetFloat(propId_opacity, targetOpacity);
#endif
            //material.SetFloat(propId_playRange, playRange);
            //material.SetFloat(propId_flowDir, flowDir);
            material.SetFloat(propId_radius, radius);
            material.SetFloat(propId_diffusePower, diffusePower);

            material.SetVector(propId_normal, normal);
            material.SetVector(propId_tangent, tangent);

            if (cloudTexture) material.SetTexture(propId_cloudTexture, cloudTexture);
            //if (noiseTexture) material.SetTexture(propId_noiseTexture, noiseTexture);
        }

        private static bool init()
        {
            if (!SkyBoxCloud)
                SkyBoxCloud = GetShader(@"Assets/RW_PlanetAtmosphere/Shader/Cloud/SkyBoxCloud.shader");
            return SkyBoxCloud;
        }

        private bool initObject()
        {
            if(init())
            {
                if (cloudTexturePath != null && cloudTexturePath.Length > 0)
                    cloudTexture = GetTexture2D(cloudTexturePath);
                //if (noiseTexturePath != null && noiseTexturePath.Length > 0)
                //    noiseTexture = GetTexture2D(noiseTexturePath);
                if (!materialSkyBoxCloud)
                    materialSkyBoxCloud = new Material(SkyBoxCloud);
                if(materialSkyBoxCloud)
                {
                    UpdateMaterial(materialSkyBoxCloud);
                    return true;
                }
            }
            return false;
        }

        public override void GenBaseColor(CommandBuffer commandBuffer, TransparentObject target, object targetSignal, Camera camera, object signal, RenderTargetIdentifier[] colors, RenderTargetIdentifier depth)
        {
            bool signalTranslated = (bool)signal;

            if(signalTranslated)
            {
#if V13 || V14 || V15
#else
                if (ModsConfig.OdysseyActive)
                    targetOpacity = Mathf.SmoothDamp(targetOpacity, WorldRendererUtility.WorldBackgroundNow ? opacity : 0, ref opacityVel, 0.15f);
                else
#endif
                    targetOpacity = Mathf.SmoothDamp(targetOpacity, Find.WorldCameraDriver.AltitudePercent >= 0.75f ? opacity : 0, ref opacityVel, 0.15f);
            }
            if (initObject() && targetOpacity > 0)
            {
                if (signalTranslated)
                {
                    commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialSkyBoxCloud, 0, 3);
                }
                else
                {
                    commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialSkyBoxCloud, 0, 4);
                }
            }
        }

        public override void BlendShadow(CommandBuffer commandBuffer, TransparentObject target, object targetSignal, Camera camera, object signal, RenderTargetIdentifier[] colors, RenderTargetIdentifier depth)
        {
            if (!renderingShadow || target == this || target is TransparentObject_Atmosphere) return;
            TransparentObject_Cloud cloud = target as TransparentObject_Cloud;
            if (cloud != null && cloud.refraction <= 0) return;
            TransparentObject_Ring ring = target as TransparentObject_Ring;
            if (ring != null && ring.refraction <= 0) return;
            if (initObject() && targetOpacity > 0)
            {
                bool signalTranslated = (bool)signal;
                if (signalTranslated)
                {
                    commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialSkyBoxCloud, 0, 0);
                }
            }
        }


        public override void BlendLumen(CommandBuffer commandBuffer, TransparentObject target, object targetSignal, Camera camera, object signal, RenderTargetIdentifier[] colors, RenderTargetIdentifier depth)
        {
            if (luminescen <= 0) return;
            if (initObject() && targetOpacity > 0)
            {
                bool signalTranslated = (bool)signal;
                if (signalTranslated)
                {
                    commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialSkyBoxCloud, 0, 5);
                }
                else
                {
                    commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialSkyBoxCloud, 0, 6);
                }
            }
        }


        public override void BlendTrans(CommandBuffer commandBuffer, TransparentObject target, object targetSignal, Camera camera, object signal, RenderTargetIdentifier[] colors, RenderTargetIdentifier depth)
        {
            if (target != null && target.IsVolum) return;
            TransparentObject_Cloud cloud = target as TransparentObject_Cloud;
            if (cloud != null && cloud.refraction <= 0 && cloud.luminescen <= 0) return;
            TransparentObject_Ring ring = target as TransparentObject_Ring;
            if (ring != null && ring.refraction <= 0 && cloud.luminescen <= 0) return;
            if (initObject() && targetOpacity > 0)
            {
                bool signalTranslated = (bool)signal;
                if (signalTranslated)
                {
                    commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialSkyBoxCloud, 0, 1);
                }
                else
                {
                    commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialSkyBoxCloud, 0, 2);
                }
            }
        }
        
        public override IEnumerator GetEnumerator()
        {
            yield return true;
            yield return false;
        }

        public override float SettingGUI(float posY, float width, Vector2 outFromTo)
        {
            HelperMethod_GUI.GUIBoolean(ref posY, ref renderingShadow, "renderingShadow".Translate(),width,outFromTo);
            HelperMethod_GUI.GUIFloat(ref posY, ref refraction, "refraction".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY, ref luminescen, "luminescen".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY, ref opacity, "opacity".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY, ref radius, "radius".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY, ref diffusePower, "diffusePower".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec3(ref posY, ref normal, "normal".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec3(ref posY, ref tangent, "tangent".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec3(ref posY, ref postion, "postion".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIString(ref posY, ref cloudTexturePath, "cloudTexturePath".Translate(),width,outFromTo);
            return posY;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref renderingShadow,"renderingShadow",true,true);
            Scribe_Values.Look(ref cloudTexturePath,"cloudTexturePath","EarthCloudTex/8k_earth_clouds",true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref refraction,"refraction",6,2,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref luminescen,"luminescen",6,0,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref opacity,"opacity",6,1,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref radius,"radius",6,63.76393f * AtmosphereSettings.scale,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref diffusePower,"diffusePower",6,16,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec3(ref normal,"normal",6,Vector3.up,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec3(ref tangent,"tangent",6,Vector3.right,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec3(ref postion,"postion",6,Vector3.zero,true);

        }

        ~TransparentObject_Cloud()
        {
            if(materialSkyBoxCloud) GameObject.Destroy(materialSkyBoxCloud);
        }
    }
}