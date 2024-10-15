using UnityEngine;
using System;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;
using Verse;

namespace RW_PlanetAtmosphere
{
    // [CreateAssetMenu(fileName = "CloudData", menuName = "Scriptable Object/CloudData", order = 1)]
    public class TransparentObject_Cloud : TransparentObject
    {
        public bool renderingShadow     = true;
        public float refraction         = 2;
        public float luminescen         = 0;
        public float opacity            = 1;
        //public float playRange          = 0.015625f;
        //public float flowDir            = 0;
        public float radius             = 63.76393f * AtmosphereSettings.scale;
        public float diffusePower       = 16;
        //public float sunRadius          = 6960;
        //public float sunDistance        = 1495978.92f;
        public Vector3 normal           = Vector3.up;
        public Vector3 tangent          = Vector3.right;
        public Vector3 postion          = Vector3.zero;
        public string cloudTexturePath  = "EarthCloudTex/8k_earth_clouds";
        //public string noiseTexturePath  = null;

        #region propsIDs

        public static readonly int propId_refraction    = Shader.PropertyToID("refraction");
        public static readonly int propId_luminescen    = Shader.PropertyToID("luminescen");
        public static readonly int propId_opacity       = Shader.PropertyToID("opacity");
        //public static readonly int propId_playRange     = Shader.PropertyToID("playRange");
        //public static readonly int propId_flowDir       = Shader.PropertyToID("flowDir");
        public static readonly int propId_radius        = Shader.PropertyToID("radius");
        public static readonly int propId_diffusePower  = Shader.PropertyToID("diffusePower");
        public static readonly int propId_normal        = Shader.PropertyToID("normal");
        public static readonly int propId_tangent       = Shader.PropertyToID("tangent");
        public static readonly int propId_cloudTexture  = Shader.PropertyToID("cloudTexture");
        //public static readonly int propId_sunRadius     = Shader.PropertyToID("sunRadius");
        //public static readonly int propId_sunDistance   = Shader.PropertyToID("sunDistance");
        //public static int propId_noiseTexture           = Shader.PropertyToID("noiseTexture");

        #endregion

        private Texture2D cloudTexture;
        //private Texture2D noiseTexture;
        private Material materialSkyBoxCloud;

        private static Shader SkyBoxCloud;

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

        public void UpdateMaterial(Material material)
        {
            if (material == null) return;

            material.SetFloat(propId_refraction, refraction);
            material.SetFloat(propId_luminescen, luminescen);
            material.SetFloat(propId_opacity, opacity);
            // material.SetFloat(propId_playRange, playRange);
            // material.SetFloat(propId_flowDir, flowDir);
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
                if (!materialSkyBoxCloud)
                    materialSkyBoxCloud = new Material(SkyBoxCloud);
                if(needUpdate)
                {
                    needUpdate = false;
                    if (cloudTexturePath != null && cloudTexturePath.Length > 0)
                        cloudTexture = GetTexture2D(cloudTexturePath);
                    // if (noiseTexturePath != null && noiseTexturePath.Length > 0)
                    //     noiseTexture = GetTexture2D(noiseTexturePath);
                }
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