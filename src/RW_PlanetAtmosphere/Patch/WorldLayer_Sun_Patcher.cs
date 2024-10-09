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
    [HarmonyPatch(typeof(WorldLayer_Sun))]
    internal static class WorldLayer_Sun_Patcher
    {
        private static MethodInfo WorldLayer_ClearSubMeshes = typeof(WorldLayer).GetMethod("ClearSubMeshes",BindingFlags.Instance | BindingFlags.NonPublic);

        private static AccessTools.FieldRef<WorldLayer,bool> WorldLayer_dirty = AccessTools.FieldRefAccess<WorldLayer,bool>("dirty");


        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(WorldLayer_Sun),
            "Regenerate"
            )]
        private static bool PreWorldLayer_Glow_Regenerate(WorldLayer_Glow __instance, ref IEnumerable __result)
        {
            __result = RegenerateOverider(__instance);
            return false;
        }

        private static IEnumerable RegenerateOverider(WorldLayer_Glow instance)
        {
            WorldLayer_dirty(instance) = false;
            WorldLayer_ClearSubMeshes.Invoke(instance,new object[]{MeshParts.All});
            yield break;
        }

    }
}