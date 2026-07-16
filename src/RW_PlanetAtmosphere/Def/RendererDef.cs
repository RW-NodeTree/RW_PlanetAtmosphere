using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if !UNITY
using HarmonyLib;
using Verse;
#endif

namespace RW_PlanetAtmosphere
{
#if UNITY
    public class RendererDef
#else
    public class RendererDef : Def
#endif
    {

        public float            gamma               = 1;
        public float            refraction          = 1.75f;
        public float            luminescen          = 0.25f;
        public float            sunRadius           = 6960 * PlanetAtmosphereRenderer.scale;
        public float            sunDistance         = 1495978.92f * PlanetAtmosphereRenderer.scale;
        public float            planetRadius        = 100;
        public float            renderingSizeFactor = 1;
        public string           sunFlareTexturePath = "Effect/sunFlare";
        public Vector4          sunColor            = new Vector4(0.8f,0.72f,0.65f,0);
        public TonemapType      tonemapType         = TonemapType.SEUSTonemap;
        public List<ObjectDef>  objects             = new List<ObjectDef>() {new AtmosphereDef(), new CloudDef()};

    }
    public enum TonemapType
    {
        SEUSTonemap = 0,
        HableTonemap = 1,
        UchimuraTonemap = 2,
        ACESTonemap = 3,
        ACESTonemap2 = 4,
        LottesTonemap = 5,
        AGXTonemap = 6,
    }
}
