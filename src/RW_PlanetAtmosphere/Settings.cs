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
        private static float sizeY = 0;
        private static Vector2 scrollPos = Vector2.zero;
        private static List<bool> subMenuDropDownOpened = new List<bool>();


        internal const float scale = 100f/63.71393f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref sunFlareTexturePath,"sunFlareTexturePath","Effect/sunFlare",true);
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
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            if(ScrollViewSize.y > inRect.height-64) ScrollViewSize.x -= GUI.skin.verticalScrollbar.fixedWidth+1;
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
            HelperMethod_GUI.GUIVec4(ref sizeY, ref sunColor, "sunColor".Translate(), ScrollViewSize.x, viewingFromTo, 6);
            HelperMethod_GUI.GUIEnum(ref sizeY, tonemapType, "tonemapType".Translate(), ScrollViewSize.x, viewingFromTo, x=>tonemapType=x);
            HelperMethod_GUI.GUIString(ref sizeY, ref sunFlareTexturePath, "sunFlareTexturePath".Translate(), ScrollViewSize.x, viewingFromTo);


            Text.Font = GameFont.Medium;
            Widgets.DrawBoxSolid(new Rect(0,sizeY,ScrollViewSize.x,48),Widgets.MenuSectionBGFillColor);
            dropDownOpened = HelperMethod_GUI.GUIDragDownButton(new Vector2(8,sizeY+8),dropDownOpened,32);
            Widgets.Label(new Rect(48,sizeY,ScrollViewSize.x-48,48),"TransparentObjects".Translate());
            Text.Font = GameFont.Small;
            sizeY += 48;

            if(dropDownOpened)
            {
                float lineStart = sizeY;
                ScrollViewSize.x -= 48;
                Widgets.BeginGroup(new Rect(48,0,ScrollViewSize.x,ScrollViewSize.y));

                for (int i = 0; i < objects.Count; i++)
                {
                    if(subMenuDropDownOpened.Count <= i) subMenuDropDownOpened.Add(false);
                    TransparentObject transparentObject = objects[i];
                    bool dropDownOpened = subMenuDropDownOpened[i];
                    Text.Font = GameFont.Medium;

                    Widgets.DrawBoxSolid(new Rect(0,sizeY,ScrollViewSize.x,48),Widgets.MenuSectionBGFillColor);

                    dropDownOpened = HelperMethod_GUI.GUIDragDownButton(new Vector2(8,sizeY+8),dropDownOpened,32);

                    Widgets.Label(new Rect(48,sizeY,ScrollViewSize.x-96,48),transparentObject.GetType().Name.Translate());

                    Rect rect = new Rect(ScrollViewSize.x-40,sizeY+8,32,32);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    HelperMethod_GUI.GUILabelInFontSize(rect,"-");
                    Text.Anchor = TextAnchor.MiddleLeft;
                    if (Widgets.ButtonInvisible(rect))
                    {
                        objects.RemoveAt(i);
                        i--;
                        continue;
                    }
                    else Widgets.DrawHighlightIfMouseover(rect);

                    Text.Font = GameFont.Small;
                    sizeY += 48;
                    if(dropDownOpened) sizeY = transparentObject.SettingGUI(sizeY,ScrollViewSize.x, viewingFromTo);
                    subMenuDropDownOpened[i] = dropDownOpened;
                }
                subMenuDropDownOpened.RemoveRange(objects.Count,subMenuDropDownOpened.Count - objects.Count);
                
                int fontSize = Text.CurFontStyle.fontSize;
                Text.CurFontStyle.fontSize = 32;
                bool newObjectClicked = Widgets.ButtonText(new Rect(0,sizeY,ScrollViewSize.x,48), "new".Translate());
                Text.CurFontStyle.fontSize = fontSize;
                if(newObjectClicked)
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach(Type type in typeof(TransparentObject).AllSubclassesNonAbstract())
                    {
                        options.Add(new FloatMenuOption(type.Name.Translate(),delegate()
                        {
                            objects.Add((TransparentObject)Activator.CreateInstance(type));
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
                sizeY += 48;


                Widgets.EndGroup();
                ScrollViewSize.x += 48;
                Widgets.DrawLineVertical(24,lineStart,sizeY-lineStart);
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
            Text.Anchor = anchor;

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