
#if !UNITY
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;
using System;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace RW_PlanetAtmosphere
{


    public class AtmosphereSettings : ModSettings
    {
        public override void ExposeData()
        {
            base.ExposeData();
            PlanetAtmosphereRenderer.CurrentRenderer.ExposeData();
        }
    }

    public class AtmosphereMod : Mod
    {
        public AtmosphereMod(ModContentPack content) : base(content)
        {
            GetSettings<AtmosphereSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            PlanetAtmosphereRenderer.CurrentRenderer.DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Atmosphere".Translate();
        }
    }
}
#endif
