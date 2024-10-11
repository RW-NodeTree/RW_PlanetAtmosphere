using HarmonyLib;
using System;
using System.Reflection;
using Verse;

namespace RW_PlanetAtmosphere.Patch
{
    [StaticConstructorOnStartup]
    internal static class HarmonyInjector
    {
        static HarmonyInjector()
        {
            patcher.PatchAll();
        }

        public static Harmony patcher = new Harmony("RW_PlanetAtmosphere.Patch");
        public static Assembly coreAssembly = typeof(Thing).Assembly;
    }
}
