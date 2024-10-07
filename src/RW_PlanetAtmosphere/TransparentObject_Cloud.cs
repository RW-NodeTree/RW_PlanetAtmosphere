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
        public float exposure           = 16f;
        public float refraction         = 1;
        public float luminescen         = 0;
        //public float playRange          = 0.015625f;
        //public float flowDir            = 0;
        public float radius             = 63.76393f;
        //public float sunRadius          = 6960;
        //public float sunDistance        = 1495978.92f;
        public Vector3 normal           = Vector3.up;
        public Vector3 tangent          = Vector3.right;
        public Vector3 postion          = Vector3.zero;
        public string cloudTexturePath  = null;
        //public string noiseTexturePath  = null;

        private float GUIheight = 0;
        private Texture2D cloudTexture;
        //private Texture2D noiseTexture;
        private Material materialSkyBoxCloud;

        private static Shader SkyBoxCloud;

        #region propsIDs

        private static readonly int propId_exposure     = Shader.PropertyToID("exposure");
        private static readonly int propId_refraction   = Shader.PropertyToID("refraction");
        private static readonly int propId_luminescen   = Shader.PropertyToID("luminescen");
        //private static readonly int propId_playRange    = Shader.PropertyToID("playRange");
        //private static readonly int propId_flowDir      = Shader.PropertyToID("flowDir");
        private static readonly int propId_radius       = Shader.PropertyToID("radius");
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
                exposure            = cloudDef.exposure;
                refraction          = cloudDef.refraction;
                luminescen          = cloudDef.luminescen;
                radius              = cloudDef.radius;
                normal              = cloudDef.normal;
                tangent             = cloudDef.tangent;
                postion             = cloudDef.postion;
                cloudTexturePath    = cloudDef.cloudTexturePath;
            }
        }
        public override bool IsVolum => false;

        public override float SettingGUIHeight => GUIheight;

        void UpdateMaterial(Material material)
        {
            if (material == null) return;

            material.SetFloat(propId_exposure, exposure);
            material.SetFloat(propId_refraction, refraction);
            material.SetFloat(propId_luminescen, luminescen);
            // material.SetFloat(propId_playRange, playRange);
            // material.SetFloat(propId_flowDir, flowDir);
            material.SetFloat(propId_radius, radius);
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
                // if (noiseTexturePath != null && noiseTexturePath.Length > 0)
                //     noiseTexture = GetTexture2D(noiseTexturePath);
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

        public override void SettingGUI(Rect inRect, Rect outRect)
        {
            float sizeY = inRect.y;
            Text.Font = GameFont.Medium;
            Widgets.DrawBoxSolid(new Rect(inRect.x,sizeY,inRect.width,48),Widgets.MenuSectionBGFillColor);
            Widgets.Label(new Rect(inRect.x,sizeY,inRect.width,48),"TransparentObject_Cloud".Translate());
            Text.Font = GameFont.Small;
            sizeY += 48;
            void GUIbool(ref bool value, string name)
            {
                if(
                    inRect.xMin     < outRect.xMax &&
                    outRect.xMin    < inRect.xMax  &&
                    sizeY           < outRect.yMax &&
                    outRect.yMin    < sizeY + 32
                )
                {
                    Widgets.Label(new Rect(inRect.x,sizeY,inRect.width*0.5f,32),name);
                    Widgets.Checkbox(inRect.xMax - 32, sizeY, ref value, 32);
                }
                sizeY+=32;
            }
            void GUIfloat(ref float value, string name)
            {
                if(
                    inRect.xMin     < outRect.xMax &&
                    outRect.xMin    < inRect.xMax  &&
                    sizeY           < outRect.yMax &&
                    outRect.yMin    < sizeY + 32
                )
                {
                    Widgets.Label(new Rect(inRect.x,sizeY,inRect.width*0.5f,32),name);
                    float.TryParse(Widgets.TextField(new Rect(inRect.x+inRect.width*0.5f,       sizeY,inRect.width*0.5f,32),value.ToString("f5")),out value);
                }
                sizeY+=32;
            }
            void GUIstring(ref string value, string name)
            {
                if(
                    inRect.xMin     < outRect.xMax &&
                    outRect.xMin    < inRect.xMax  &&
                    sizeY           < outRect.yMax &&
                    outRect.yMin    < sizeY + 32
                )
                {
                    Widgets.Label(new Rect(inRect.x,sizeY,inRect.width*0.5f,32),name);
                    value = Widgets.TextField(new Rect(inRect.x+inRect.width*0.5f,       sizeY,inRect.width*0.5f,32),value);
                }
                sizeY+=32;
            }
            void GUIVec3(ref Vector3 value, string name)
            {
                if(
                    inRect.xMin     < outRect.xMax &&
                    outRect.xMin    < inRect.xMax  &&
                    sizeY           < outRect.yMax &&
                    outRect.yMin    < sizeY + 32
                )
                {
                    float newValue;
                    Widgets.Label(new Rect(inRect.x,sizeY,inRect.width*0.5f,32),name);
                    float.TryParse(Widgets.TextField(new Rect(inRect.x+inRect.width*0.5f,       sizeY,inRect.width*0.5f/3f,32),value.x.ToString("f5")),out newValue);
                    value.x = newValue;
                    float.TryParse(Widgets.TextField(new Rect(inRect.x+inRect.width*0.5f*4f/3f, sizeY,inRect.width*0.5f/3f,32),value.y.ToString("f5")),out newValue);
                    value.y = newValue;
                    float.TryParse(Widgets.TextField(new Rect(inRect.x+inRect.width*0.5f*5f/3f, sizeY,inRect.width*0.5f/3f,32),value.z.ToString("f5")),out newValue);
                    value.z = newValue;
                }
                sizeY+=32;
            }
            GUIbool(ref renderingShadow, "renderingShadow".Translate());
            GUIfloat(ref exposure, "exposure".Translate());
            GUIfloat(ref refraction, "refraction".Translate());
            GUIfloat(ref luminescen, "luminescen".Translate());
            GUIfloat(ref radius, "radius".Translate());
            GUIVec3(ref normal, "normal".Translate());
            GUIVec3(ref tangent, "tangent".Translate());
            GUIVec3(ref postion, "postion".Translate());
            GUIstring(ref cloudTexturePath, "cloudTexturePath".Translate());
            GUIheight = sizeY - inRect.y;
        }

        public override void ExposeData()
        {
            
        }

        ~TransparentObject_Cloud()
        {
            if(materialSkyBoxCloud) GameObject.Destroy(materialSkyBoxCloud);
        }
    }
}