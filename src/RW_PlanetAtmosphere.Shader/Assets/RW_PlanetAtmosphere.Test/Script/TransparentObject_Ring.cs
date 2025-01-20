using UnityEngine;
using System;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;

namespace RW_PlanetAtmosphere
{
    public class TransparentObject_Ring : TransparentObject
    {
        public bool renderingShadow = true;
        public float refraction     = 1;
        public float luminescen     = 0;
        public Vector2 ringFromTo   = new Vector2(100, 150);
        public Vector3 normal       = Vector3.up;
        public Vector3 postion      = Vector3.zero;
        public string ringMapPath   = null;

        private Texture2D ringMap;
        private Material materialBasicRing;

        private static Shader BasicRing;

        #region propsIDs

        private static readonly int propId_refraction   = Shader.PropertyToID("refraction");
        private static readonly int propId_luminescen   = Shader.PropertyToID("luminescen");
        private static readonly int propId_ringFromTo   = Shader.PropertyToID("ringFromTo");
        private static readonly int propId_normal       = Shader.PropertyToID("normal");
        private static readonly int propId_ringMap      = Shader.PropertyToID("ringMap");

        #endregion

        public TransparentObject_Ring() { }

        public TransparentObject_Ring(RingDef ringDef)
        {
            if(ringDef != null)
            {
                renderingShadow = ringDef.renderingShadow;
                refraction      = ringDef.refraction;
                luminescen      = ringDef.luminescen ;   
                ringFromTo      = ringDef.ringFromTo ;   
                normal          = ringDef.normal;   
                postion         = ringDef.postion;
                ringMapPath     = ringDef.ringMapPath;
            }
        }

        public override bool IsVolum => false;

        public override int Order => 0;

        void UpdateMaterial(Material material)
        {
            if (material == null) return;

            material.SetFloat(propId_refraction, refraction);
            material.SetFloat(propId_luminescen, luminescen);

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
            if (initObject())
            {
                commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialBasicRing, 0, 2);
            }
        }

        public override void BlendShadow(CommandBuffer commandBuffer, TransparentObject target, object targetSignal, Camera camera, object signal, RenderTargetIdentifier[] colors, RenderTargetIdentifier depth)
        {
            if (initObject())
            {
                if (!renderingShadow || target == this) return;
                TransparentObject_Cloud cloud = target as TransparentObject_Cloud;
                if (cloud != null && cloud.refraction <= 0) return;
                TransparentObject_Ring ring = target as TransparentObject_Ring;
                if (ring != null && ring.refraction <= 0) return;
                commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialBasicRing, 0, 0);
            }
        }


        public override void BlendLumen(CommandBuffer commandBuffer, TransparentObject target, object targetSignal, Camera camera, object signal, RenderTargetIdentifier[] colors, RenderTargetIdentifier depth)
        {
            if (initObject())
            {
                if (luminescen <= 0) return;
                commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialBasicRing, 0, 3);
            }
        }


        public override void BlendTrans(CommandBuffer commandBuffer, TransparentObject target, object targetSignal, Camera camera, object signal, RenderTargetIdentifier[] colors, RenderTargetIdentifier depth)
        {
            if (initObject())
            {
                if (target != null && target.IsVolum) return;
                TransparentObject_Cloud cloud = target as TransparentObject_Cloud;
                if (cloud != null && cloud.refraction <= 0 && cloud.luminescen <= 0) return;
                TransparentObject_Ring ring = target as TransparentObject_Ring;
                if (ring != null && ring.refraction <= 0 && cloud.luminescen <= 0) return;
                commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialBasicRing, 0, 1);
            }
        }

        ~TransparentObject_Ring()
        {
            if(materialBasicRing) GameObject.Destroy(materialBasicRing);
        }
    }
}