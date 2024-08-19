using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace RW_PlanetAtmosphere
{
    public class AtmosphereDef : Def
    {
        public float            exposure                = 16;
        public float            ground_refract          = 0.1f;
        public float            ground_light            = 0.025f;
        public float            deltaAHLW_L             = 8.0f;
        public float            deltaAHLW_W             = 4.0f;
        public float            lengthAHLW_L            = 1.0f;
        public float            lengthAHLW_W            = 1.0f;
        public float            H_Reayleigh             = 0.08f*AtmosphereSettings.scale;
        public float            H_Mie                   = 0.02f*AtmosphereSettings.scale;
        public float            H_OZone                 = 0.25f*AtmosphereSettings.scale;
        public float            D_OZone                 = 0.15f*AtmosphereSettings.scale;
        public Vector2          translucentLUT_Size     = new Vector2(16, 16);
        public Vector4          mie_scatter             = Vector4.one * 3.996f / AtmosphereSettings.scale;
        public Vector4          mie_absorb              = Vector4.one * 4.44f / AtmosphereSettings.scale;
        public Vector4          mie_eccentricity        = new Vector4(0.618f,0.618f,0.618f,0.618f);
        public Vector4          reayleigh_scatter       = new Vector4(0.46278f,1.25945f,3.10319f,11.69904f)/AtmosphereSettings.scale;
        public Vector4          OZone_absorb            = new Vector4(0.0f,0.0f,0.0f,6.4f)/AtmosphereSettings.scale;
        public Vector4          SunColor                = new Vector4(0.8f,0.72f,0.65f,0);
        public Vector4          scatterLUT_Size         = new Vector4( 8, 2, 1, 2);
        public List<string>     cloudTexPath            = new List<string>(){"EarthCloudTex/8k_earth_clouds"};
        public List<Vector4>    cloudTexValue           = new List<Vector4>(){new Vector4(1.0f,0.0f,0.5f,0.05f)};
        public List<string>     noiseTexPath            = new List<string>(){"EarthCloudTex/noise"};
        public List<Vector2>    noiseTexValue           = new List<Vector2>(){new Vector2(0.0f,0.015625f)};

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            cloudTexPath.RemoveAll(x => x.NullOrEmpty());
            for(int i = cloudTexValue.Count; i < cloudTexPath.Count; i++)
            {
                cloudTexValue.Add(new Vector4(1.0f,0.0f,0.5f,0.05f));
            }
            if(cloudTexValue.Count > cloudTexPath.Count) cloudTexValue.RemoveRange(cloudTexPath.Count, cloudTexValue.Count - cloudTexPath.Count);
            for(int i = noiseTexPath.Count; i < noiseTexPath.Count; i++)
            {
                noiseTexPath.Add("EarthCloudTex/noise");
            }
            if(noiseTexPath.Count > cloudTexPath.Count) noiseTexPath.RemoveRange(cloudTexPath.Count, noiseTexPath.Count - cloudTexPath.Count);
            for(int i = noiseTexValue.Count; i < cloudTexPath.Count; i++)
            {
                noiseTexValue.Add(new Vector2(0.0f,0.015625f));
            }
            if(noiseTexValue.Count > cloudTexPath.Count) noiseTexValue.RemoveRange(cloudTexPath.Count, noiseTexValue.Count - cloudTexPath.Count);

            translucentLUT_Size.x = (int)Math.Abs(translucentLUT_Size.x);
            translucentLUT_Size.y = (int)Math.Abs(translucentLUT_Size.y);
            scatterLUT_Size.x = (int)Math.Abs(scatterLUT_Size.x);
            scatterLUT_Size.y = (int)Math.Abs(scatterLUT_Size.y);
            scatterLUT_Size.z = (int)Math.Abs(scatterLUT_Size.z);
            scatterLUT_Size.w = (int)Math.Abs(scatterLUT_Size.w);
        }
    }
}
