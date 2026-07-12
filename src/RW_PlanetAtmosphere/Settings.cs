
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;
using System;
#if !UNITY
using Verse;
using RimWorld;
using RimWorld.Planet;
#endif


namespace RW_PlanetAtmosphere
{


#if UNITY
    public class AtmosphereSettings : MonoBehaviour
#else
    public class AtmosphereSettings : ModSettings
#endif
    {

        public static AtmosphereSettings Current
        {
            get
            {
#if UNITY
                if (!current)
#else
                if (current == null)
#endif
                {
#if UNITY
                    GameObject gameObject = new GameObject(nameof(AtmosphereSettings));
                    gameObject.AddComponent<AtmosphereSettings>();
#else
                    LoadedModManager.GetMod<AtmosphereMod>();
#endif
                }
                return current;
            }
        }

        public bool                     needUpdate              = true;
        public float                    gamma                   = 1;
        public float                    refraction              = 1.75f;
        public float                    luminescen              = 0.25f;
        public float                    sunRadius               = 6960 * scale;
        public float                    sunDistance             = 1495978.92f * scale;
        public float                    planetRadius            = 63.71393f * scale;
        public float                    renderingSizeFactor     = 1;
        public float                    closeRenderingDistance  = 0.5f;
        public Vector4                  sunColor                = new Vector4(0.8f,0.72f,0.65f,0);
        public TonemapType              tonemapType             = TonemapType.SEUSTonemap;
        public string                   sunFlareTexturePath     = "Effect/sunFlare";
        public List<TransparentObject>  objects                 = new List<TransparentObject>() {new TransparentObject_Atmosphere(), new TransparentObject_Cloud()};


#if UNITY
        private Camera _camera;
        public Camera TargetCamera
        {
            get
            {
                if (!_camera && !gameObject.TryGetComponent<Camera>(out _camera))
                {
                    _camera = Camera.main;
                }
        
                return _camera;
            }
        }

        public Camera EditorCamera;
        // public Camera TargetCamera;
        internal const float scale = 1f;

#else
        internal const float scale = 100f/63.71393f;

#endif

        public AtmosphereSettings()
        {
            current = this;
        }
        private static AtmosphereSettings current = null;

#if UNITY

        private int debugViewIndex = -1;
        private void OnGUI()
        {
            if (GUI.Button(new Rect(0,0,32,32),"-"))
            {
                if(debugViewIndex >= 0) debugViewIndex--;
            }
            GUI.Label(new Rect(32, 0, 64, 32), debugViewIndex.ToString());
            if (GUI.Button(new Rect(96,0,32,32),"+"))
            {
                if(debugViewIndex < objects.Count - 1) debugViewIndex++;
            }
            if (debugViewIndex >= 0 && debugViewIndex < objects.Count)
            {
                objects[debugViewIndex].DebugGUI(new Rect(0,32,1024,768));
            }
        }
#else
        private bool dropDownOpened = false;
        private float sizeY = 0;
        private Vector2 scrollPos = Vector2.zero;
        private List<bool> subMenuDropDownOpened = new List<bool>();
        
        private class DebugViwer : Window
        {
            public DebugViwer(TransparentObject transparentObject)
            {
                this.transparentObject = transparentObject;
                base.doCloseX = true;
                base.draggable = true;
                base.doWindowBackground = true;
            }
            private TransparentObject transparentObject;
            public override void DoWindowContents(Rect inRect)
            {
                inRect.width *= Prefs.UIScale;
                inRect.height *= Prefs.UIScale;
                transparentObject.DebugGUI(inRect);
            }
        }
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
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref closeRenderingDistance, "closeRenderingDistance", 6, 0.5f, true);
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

        public void DoWindowContents(Rect inRect)
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
            HelperMethod_GUI.GUIFloat(ref sizeY, ref closeRenderingDistance, "closeRenderingDistance".Translate(), ScrollViewSize.x, viewingFromTo, 6);
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
                    if(dropDownOpened)
                    {
                        sizeY = transparentObject.SettingGUI(sizeY, ScrollViewSize.x, viewingFromTo);
                        if (Prefs.DevMode)
                        {
                            if (Widgets.ButtonText(new Rect(0, sizeY, ScrollViewSize.x, 32), "Show Dev View"))
                            {
                                DebugViwer debugViwer = new DebugViwer(transparentObject);
                                Find.WindowStack.Add(debugViwer);
                            }
                            sizeY += 32;
                        }
                    }
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
            AtmosphereSettings.Current.DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Atmosphere".Translate();
        }
#endif
    }
}