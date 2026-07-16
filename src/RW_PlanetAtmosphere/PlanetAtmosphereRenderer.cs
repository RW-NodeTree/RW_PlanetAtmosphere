using System;
using System.Data;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
#if !UNITY
using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;
#endif

namespace RW_PlanetAtmosphere
{
#if !UNITY
    [StaticConstructorOnStartup]
#endif
    public class PlanetAtmosphereRenderer : MonoBehaviour
    {
        
#if !UNITY
        static PlanetAtmosphereRenderer()
        {
            WorldCameraManager.WorldCamera.depthTextureMode = DepthTextureMode.Depth;
            WorldCameraManager.WorldCamera.gameObject.AddComponent<PlanetAtmosphereRenderer>();
            WorldCameraManager.WorldSkyboxCamera.depthTextureMode = DepthTextureMode.Depth;
            WorldCameraManager.WorldSkyboxCamera.backgroundColor = Color.black;
            

            // WorldMaterials.Rivers.shader = WorldMaterials.WorldTerrain.shader;
            // WorldMaterials.WorldOcean.shader = WorldMaterials.WorldTerrain.shader;
            // WorldMaterials.RiversBorder.shader = WorldMaterials.WorldTerrain.shader;
            // WorldMaterials.UngeneratedPlanetParts.shader = WorldMaterials.WorldTerrain.shader;

            // WorldMaterials.Rivers.renderQueue = 3530;
            // WorldMaterials.WorldOcean.renderQueue = 3500;
            // WorldMaterials.RiversBorder.renderQueue = 3520;
            // WorldMaterials.UngeneratedPlanetParts.renderQueue = 3500;

            // WorldMaterials.Rivers.color = new Color(-65536,-65536,-65536,1);
            // WorldMaterials.RiversBorder.color = new Color(0,0,0,0);


            // WorldMaterials.Rivers.mainTexture = ContentFinder<Texture2D>.Get("TerrainReplace/Water");
            // WorldMaterials.WorldOcean.mainTexture = ContentFinder<Texture2D>.Get("TerrainReplace/Water");
            // WorldMaterials.RiversBorder.mainTexture = ContentFinder<Texture2D>.Get("TerrainReplace/Water");
            // WorldMaterials.UngeneratedPlanetParts.mainTexture = ContentFinder<Texture2D>.Get("TerrainReplace/Water");
        }
#endif

#region propsIDs

        public static readonly int propId_gamma             = Shader.PropertyToID(nameof(gamma));
        public static readonly int propId_radius            = Shader.PropertyToID(nameof(planetRadius));
        public static readonly int propId_sunRadius         = Shader.PropertyToID(nameof(sunRadius));
        public static readonly int propId_sunDistance       = Shader.PropertyToID(nameof(sunDistance));
        public static readonly int propId_sunFlareTexture   = Shader.PropertyToID(nameof(sunFlareTexture));
        public static readonly int propId_backgroundTexture = Shader.PropertyToID("backgroundTexture");

        #endregion

        public bool                     needUpdate              = true;
        public float                    gamma                   = 1;
        public float                    refraction              = 1.75f;
        public float                    luminescen              = 0.25f;
        public float                    sunRadius               = 6960 * scale;
        public float                    sunDistance             = 1495978.92f * scale;
        public float                    planetRadius            = 63.71393f * scale;
        public float                    renderingSizeFactor     = 1;
        public float                    closeRenderingDistance  = 0.5f;
#if !UNITY
        public Vector4                  sunColor                = new Vector4(0.8f,0.72f,0.65f,0);
#endif
        public TonemapType              tonemapType             = TonemapType.SEUSTonemap;
        public string                   sunFlareTexturePath     = "Effect/sunFlare";
        public List<TransparentObject>  objects                 = new List<TransparentObject>() {new TransparentObject_Atmosphere(), new TransparentObject_Cloud()};
        

        private float luminescenVel = 0;
        private float targetLuminescen = 0;
        private float refractionVel = 0;
        private float targetRefraction = 0;

        private Material materialSunFlear;
        private Material materialTonemaps;
        private Material materialWriteDepth;
        private Material materialRemoveAlpha;
        private Texture2D sunFlareTexture;
        private CommandBuffer commandBufferAfterDepth;
        private CommandBuffer commandBufferAfterAlpha;
        internal RenderTexture cameraOverride;
#if UNITY
        private CommandBuffer commandBufferAfterDepth_EditorCamera;
        private CommandBuffer commandBufferAfterAlpha_EditorCamera;
        private Camera _camera;
        public Camera EditorCamera;
        // public Camera TargetCamera;
        internal const float scale = 1f;
#else
        internal const float scale = 100f/63.71393f;
#endif
        private static PlanetAtmosphereRenderer currentRenderer;

        public PlanetAtmosphereRenderer()
        {
            if (currentRenderer)
            {
                GameObject.Destroy(currentRenderer);
            }
            currentRenderer = this;
        }

        public static PlanetAtmosphereRenderer CurrentRenderer
        {
            get
            {
                if (!currentRenderer)
                {
                    GameObject obj = new GameObject(nameof(PlanetAtmosphereRenderer));
                    obj.AddComponent<PlanetAtmosphereRenderer>();
                }
                return currentRenderer;
            }
        }

#if UNITY
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

#endif

        private void checkAndUpdate()
        {
            if(needUpdate)
            {
                needUpdate = false;
                sunFlareTexture = TransparentObject.GetTexture2D(sunFlareTexturePath);
                Shader
                shader = TransparentObject.GetShader(@"Assets/RW_PlanetAtmosphere/Resources/Shader/SunFlare.shader");
                if(shader)
                {
                    materialSunFlear = new Material(shader);
                    materialSunFlear.SetTexture(propId_sunFlareTexture, sunFlareTexture);
                }
                shader = TransparentObject.GetShader(@"Assets/RW_PlanetAtmosphere/Resources/Shader/Tonemaps.shader");
                if(shader)
                {
                    materialTonemaps = new Material(shader);
                    materialTonemaps.SetFloat(propId_gamma, gamma);
                }
                shader = TransparentObject.GetShader(@"Assets/RW_PlanetAtmosphere/Resources/Shader/WriteDepth.shader");
                if(shader)
                {
                    materialWriteDepth = new Material(shader);
                    materialWriteDepth.SetFloat(propId_radius,planetRadius);
                }
                shader = TransparentObject.GetShader(@"Assets/RW_PlanetAtmosphere/Resources/Shader/RemoveAlpha.shader");
                if(shader) materialRemoveAlpha = new Material(shader);
            }
            if(renderingSizeFactor != 1)
            {
                int width = (int)(Screen.width * renderingSizeFactor);
                int height = (int)(Screen.height * renderingSizeFactor);
                if(!cameraOverride || cameraOverride.width != width || cameraOverride.height != height)
                {
                    if(cameraOverride) GameObject.Destroy(cameraOverride);
                    cameraOverride = new RenderTexture(width,height,24,RenderTextureFormat.ARGBFloat)
                    {
                        useMipMap = false
                    };
                    cameraOverride.Create();
                }
            }
            else
            {
                if(cameraOverride) GameObject.Destroy(cameraOverride);
                cameraOverride = null;
            }
#if UNITY
            TargetCamera.targetTexture = cameraOverride;
#else
            WorldCameraManager.WorldCamera.targetTexture = cameraOverride;
            WorldCameraManager.WorldSkyboxCamera.targetTexture = cameraOverride;
#endif
        }
        
        
        void Start()
        {
            commandBufferAfterDepth = new CommandBuffer();
            commandBufferAfterDepth.name = "RW_PlanetAtmosphere.AfterDepth";
            commandBufferAfterAlpha = new CommandBuffer();
            commandBufferAfterAlpha.name = "RW_PlanetAtmosphere.AfterAlpha";
#if UNITY
            TargetCamera.AddCommandBuffer(CameraEvent.AfterDepthTexture,commandBufferAfterDepth);
            TargetCamera.AddCommandBuffer(CameraEvent.AfterForwardAlpha,commandBufferAfterAlpha);
#else
            Find.WorldCamera.AddCommandBuffer(CameraEvent.AfterDepthTexture,commandBufferAfterDepth);
            Find.WorldCamera.AddCommandBuffer(CameraEvent.AfterForwardAlpha,commandBufferAfterAlpha);
#endif
        }

        
        void Update()
        {
#if !UNITY
            if (Find.World != null)
            {
                Shader.SetGlobalVector("_WorldSpaceLightPos0",GenCelestial.CurSunPositionInWorldSpace());
                Shader.SetGlobalVector("_LightColor0",sunColor);
            }
#endif
            commandBufferAfterDepth.Clear();
            commandBufferAfterAlpha.Clear();
#if !UNITY
#if V13 || V14 || V15
            if(Find.World?.renderer == null || !WorldRendererUtility.WorldRenderedNow) return;
#else
            if(Find.World?.renderer == null || !WorldRendererUtility.WorldRendered) return;
#endif
            if(Find.PlaySettings.usePlanetDayNightSystem)
#endif
                
            {
                checkAndUpdate();
                refraction = Math.Abs(refraction);
                luminescen = Math.Abs(luminescen);
                void BeforeShadow(CommandBuffer cb)
                {
                    cb.GetTemporaryRT(propId_backgroundTexture, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
                    cb.Blit(BuiltinRenderTextureType.CameraTarget, propId_backgroundTexture);
                    if (materialRemoveAlpha)
                    {
                        cb.Blit(null, BuiltinRenderTextureType.CameraTarget, materialRemoveAlpha, 0);
                        cb.Blit(BuiltinRenderTextureType.CameraTarget, propId_backgroundTexture);
                        // cb.ReleaseTemporaryRT(propId_backgroundTexture);
                    }
                    targetRefraction = TransparentObject.LuminescenTransaction(targetRefraction, refraction, luminescen, ref refractionVel);
                    if (targetRefraction != 1)
                    {
                        Color color = new Color(targetRefraction, targetRefraction, targetRefraction, 1);
                        cb.SetGlobalTexture(TransparentObject.ColorTex, propId_backgroundTexture);
                        cb.SetGlobalColor(TransparentObject.MainColor, color);
                        cb.Blit(null, BuiltinRenderTextureType.CameraTarget, TransparentObject.AddToTargetMaterial, 2);
                    }
                    if (materialSunFlear)
                    {
                        cb.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                        cb.DrawMesh(TransparentObject.DefaultRenderingMesh, Matrix4x4.identity, materialSunFlear, 0, 0);
                    }
                    if (materialWriteDepth)
                    {
                        cb.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                        cb.DrawMesh(TransparentObject.DefaultRenderingMesh,Matrix4x4.identity,materialWriteDepth, 0, 1);
                    }
                }
                void BackgroundBlendLumen(CommandBuffer cb)
                {
                    
                    targetLuminescen = TransparentObject.LuminescenTransaction(targetLuminescen, luminescen, refraction, ref luminescenVel);
                    if (targetLuminescen != 0)
                    {
                        Color color = new Color(targetLuminescen, targetLuminescen, targetLuminescen, 0);
                        cb.SetGlobalTexture(TransparentObject.ColorTex, propId_backgroundTexture);
                        cb.SetGlobalColor(TransparentObject.MainColor, color);
                        cb.Blit(null, BuiltinRenderTextureType.CameraTarget, TransparentObject.AddToTargetMaterial, 3);
                    }
                    cb.ReleaseTemporaryRT(propId_backgroundTexture);
                }
                void AfterTrans(CommandBuffer cb)
                {
                    if (!materialSunFlear) return;
                    cb.GetTemporaryRT(propId_backgroundTexture, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
                    cb.Blit(BuiltinRenderTextureType.CameraTarget, propId_backgroundTexture);
                }
                if (materialWriteDepth)
                {
                    commandBufferAfterDepth.DrawMesh(TransparentObject.DefaultRenderingMesh, Matrix4x4.identity,materialWriteDepth, 0, 0);
                }
                
                Shader.SetGlobalFloat(propId_sunRadius, sunRadius);
                Shader.SetGlobalFloat(propId_sunDistance, sunDistance);
#if UNITY
                TransparentObject.DrawTransparentObjects(objects, commandBufferAfterAlpha, TargetCamera, BeforeShadow, BackgroundBlendLumen, AfterTrans);
#else
                TransparentObject.DrawTransparentObjects(objects, commandBufferAfterAlpha, WorldCameraManager.WorldCamera, BeforeShadow, BackgroundBlendLumen, AfterTrans);
#endif
                if (materialSunFlear)
                {
                    commandBufferAfterAlpha.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                    commandBufferAfterAlpha.DrawMesh(TransparentObject.DefaultRenderingMesh, Matrix4x4.identity, materialSunFlear, 0, 1);
                    commandBufferAfterAlpha.ReleaseTemporaryRT(propId_backgroundTexture);
                }
                if (materialTonemaps)
                {
                    materialTonemaps.SetFloat(propId_gamma, gamma);
                    commandBufferAfterAlpha.GetTemporaryRT(propId_backgroundTexture, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
                    commandBufferAfterAlpha.Blit(BuiltinRenderTextureType.CameraTarget, propId_backgroundTexture);
                    //commandBufferAfter.SetGlobalTexture(propId_backgroundTexture, BuiltinRenderTextureType.CameraTarget);
                    commandBufferAfterAlpha.Blit(null, BuiltinRenderTextureType.CameraTarget, materialTonemaps,(int)tonemapType);
                    commandBufferAfterAlpha.ReleaseTemporaryRT(propId_backgroundTexture);
                }
                commandBufferAfterAlpha.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
#if UNITY
                if (EditorCamera)
                {
                    if (commandBufferAfterDepth_EditorCamera == null)
                    {
                        commandBufferAfterDepth_EditorCamera = new CommandBuffer();
                        commandBufferAfterDepth_EditorCamera.name = "RW_PlanetAtmosphere.AfterDepth.EditorCamera";
                        EditorCamera.AddCommandBuffer(CameraEvent.AfterDepthTexture,commandBufferAfterDepth_EditorCamera);
                    }
                    if (commandBufferAfterAlpha_EditorCamera == null)
                    {
                        commandBufferAfterAlpha_EditorCamera = new CommandBuffer();
                        commandBufferAfterAlpha_EditorCamera.name = "RW_PlanetAtmosphere.AfterAlpha.EditorCamera";
                        EditorCamera.AddCommandBuffer(CameraEvent.AfterForwardAlpha,commandBufferAfterAlpha_EditorCamera);
                    }
                    commandBufferAfterDepth_EditorCamera.Clear();
                    commandBufferAfterAlpha_EditorCamera.Clear();
                    if (materialWriteDepth)
                    {
                        commandBufferAfterDepth_EditorCamera.DrawMesh(TransparentObject.DefaultRenderingMesh,Matrix4x4.identity,materialWriteDepth, 0, 0);
                    }
                    TransparentObject.DrawTransparentObjects(objects, commandBufferAfterAlpha_EditorCamera, TargetCamera, BeforeShadow, BackgroundBlendLumen, AfterTrans);
                    if (materialSunFlear)
                    {
                        commandBufferAfterAlpha_EditorCamera.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                        commandBufferAfterAlpha_EditorCamera.DrawMesh(TransparentObject.DefaultRenderingMesh, Matrix4x4.identity, materialSunFlear, 0, 1);
                        commandBufferAfterAlpha_EditorCamera.ReleaseTemporaryRT(propId_backgroundTexture);
                    }
                    if (materialTonemaps)
                    {
                        materialTonemaps.SetFloat(propId_gamma, gamma);
                        commandBufferAfterAlpha_EditorCamera.GetTemporaryRT(propId_backgroundTexture, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
                        commandBufferAfterAlpha_EditorCamera.Blit(BuiltinRenderTextureType.CameraTarget, propId_backgroundTexture);
                        //commandBufferAfter.SetGlobalTexture(propId_backgroundTexture, BuiltinRenderTextureType.CameraTarget);
                        commandBufferAfterAlpha_EditorCamera.Blit(null, BuiltinRenderTextureType.CameraTarget, materialTonemaps,(int)tonemapType);
                        commandBufferAfterAlpha_EditorCamera.ReleaseTemporaryRT(propId_backgroundTexture);
                    }
                    commandBufferAfterAlpha_EditorCamera.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                }
#endif

                
            }
            
        }
    

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


        public void ExposeData()
        {
            Scribe_Values.Look(ref sunFlareTexturePath,"sunFlareTexturePath","Effect/sunFlare",true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref gamma, nameof(gamma), 6, 1, true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref refraction, nameof(refraction), 6, 1.75f, true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref luminescen, nameof(luminescen), 6, 0.25f, true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref sunRadius, nameof(sunRadius), 6, 6960 * scale, true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref sunDistance, nameof(sunDistance), 6, 1495978.92f * scale, true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref planetRadius, nameof(planetRadius), 6, 100, true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref renderingSizeFactor, nameof(renderingSizeFactor), 6, 1, true);
            HelperMethod_Scribe_Values.SaveAndLoadValueFloat(ref closeRenderingDistance, nameof(closeRenderingDistance), 6, 0.5f, true);
            HelperMethod_Scribe_Values.SaveAndLoadValueVec4(ref sunColor, nameof(sunColor), 6, new Vector4(0.8f,0.72f,0.65f, 0), true);
            Scribe_Values.Look(ref tonemapType, nameof(tonemapType), TonemapType.SEUSTonemap, true);
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
            HelperMethod_GUI.GUIFloat(ref sizeY, ref gamma, nameof(gamma).Translate(), ScrollViewSize.x, viewingFromTo, 6);
            HelperMethod_GUI.GUIFloat(ref sizeY, ref refraction, nameof(refraction).Translate(), ScrollViewSize.x, viewingFromTo, 6);
            HelperMethod_GUI.GUIFloat(ref sizeY, ref luminescen, nameof(luminescen).Translate(), ScrollViewSize.x, viewingFromTo, 6);
            HelperMethod_GUI.GUIFloat(ref sizeY, ref sunRadius, nameof(sunRadius).Translate(), ScrollViewSize.x, viewingFromTo, 6);
            HelperMethod_GUI.GUIFloat(ref sizeY, ref sunDistance, nameof(sunDistance).Translate(), ScrollViewSize.x, viewingFromTo, 6);
            HelperMethod_GUI.GUIFloat(ref sizeY, ref planetRadius, nameof(planetRadius).Translate(), ScrollViewSize.x, viewingFromTo, 6);
            HelperMethod_GUI.GUIFloat(ref sizeY, ref renderingSizeFactor, nameof(renderingSizeFactor).Translate(), ScrollViewSize.x, viewingFromTo, 6);
            HelperMethod_GUI.GUIFloat(ref sizeY, ref closeRenderingDistance, nameof(closeRenderingDistance).Translate(), ScrollViewSize.x, viewingFromTo, 6);
            HelperMethod_GUI.GUIVec4(ref sizeY, ref sunColor, nameof(sunColor).Translate(), ScrollViewSize.x, viewingFromTo, 6);
            HelperMethod_GUI.GUIEnum(ref sizeY, tonemapType, nameof(tonemapType).Translate(), ScrollViewSize.x, viewingFromTo, x=>tonemapType=x);
            HelperMethod_GUI.GUIString(ref sizeY, ref sunFlareTexturePath, nameof(sunFlareTexturePath).Translate(), ScrollViewSize.x, viewingFromTo);


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
    
#endif
    }
}