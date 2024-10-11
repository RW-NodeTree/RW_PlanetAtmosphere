using UnityEngine;
using System;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;
using Verse;

namespace RW_PlanetAtmosphere
{
    // [CreateAssetMenu(fileName = "RingData", menuName = "Scriptable Object/RingData", order = 2)]
    public class TransparentObject_Ring : TransparentObject
    {
        public bool renderingShadow = true;
        public float refraction     = 1;
        public float luminescen     = 0;
        public Vector2 ringFromTo   = new Vector2(100, 150) * AtmosphereSettings.scale;
        public Vector3 normal       = Vector3.up;
        public Vector3 postion      = Vector3.zero;
        public string ringMapPath   = "Ring/2k_saturn_ring_alpha";


        private Texture2D ringMap;
        private Material materialBasicRing;

        #region propsIDs

        public static readonly int propId_refraction   = Shader.PropertyToID("refraction");
        public static readonly int propId_luminescen   = Shader.PropertyToID("luminescen");
        public static readonly int propId_ringFromTo   = Shader.PropertyToID("ringFromTo");
        public static readonly int propId_sunRadius    = Shader.PropertyToID("sunRadius");
        public static readonly int propId_sunDistance  = Shader.PropertyToID("sunDistance");
        public static readonly int propId_normal       = Shader.PropertyToID("normal");
        public static readonly int propId_ringMap      = Shader.PropertyToID("ringMap");

        #endregion


        private static Shader BasicRing;

        public TransparentObject_Ring() { }

        public TransparentObject_Ring(RingDef ringDef)
        {
            if(ringDef != null)
            {
                renderingShadow = ringDef.renderingShadow;
                refraction      = ringDef.refraction;
                luminescen      = ringDef.luminescen;
                ringFromTo      = ringDef.ringFromTo;
                normal          = ringDef.normal;
                postion         = ringDef.postion;
                ringMapPath     = ringDef.ringMapPath;
            }
        }

        public override bool IsVolum => false;

        public override int Order => 0;


        public void UpdateMaterial(Material material)
        {
            if (material == null) return;

            material.SetFloat(propId_refraction, refraction);
            material.SetFloat(propId_luminescen, luminescen);
            material.SetFloat(propId_sunRadius, AtmosphereSettings.sunRadius);
            material.SetFloat(propId_sunDistance, AtmosphereSettings.sunDistance);

            material.SetVector(propId_ringFromTo, ringFromTo);
            material.SetVector(propId_normal, normal);

            if (ringMap) material.SetTexture(propId_ringMap, ringMap);
        }

        private static bool init()
        {
            if (!BasicRing)
                BasicRing = GetShader(@"Assets/RW_PlanetAtmosphere/Shader/Ring/BasicRing.shader");
            return BasicRing;
        }

        private bool initObject()
        {
            if(init())
            {
                if (!materialBasicRing)
                    materialBasicRing = new Material(BasicRing);
                if(needUpdate)
                {
                    needUpdate = false;
                    if (ringMapPath != null && ringMapPath.Length > 0)
                        ringMap = GetTexture2D(ringMapPath);
                }
                if(materialBasicRing)
                {
                    UpdateMaterial(materialBasicRing);
                    return true;
                }
            }
            return false;
        }

        public override void GenBaseColor(CommandBuffer commandBuffer, Camera camera, object signal)
        {
            if (initObject())
            {
                commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialBasicRing, 0, 2);
            }
        }

        public override void BlendShadow(CommandBuffer commandBuffer, TransparentObject target, Camera camera, object signal)
        {
            if (initObject())
            {
                if (!renderingShadow || target == this) return;
                commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialBasicRing, 0, 0);
            }
        }


        public override void BlendLumen(CommandBuffer commandBuffer, Camera camera, object signal)
        {
            if (initObject())
            {
                if (luminescen <= 0) return;
                commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialBasicRing, 0, 3);
            }
        }


        public override void BlendTrans(CommandBuffer commandBuffer, TransparentObject target, Camera camera, object signal)
        {
            if (initObject())
            {
                if (target != null && target.IsVolum) return;
                commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialBasicRing, 0, 1);
            }
        }

        public override float SettingGUI(float posY, float width, Vector2 outFromTo)
        {
            HelperMethod_GUI.GUIBoolean(ref posY, ref renderingShadow, "renderingShadow".Translate(),width,outFromTo);
            HelperMethod_GUI.GUIFloat(ref posY, ref refraction, "refraction".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIFloat(ref posY, ref luminescen, "luminescen".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec2(ref posY, ref ringFromTo, "ringFromTo".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec3(ref posY, ref normal, "normal".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIVec3(ref posY, ref postion, "postion".Translate(),width,outFromTo,6);
            HelperMethod_GUI.GUIString(ref posY, ref ringMapPath, "ringMapPath".Translate(),width,outFromTo);
            return posY;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref renderingShadow,"renderingShadow",true,true);
            Scribe_Values.Look(ref ringMapPath,"ringMapPath","Ring/2k_saturn_ring_alpha",true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref refraction,"refraction",6,1,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref luminescen,"luminescen",6,0,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec2(ref ringFromTo,"ringFromTo",6,new Vector2(100, 150) * AtmosphereSettings.scale,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec3(ref normal,"normal",6,Vector3.up,true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec3(ref postion,"postion",6,Vector3.zero,true);
        }

        ~TransparentObject_Ring()
        {
            if(materialBasicRing) GameObject.Destroy(materialBasicRing);
        }
    }
}