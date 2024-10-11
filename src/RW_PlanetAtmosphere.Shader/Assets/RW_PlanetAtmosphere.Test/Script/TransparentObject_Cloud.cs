using UnityEngine;
using System;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;

namespace RW_PlanetAtmosphere
{
    public class TransparentObject_Cloud : TransparentObject
    {
        public bool renderingShadow     = true;
        public float refraction         = 2;
        public float luminescen         = 0;
        //public float playRange          = 0.015625f;
        //public float flowDir            = 0;
        public float radius             = 63.76393f;
        public float diffusePower       = 16;
        //public float sunRadius          = 6960;
        //public float sunDistance        = 1495978.92f;
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
        //private static readonly int propId_playRange    = Shader.PropertyToID("playRange");
        //private static readonly int propId_flowDir      = Shader.PropertyToID("flowDir");
        private static readonly int propId_radius       = Shader.PropertyToID("radius");
        private static readonly int propId_diffusePower = Shader.PropertyToID("diffusePower");
        private static readonly int propId_normal       = Shader.PropertyToID("normal");
        private static readonly int propId_tangent      = Shader.PropertyToID("tangent");
        private static readonly int propId_cloudTexture = Shader.PropertyToID("cloudTexture");
        //private static readonly int propId_sunRadius    = Shader.PropertyToID("sunRadius");
        //private static readonly int propId_sunDistance  = Shader.PropertyToID("sunDistance");
        //private static int propId_noiseTexture = Shader.PropertyToID("noiseTexture");

        #endregion

        public TransparentObject_Cloud() { }

        public TransparentObject_Cloud(CloudDef cloudDef)
        {
            if (cloudDef != null)
            {
                renderingShadow     = cloudDef.renderingShadow;
                refraction          = cloudDef.refraction;
                luminescen          = cloudDef.luminescen;
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
            //material.SetFloat(propId_playRange, playRange);
            //material.SetFloat(propId_flowDir, flowDir);
            material.SetFloat(propId_radius, radius);
            material.SetFloat(propId_diffusePower, diffusePower);
            //material.SetFloat(propId_sunRadius, sunRadius);
            //material.SetFloat(propId_sunDistance, sunDistance);

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

        public override void GenBaseColor(CommandBuffer commandBuffer, Camera camera, object signal)
        {
            if (initObject())
            {
                bool signalTranslated = (bool)signal;
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

        public override void BlendShadow(CommandBuffer commandBuffer, TransparentObject target, Camera camera, object signal)
        {
            if (initObject())
            {
                if (!renderingShadow || target == this || target is TransparentObject_Atmosphere) return;
                bool signalTranslated = (bool)signal;
                if (signalTranslated)
                {
                    commandBuffer.DrawMesh(DefaultRenderingMesh, Matrix4x4.Translate(postion), materialSkyBoxCloud, 0, 0);
                }
            }
        }


        public override void BlendLumen(CommandBuffer commandBuffer, Camera camera, object signal)
        {
            if (initObject())
            {
                if (luminescen <= 0) return;
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


        public override void BlendTrans(CommandBuffer commandBuffer, TransparentObject target, Camera camera, object signal)
        {
            if (initObject())
            {
                if(target != null && target.IsVolum) return;
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

        ~TransparentObject_Cloud()
        {
            if(materialSkyBoxCloud) GameObject.Destroy(materialSkyBoxCloud);
        }
    }
}