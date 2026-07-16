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
        internal float targetOpacity = 0;
        public bool renderingShadow     = true;
        public bool hideWhenGetClose    = true;
        public float refraction         = 2;
        public float luminescen         = 0;
        public float opacity            = 1;
        public float opacityType        = 0;
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

        private static readonly int propId_refraction   = Shader.PropertyToID(nameof(refraction));
        private static readonly int propId_luminescen   = Shader.PropertyToID(nameof(luminescen));
        private static readonly int propId_opacity      = Shader.PropertyToID(nameof(opacity));
        private static readonly int propId_opacityType  = Shader.PropertyToID(nameof(opacityType));
        //private static readonly int propId_playRange    = Shader.PropertyToID(nameof(playRange));
        //private static readonly int propId_flowDir      = Shader.PropertyToID(nameof(flowDir));
        private static readonly int propId_radius       = Shader.PropertyToID(nameof(radius));
        private static readonly int propId_diffusePower = Shader.PropertyToID(nameof(diffusePower));
        private static readonly int propId_normal       = Shader.PropertyToID(nameof(normal));
        private static readonly int propId_tangent      = Shader.PropertyToID(nameof(tangent));
        private static readonly int propId_cloudTexture = Shader.PropertyToID(nameof(cloudTexture));

        #endregion

        public TransparentObject_Cloud() { }

        public TransparentObject_Cloud(CloudDef cloudDef)
        {
            if (cloudDef != null)
            {
                renderingShadow     = cloudDef.renderingShadow;
                hideWhenGetClose    = cloudDef.hideWhenGetClose;
                refraction          = cloudDef.refraction;
                luminescen          = cloudDef.luminescen;
                opacity             = cloudDef.opacity;
                opacityType         = cloudDef.opacityType;
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
            if (!material) return;

            material.SetFloat(propId_refraction, refraction);
            material.SetFloat(propId_luminescen, luminescen);
            material.SetFloat(propId_opacity, targetOpacity);
            material.SetFloat(propId_opacityType, opacityType);
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
                SkyBoxCloud = GetShader(@"Assets/RW_PlanetAtmosphere/Resources/Shader/Cloud/SkyBoxCloud.shader");
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
                if (hideWhenGetClose)
                {
                    targetOpacity = TransparentObject.LuminescenTransaction(targetOpacity, opacity, -opacity, ref opacityVel);
                }
                else
                {
                    targetOpacity = opacity;
                }
            }
            if (initObject() && diffusePower >= -0.001 && targetOpacity > 0.001)
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
            if (cloud != null && cloud.refraction <= 0.001) return;
            TransparentObject_Ring ring = target as TransparentObject_Ring;
            if (ring != null && ring.refraction <= 0.001) return;
            if (initObject() && targetOpacity > 0.001)
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
            if (initObject() && targetOpacity > 0.001)
            {
                bool signalTranslated = (bool)signal;
                if(diffusePower < -0.001)
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
                if (luminescen > 0.001)
                {
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
        }


        public override void BlendTrans(CommandBuffer commandBuffer, TransparentObject target, object targetSignal, Camera camera, object signal, RenderTargetIdentifier[] colors, RenderTargetIdentifier depth)
        {
            if (target != null && target.IsVolum) return;
            TransparentObject_Cloud cloud = target as TransparentObject_Cloud;
            if (cloud != null && cloud.refraction <= 0.001 && cloud.luminescen <= 0.001) return;
            TransparentObject_Ring ring = target as TransparentObject_Ring;
            if (ring != null && ring.refraction <= 0.001 && cloud.luminescen <= 0.001) return;
            if (initObject() && renderingShadow && targetOpacity > 0.001)
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

#if !UNITY
        public override float SettingGUI(float posY, float width, Vector2 outFromTo)
        {
            HelperMethod_GUI.GUIBoolean(ref posY, ref renderingShadow, nameof(renderingShadow).Translate(),width,outFromTo);
            HelperMethod_GUI.GUIBoolean(ref posY, ref hideWhenGetClose, nameof(hideWhenGetClose).Translate(),width,outFromTo);
            HelperMethod_GUI.GUIFloat(ref posY, ref refraction, nameof(refraction).Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY, ref luminescen, nameof(luminescen).Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY, ref opacity, nameof(opacity).Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY, ref opacityType, nameof(opacityType).Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY, ref radius, nameof(radius).Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY, ref diffusePower, nameof(diffusePower).Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec3(ref posY, ref normal, nameof(normal).Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec3(ref posY, ref tangent, nameof(tangent).Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec3(ref posY, ref postion, nameof(postion).Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIString(ref posY, ref cloudTexturePath, nameof(cloudTexturePath).Translate(),width,outFromTo);
            return posY;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref renderingShadow,nameof(renderingShadow),true,true);
            Scribe_Values.Look(ref hideWhenGetClose,nameof(hideWhenGetClose),true,true);
            Scribe_Values.Look(ref cloudTexturePath,nameof(cloudTexturePath),"EarthCloudTex/8k_earth_clouds",true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref refraction,nameof(refraction),6,2,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref luminescen,nameof(luminescen),6,0,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref opacity,nameof(opacity),6,1,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref opacityType,nameof(opacityType),6,0,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref radius,nameof(radius),6,63.76393f * PlanetAtmosphereRenderer.scale,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref diffusePower,nameof(diffusePower),6,16,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec3(ref normal,nameof(normal),6,Vector3.up,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec3(ref tangent,nameof(tangent),6,Vector3.right,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec3(ref postion,nameof(postion),6,Vector3.zero,true);

        }
#endif
        ~TransparentObject_Cloud()
        {
            if(materialSkyBoxCloud) GameObject.Destroy(materialSkyBoxCloud);
        }
    }
}