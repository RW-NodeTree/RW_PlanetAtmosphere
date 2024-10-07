using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;
using System;

namespace RW_PlanetAtmosphere
{
    internal class AtmosphereSettings : ModSettings
    {
        public static Vector4       sunColor                    = new Vector4(0.8f,0.72f,0.65f,0);


        private static Vector2 scrollPos = Vector2.zero;
        private static Vector2 scrollPosDev = Vector2.zero;
        private static float sizeY = 0;


        internal const float scale = 100f/63.71393f;

        public override void ExposeData()
        {
            base.ExposeData();
            void SaveAndLoadValueVec4(ref Vector4 value, string label, Vector4 defaultValue = default(Vector4), bool forceSave = false)
            {
                value.x = Math.Abs(value.x);
                value.y = Math.Abs(value.y);
                value.z = Math.Abs(value.z);
                value.w = Math.Abs(value.w);
                value *= 1000;
                Scribe_Values.Look(ref value, label, defaultValue, forceSave);
                value /= 1000;
                value.x = Math.Abs(value.x);
                value.y = Math.Abs(value.y);
                value.z = Math.Abs(value.z);
                value.w = Math.Abs(value.w);
            }
            SaveAndLoadValueVec4(ref sunColor, "sunColor", defaultValue: new Vector4(1, 1, 1, 0), forceSave: true);
        }

        public static void DoWindowContents(Rect inRect)
        {
            Widgets.DrawLineHorizontal(0,31,inRect.width);
            Vector2 ScrollViewSize = new Vector2(inRect.width,sizeY);
            if(ScrollViewSize.y > inRect.height-64) ScrollViewSize.x -= 36;
            Widgets.BeginScrollView(new Rect(0,32,inRect.width,inRect.height-64),ref scrollPos,new Rect(Vector2.zero, ScrollViewSize));

            float newValue;

            sizeY = 0;



            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"sunColor".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f/4f,32),sunColor.x.ToString("f5")),out newValue);
            sunColor.x = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),sunColor.y.ToString("f5")),out newValue);
            sunColor.y = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*6f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),sunColor.z.ToString("f5")),out newValue);
            sunColor.z = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*7f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),sunColor.w.ToString("f5")),out newValue);
            sunColor.w = newValue;
            sizeY+=32;
            Widgets.EndScrollView();

            if(Widgets.ButtonText(new Rect(0,inRect.height-32,inRect.width*0.5f,32), "apply".Translate()))
            {
                
            }

            if(Widgets.ButtonText(new Rect(inRect.width*0.5f,inRect.height-32,inRect.width*0.5f,32), "reset".Translate()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach(PlenetRendererDef def in DefDatabase<PlenetRendererDef>.AllDefs)
                {
                    options.Add(new FloatMenuOption(def.label ?? def.defName,delegate()
                    {
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }

        }
    }

    public class AtmosphereMod : Mod
    {
        private static AtmosphereSettings settings;
        public AtmosphereMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<AtmosphereSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            AtmosphereSettings.DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Atmosphere".Translate();
        }
    }
}