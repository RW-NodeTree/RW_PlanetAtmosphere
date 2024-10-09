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
    [HarmonyPatch(typeof(WorldLayer_Glow))]
    internal static class WorldLayer_Glow_Patcher
    {
        private static MethodInfo WorldLayer_ClearSubMeshes = typeof(WorldLayer).GetMethod("ClearSubMeshes",BindingFlags.Instance | BindingFlags.NonPublic);

        private static AccessTools.FieldRef<WorldLayer,bool> WorldLayer_dirty = AccessTools.FieldRefAccess<WorldLayer,bool>("dirty");


        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(WorldLayer_Glow),
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
            // if(ShaderLoader.isEnable)
            // {
            //     ShaderLoader.mesh.vertices = new Vector3[]
            //     {
            //         new Vector3(-200,-200,-200),
            //         new Vector3( 200,-200,-200),
            //         new Vector3(-200, 200,-200),
            //         new Vector3( 200, 200,-200),
            //         new Vector3(-200,-200, 200),
            //         new Vector3( 200,-200, 200),
            //         new Vector3(-200, 200, 200),
            //         new Vector3( 200, 200, 200)
            //     };
            //     ShaderLoader.mesh.triangles = new int[]
            //     {
            //         0,2,1,3,1,2,
            //         5,7,4,6,4,7,
            //         4,6,0,2,0,6,
            //         1,3,5,7,5,3,
            //         2,6,3,7,3,6,
            //         4,0,5,1,5,0
            //     };
            //     ShaderLoader.mesh.RecalculateBounds();
            //     ShaderLoader.mesh.RecalculateNormals();
            //     ShaderLoader.mesh.RecalculateTangents();
            // }
            yield break;
        }

    }
}