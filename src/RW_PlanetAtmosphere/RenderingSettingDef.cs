using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace RW_PlanetAtmosphere
{
    public class RenderingSettingDef : Def
    {
        public Vector4          sunColor                = new Vector4(0.8f,0.72f,0.65f,0);
        public List<ObjectDef>  objects                 = new List<ObjectDef>();

    }
}
