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
    public class TransparentObject_Ring : TransparentObject
    {
        float opacityVel = 0;
        internal float targetOpacity    = 0;
        public bool renderingShadow     = true;
        public bool hideWhenGetClose    = true;
        public float opacity            = 1;
        public float opacityType        = 0;
        public float refraction         = 1;
        public float luminescen         = 0;
        public Vector2 ringFromTo       = new Vector2(100, 150);
        public Vector3 normal           = Vector3.up;
        public Vector3 postion          = Vector3.zero;
        public string ringMapPath       = null;

        private Texture2D ringMap;
        private Material materialBasicRing;

        private static Shader BasicRing;

        #region propsIDs

        private static readonly int propId_refraction   = Shader.PropertyToID(nameof(refraction));
        private static readonly int propId_luminescen   = Shader.PropertyToID(nameof(luminescen));
        private static readonly int propId_opacity      = Shader.PropertyToID(nameof(opacity));
        private static readonly int propId_opacityType  = Shader.PropertyToID(nameof(opacityType));
        private static readonly int propId_ringFromTo   = Shader.PropertyToID(nameof(ringFromTo));
        private static readonly int propId_normal       = Shader.PropertyToID(nameof(normal));
        private static readonly int propId_ringMap      = Shader.PropertyToID(nameof(ringMap));

        #endregion

        public TransparentObject_Ring() { }

        public TransparentObject_Ring(RingDef ringDef)
        {
            if(ringDef != null)
            {
                renderingShadow     = ringDef.renderingShadow;
                hideWhenGetClose    = ringDef.hideWhenGetClose;
                refraction          = ringDef.refraction;
                luminescen          = ringDef.luminescen;
                opacity             = ringDef.opacity;
                opacityType         = ringDef.opacityType;
                ringFromTo          = ringDef.ringFromTo;
                normal              = ringDef.normal;   
                postion             = ringDef.postion;
                ringMapPath         = ringDef.ringMapPath;
            }
        }

        public override bool IsVolum => false;

        public override int Order => 0;

        void UpdateMaterial(Material material)
        {
            if (!material) return;

            material.SetFloat(propId_refraction, refraction);
            material.SetFloat(propId_luminescen, luminescen);

            material.SetVector(propId_ringFromTo, ringFromTo);
            material.SetVector(propId_normal, normal);
            material.SetFloat(propId_opacity, targetOpacity);
            material.SetFloat(propId_opacityType, opacityType);

            if (ringMap) material.SetTexture(propId_ringMap, ringMap);
        }

        private static bool init()
        {
            if (!BasicRing)
                BasicRing = GetShader(@"Assets/RW_PlanetAtmosphere/Resources/Shader/Ring/BasicRing.shader");
            return BasicRing;
        }

        private bool initObject()
        {
            if(init())
            {
                if (ringMapPath != null && ringMapPath.Length > 0)
                    ringMap = GetTexture2D(ringMapPath);
                if (!materialBasicRing)
                    materialBasicRing = new Material(BasicRing);
                if(materialBasicRing)
                {
                    UpdateMaterial(materialBasicRing);
                    return true;
                }
            }
            return false;
        }
        
        public override void GenBaseColor(CommandBuffer commandBuffer, TransparentObject target, object targetSignal, Camera camera, object signal, RenderTargetIdentifier[] colors, RenderTargetIdentifier depth)
        {
            if (hideWhenGetClose)
            {
                targetOpacity = TransparentObject.LuminescenTransaction(targetOpacity, opacity, -opacity, ref opacityVel);
            }
            else
            {
                targetOpacity = opacity;
            }
            if (initObject() && targetOpacity > 0.001)
            {
                commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialBasicRing, 0, 2);
            }
        }

        public override void BlendShadow(CommandBuffer commandBuffer, TransparentObject target, object targetSignal, Camera camera, object signal, RenderTargetIdentifier[] colors, RenderTargetIdentifier depth)
        {
            if (!renderingShadow || target == this) return;
            TransparentObject_Cloud cloud = target as TransparentObject_Cloud;
            if (cloud != null && cloud.refraction <= 0.001) return;
            TransparentObject_Ring ring = target as TransparentObject_Ring;
            if (ring != null && ring.refraction <= 0.001) return;
            if (initObject() && targetOpacity > 0.001)
            {
                commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialBasicRing, 0, 0);
            }
        }


        public override void BlendLumen(CommandBuffer commandBuffer, TransparentObject target, object targetSignal, Camera camera, object signal, RenderTargetIdentifier[] colors, RenderTargetIdentifier depth)
        {
            if (luminescen <= 0.001) return;
            if (initObject() && targetOpacity > 0.001)
            {
                commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialBasicRing, 0, 3);
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
                commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialBasicRing, 0, 1);
            }
        }
#if !UNITY
        public override float SettingGUI(float posY, float width, Vector2 outFromTo)
        {
            HelperMethod_GUI.GUIBoolean(ref posY, ref renderingShadow, nameof(renderingShadow).Translate(),width,outFromTo);
            HelperMethod_GUI.GUIBoolean(ref posY, ref hideWhenGetClose, nameof(hideWhenGetClose).Translate(),width,outFromTo);
            HelperMethod_GUI.GUIFloat(ref posY, ref refraction, nameof(refraction).Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY, ref luminescen, nameof(luminescen).Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY, ref opacity, nameof(opacity).Translate(), width, outFromTo, 6);
            HelperMethod_GUI.GUIFloat(ref posY, ref opacityType, nameof(opacityType).Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec2(ref posY, ref ringFromTo, nameof(ringFromTo).Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec3(ref posY, ref normal, nameof(normal).Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec3(ref posY, ref postion, nameof(postion).Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIString(ref posY, ref ringMapPath, nameof(ringMapPath).Translate(),width,outFromTo);
            return posY;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref renderingShadow, nameof(renderingShadow), true,true);
            Scribe_Values.Look(ref hideWhenGetClose, nameof(hideWhenGetClose), true,true);
            Scribe_Values.Look(ref ringMapPath, nameof(ringMapPath), "Ring/2k_saturn_ring_alpha",true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref refraction, nameof(refraction),6,1,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref luminescen, nameof(luminescen),6,0,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref opacity, nameof(opacity), 6, 1, true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref opacityType, nameof(opacityType),6,0,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec2(ref ringFromTo, nameof(ringFromTo),6,new Vector2(100, 150) * PlanetAtmosphereRenderer.scale,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec3(ref normal, nameof(normal),6,Vector3.up,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec3(ref postion, nameof(postion),6,Vector3.zero,true);
        }
#endif
        ~TransparentObject_Ring()
        {
            if(materialBasicRing) GameObject.Destroy(materialBasicRing);
        }
    }
}