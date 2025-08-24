using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace RW_PlanetAtmosphere.Patch
{
    [HarmonyPatch(typeof(Root))]
    internal static class Root_Patcher
    {


        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(Root),
            "OnGUI"
            )]
        private static void PreRoot_OnGUI()
        {
#if V13 || V14 || V15
            if(Find.World?.renderer != null && WorldRendererUtility.WorldRenderedNow)
#else
            if(Find.World?.renderer != null && WorldRendererUtility.WorldRendered)
#endif
            {
                WorldCameraManager.WorldCamera.targetTexture = null;
                WorldCameraManager.WorldSkyboxCamera.targetTexture = null;
                if(ShaderLoader.cameraOverride != null)
                {
                    GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height), ShaderLoader.cameraOverride);
                }
            }
        }

    }
}