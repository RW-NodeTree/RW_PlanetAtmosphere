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
        public bool updated                     = false;
        public float exposure                   = 4;
        public float ground_refract             = 1;
        public float ground_light               = 0.01f;
        public float mie_amount                 = 3.996f/AtmosphereSettings.scale;
        public float mie_absorb                 = 1.11f;
        public float deltaAHLW                  = 7.5f;
        public float lengthAHLW                 = 1.0f;
        public float H_Reayleigh                = 0.08f*AtmosphereSettings.scale;
        public float H_Mie                      = 0.02f*AtmosphereSettings.scale;
        public float H_OZone                    = 0.25f*AtmosphereSettings.scale;
        public float D_OZone                    = 0.15f*AtmosphereSettings.scale;
        public Vector2 translucentLUTSize       = new Vector2(16, 16);
        public Vector3 SunColor                 = new Vector3(1,1,1);
        public Vector3 mie_eccentricity         = new Vector3(0.618f,0.618f,0.618f);
        public Vector3 reayleighScatterFactor   = new Vector3(0.46278f,1.25945f,3.10319f)/AtmosphereSettings.scale;
        public Vector3 OZoneAbsorbFactor        = new Vector3(0.21195f,0.20962f,0.01686f)/AtmosphereSettings.scale;
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
