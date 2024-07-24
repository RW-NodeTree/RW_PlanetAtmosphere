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
        public float exposure                   = 15;
        public float ground_refract             = 1;
        public float ground_light               = 0.01f;
        public float deltaAHLW_L                = 8.0f;
        public float lengthAHLW_L               = 1.0f;
        public float deltaAHLW_W                = 4.0f;
        public float lengthAHLW_W               = 1.0f;
        public float H_Reayleigh                = 0.08f*AtmosphereSettings.scale;
        public float H_Mie                      = 0.02f*AtmosphereSettings.scale;
        public float H_OZone                    = 0.25f*AtmosphereSettings.scale;
        public float D_OZone                    = 0.15f*AtmosphereSettings.scale;
        public Vector2 translucentLUTSize       = new Vector2(16, 16);
        public Vector4 mie_scatter              = Vector4.one * 3.996f / AtmosphereSettings.scale;
        public Vector4 mie_absorb               = Vector4.one * 4.44f / AtmosphereSettings.scale;
        public Vector4 mie_eccentricity         = new Vector4(0.618f,0.618f,0.618f,0.618f);
        public Vector4 reayleighScatterFactor   = new Vector4(0.46278f,1.25945f,3.10319f,11.69904f)/AtmosphereSettings.scale;
        public Vector4 OZoneAbsorbFactor        = new Vector4(0.0f,0.0f,0.0f,6.4f)/AtmosphereSettings.scale;
        public Vector4 SunColor                 = new Vector4(0.8f,0.72f,0.65f,0);
        public Vector4 scatterLUTSize           = new Vector4( 8, 2, 2, 1);
        public List<string> cloudTexPath        = new List<string>(){"EarthCloudTex/8k_earth_clouds"};
        public List<Vector4> cloudTexValue      = new List<Vector4>(){new Vector4(1.0f,0.01f,0.5f,0.05f)};

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            translucentLUTSize.x = (int)Math.Abs(translucentLUTSize.x);
            translucentLUTSize.y = (int)Math.Abs(translucentLUTSize.y);
            scatterLUTSize.x = (int)Math.Abs(scatterLUTSize.x);
            scatterLUTSize.y = (int)Math.Abs(scatterLUTSize.y);
            scatterLUTSize.z = (int)Math.Abs(scatterLUTSize.z);
            scatterLUTSize.w = (int)Math.Abs(scatterLUTSize.w);
        }
    }
}
