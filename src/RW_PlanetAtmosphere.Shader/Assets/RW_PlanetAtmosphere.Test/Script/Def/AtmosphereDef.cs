using UnityEngine;
using System;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;

namespace RW_PlanetAtmosphere
{
    [CreateAssetMenu(fileName = "AtmosphereDef", menuName = "Scriptable Object/AtmosphereDef", order = 0)]
    public class AtmosphereDef : RendererDef
    {
        public Vector2Int translucentLUTSize    = new Vector2Int(256, 256);
        public Vector2Int outSunLightLUTSize    = new Vector2Int(256, 256);
        public Vector2Int inSunLightLUTSize     = new Vector2Int(256, 256);
        public Vector4 scatterLUTSize           = new Vector4(128, 32, 8, 32);
        public Vector3 postion                  = Vector3.zero;


        public float exposure                   = 16;
        public float deltaL                     = 8;
        public float deltaW                     = 2;
        public float lengthL                    = 1;
        public float lengthW                    = 1;
        public float minh                       = 63.71393f;
        public float maxh                       = 64.21393f;
        public float H_Reayleigh                = 0.08f;
        public float H_Mie                      = 0.02f;
        public float H_OZone                    = 0.25f;
        public float D_OZone                    = 0.15f;
        public Vector4 reayleigh_scatter        = new Vector4(0.46278f, 1.25945f, 3.10319f, 11.69904f);
        public Vector4 molecule_absorb          = Vector4.zero;
        public Vector4 OZone_absorb             = new Vector4(0.21195f, 0.20962f, 0.01686f, 6.4f);
        public Vector4 mie_scatter              = new Vector4(3.996f, 3.996f, 3.996f, 3.996f);
        public Vector4 mie_absorb               = new Vector4(4.44f, 4.44f, 4.44f, 4.44f);
        public Vector4 mie_eccentricity         = new Vector4(0.618f, 0.618f, 0.618f, 0.618f);

        public override TransparentObject TransparentObject => new TransparentObject_Atmosphere(this);
    }

}