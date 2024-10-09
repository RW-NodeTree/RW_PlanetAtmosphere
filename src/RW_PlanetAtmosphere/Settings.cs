using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;
using System;

namespace RW_PlanetAtmosphere
{
    public class AtmosphereSettings : ModSettings
    {
        public static bool                      needUpdate          = true;
        public static float                     gamma               = 1;
        public static float                     refraction          = 1.75f;
        public static float                     luminescen          = 0.25f;
        public static float                     sunRadius           = 6960 * scale;
        public static float                     sunDistance         = 1495978.92f * scale;
        public static float                     planetRadius        = 100;
        public static float                     renderingSizeFactor = 1;
        public static Vector4                   sunColor            = new Vector4(0.8f,0.72f,0.65f,0);
        public static TonemapType               tonemapType         = TonemapType.SEUSTonemap;
        public static string                    sunFlareTexturePath = "Effect/sunFlare";
        public static List<TransparentObject>   objects             = new List<TransparentObject>() {new TransparentObject_Atmosphere(), new TransparentObject_Cloud()};


        private static bool dropDownOpened = false;
        private static Vector2 scrollPos = Vector2.zero;
        private static float sizeY = 0;


        internal const float scale = 100f/63.71393f;

        public override void ExposeData()
        {
            base.ExposeData();
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref gamma, "gamma", 6, 1, true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref refraction, "refraction", 6, 1.75f, true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref luminescen, "luminescen", 6, 0.25f, true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref sunRadius, "sunRadius", 6, 6960 * scale, true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref sunDistance, "sunDistance", 6, 1495978.92f * scale, true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref planetRadius, "planetRadius", 6, 100, true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref renderingSizeFactor, "renderingSizeFactor", 6, 1, true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec4(ref sunColor, "sunColor", 6, new Vector4(0.8f,0.72f,0.65f, 0), true);
            Scribe_Values.Look(ref tonemapType, "tonemapType", TonemapType.SEUSTonemap, true);
            if(objects == null)
            {
                objects = new List<TransparentObject>() {new TransparentObject_Atmosphere(), new TransparentObject_Cloud()};
            }
            Scribe_Collections.Look(ref objects, "objects", LookMode.Deep);
            if(objects == null)
            {
                objects = new List<TransparentObject>() {new TransparentObject_Atmosphere(), new TransparentObject_Cloud()};
            }
        }

        public static void DoWindowContents(Rect inRect)
        {
            if(objects == null)
            {
                objects = new List<TransparentObject>() {new TransparentObject_Atmosphere(), new TransparentObject_Cloud()};
            }
            objects.RemoveAll(x => x == null);
            Widgets.DrawLineHorizontal(0,31,inRect.width);
            Vector2 ScrollViewSize = new Vector2(inRect.width,sizeY);
            if(ScrollViewSize.y > inRect.height-64) ScrollViewSize.x -= GUI.skin.verticalScrollbar.fixedWidth;
            Widgets.BeginScrollView(new Rect(0,32,inRect.width,inRect.height-64),ref scrollPos,new Rect(Vector2.zero, ScrollViewSize));

            sizeY = 0;
            Vector2 viewingFromTo = new Vector2(scrollPos.y,scrollPos.y+inRect.height-64);
            HelperMethod_GUI.GUIFloat(ref sizeY, ref gamma, "gamma".Translate(), ScrollViewSize.x, viewingFromTo, 6);
            HelperMethod_GUI.GUIFloat(ref sizeY, ref refraction, "refraction".Translate(), ScrollViewSize.x, viewingFromTo, 6);
            HelperMethod_GUI.GUIFloat(ref sizeY, ref luminescen, "luminescen".Translate(), ScrollViewSize.x, viewingFromTo, 6);
            HelperMethod_GUI.GUIFloat(ref sizeY, ref sunRadius, "sunRadius".Translate(), ScrollViewSize.x, viewingFromTo, 6);
            HelperMethod_GUI.GUIFloat(ref sizeY, ref sunDistance, "sunDistance".Translate(), ScrollViewSize.x, viewingFromTo, 6);
            HelperMethod_GUI.GUIFloat(ref sizeY, ref planetRadius, "planetRadius".Translate(), ScrollViewSize.x, viewingFromTo, 6);
            HelperMethod_GUI.GUIFloat(ref sizeY, ref renderingSizeFactor, "renderingSizeFactor".Translate(), ScrollViewSize.x, viewingFromTo, 6);
            HelperMethod_GUI.GUIEnum(ref sizeY, tonemapType, "tonemapType".Translate(), ScrollViewSize.x, viewingFromTo, x=>tonemapType=x);
            HelperMethod_GUI.GUIString(ref sizeY, ref sunFlareTexturePath, "sunFlareTexturePath".Translate(), ScrollViewSize.x, viewingFromTo);


            Text.Font = GameFont.Medium;
            Widgets.DrawBoxSolid(new Rect(0,sizeY,ScrollViewSize.x,48),Widgets.MenuSectionBGFillColor);
            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x,48),"TransparentObject_Atmosphere".Translate());
            dropDownOpened = HelperMethod_GUI.GUIDragDownButton(new Vector2(inRect.width-48,sizeY),dropDownOpened,48);
            Text.Font = GameFont.Small;
            sizeY += 48;

            if(dropDownOpened)
            {
                ScrollViewSize.x -= GUI.skin.verticalScrollbar.fixedWidth;
                Widgets.BeginGroup(new Rect(GUI.skin.verticalScrollbar.fixedWidth,0,ScrollViewSize.x,ScrollViewSize.y));
                foreach(TransparentObject transparentObject in objects)
                {
                    sizeY = transparentObject.SettingGUI(sizeY,ScrollViewSize.x, viewingFromTo);
                }
                Widgets.EndGroup();
            }
            Widgets.EndScrollView();

            if(Widgets.ButtonText(new Rect(0,inRect.height-32,inRect.width*0.5f,32), "apply".Translate()))
            {
                foreach(TransparentObject obj in objects) obj.needUpdate = true;
                needUpdate = true;
            }

            if(Widgets.ButtonText(new Rect(inRect.width*0.5f,inRect.height-32,inRect.width*0.5f,32), "reset".Translate()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach(RendererDef def in DefDatabase<RendererDef>.AllDefs)
                {
                    options.Add(new FloatMenuOption(def.label ?? def.defName,delegate()
                    {
                        gamma = def.gamma;
                        refraction = def.refraction;
                        luminescen = def.luminescen;
                        sunRadius = def.sunRadius;
                        sunDistance = def.sunDistance;
                        planetRadius = def.planetRadius;
                        renderingSizeFactor = def.renderingSizeFactor;
                        sunColor = def.sunColor;
                        tonemapType = def.tonemapType;
                        sunFlareTexturePath = def.sunFlareTexturePath;
                        objects.Clear();
                        objects.Capacity = def.objects.Count;
                        foreach(ObjectDef objectDef in def.objects) objects.Add(objectDef.TransparentObject);
                        objects.RemoveAll(x => x == null);
                        needUpdate = true;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }

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
            AtmosphereSettings.DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Atmosphere".Translate();
        }
    }
}