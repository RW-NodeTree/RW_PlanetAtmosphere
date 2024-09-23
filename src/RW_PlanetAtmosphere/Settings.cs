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
        public static bool          updated                     = false;
        public static float         exposure                    = 16;
        public static float         ground_refract              = 0.1f;
        public static float         ground_light                = 0.025f;
        public static float         deltaL                 = 8.0f;
        public static float         deltaW                 = 4.0f;
        public static float         lengthL                = 1.0f;
        public static float         lengthW                = 1.0f;
        public static float         H_Reayleigh                 = 0.08f*AtmosphereSettings.scale;
        public static float         H_Mie                       = 0.02f*AtmosphereSettings.scale;
        public static float         H_OZone                     = 0.25f*AtmosphereSettings.scale;
        public static float         D_OZone                     = 0.15f*AtmosphereSettings.scale;
        public static Vector2       translucentLUT_Size         = new Vector2(16, 16);
        public static Vector4       reayleigh_scatter           = new Vector4(0.46278f,1.25945f,3.10319f,11.69904f)/AtmosphereSettings.scale;
        public static Vector4       molecule_absorb             = Vector4.zero;
        public static Vector4       OZone_absorb                = new Vector4(0.0f,0.0f,0.0f,6.4f)/AtmosphereSettings.scale;
        public static Vector4       mie_scatter                 = Vector4.one * 3.996f / AtmosphereSettings.scale;
        public static Vector4       mie_absorb                  = Vector4.one * 4.44f / AtmosphereSettings.scale;
        public static Vector4       mie_eccentricity            = new Vector4(0.618f,0.618f,0.618f,0.618f);
        public static Vector4       SunColor                    = new Vector4(0.8f,0.72f,0.65f,0);
        public static Vector4       scatterLUT_Size             = new Vector4( 8, 2, 1, 2);
        public static List<string>  cloudTexPath                = new List<string>(){"EarthCloudTex/8k_earth_clouds"};
        public static List<Vector4> cloudTexValue               = new List<Vector4>(){new Vector4(1.0f,0.0f,0.5f,0.05f)};
        public static List<string>  noiseTexPath                = new List<string>(){"EarthCloudTex/noise"};
        public static List<Vector2> noiseTexValue               = new List<Vector2>(){new Vector2(0.0f,0.015625f)};


        private static Vector2 scrollPos = Vector2.zero;
        private static Vector2 scrollPosDev = Vector2.zero;
        private static float sizeY = 0;


        internal const float scale = 100f/63.71393f;

        public override void ExposeData()
        {
            base.ExposeData();
            void SaveAndLoadValueFloat(ref float value, string label, float defaultValue = 0, bool forceSave = false)
            {
                value = Math.Abs(value);
                value *= 1000;
                Scribe_Values.Look(ref value, label, defaultValue, forceSave);
                value /= 1000;
                value = Math.Abs(value);
            }
            SaveAndLoadValueFloat(ref exposure, "exposure", defaultValue: 4, forceSave: true);
            SaveAndLoadValueFloat(ref ground_refract, "ground_refract", defaultValue: 1, forceSave: true);
            SaveAndLoadValueFloat(ref ground_light, "ground_light", defaultValue: 0.01f, forceSave: true);
            SaveAndLoadValueFloat(ref deltaL, "deltaL", defaultValue: 8.0f, forceSave: true);
            SaveAndLoadValueFloat(ref lengthL, "lengthL", defaultValue: 1.0f, forceSave: true);
            SaveAndLoadValueFloat(ref deltaW, "deltaW", defaultValue: 4.0f, forceSave: true);
            SaveAndLoadValueFloat(ref lengthW, "lengthW", defaultValue: 1.0f, forceSave: true);
            SaveAndLoadValueFloat(ref H_Reayleigh, "H_Reayleigh", defaultValue: 0.08f*scale, forceSave: true);
            SaveAndLoadValueFloat(ref H_Mie, "H_Mie", defaultValue: 0.02f*scale, forceSave: true);
            SaveAndLoadValueFloat(ref H_OZone, "H_OZone", defaultValue: 0.25f*scale, forceSave: true);
            SaveAndLoadValueFloat(ref D_OZone, "D_OZone", defaultValue: 0.15f*scale, forceSave: true);
            void SaveAndLoadValueVec2(ref Vector2 value, string label, Vector2 defaultValue = default(Vector2), bool forceSave = false)
            {
                value.x = Math.Abs(value.x);
                value.y = Math.Abs(value.y);
                value *= 1000;
                Scribe_Values.Look(ref value, label, defaultValue, forceSave);
                value /= 1000;
                value.x = Math.Abs(value.x);
                value.y = Math.Abs(value.y);
            }
            SaveAndLoadValueVec2(ref translucentLUT_Size, "translucentLUT_Size", defaultValue: new Vector2(16, 16), forceSave: true);
            // void SaveAndLoadValueVec3(ref Vector3 value, string label, Vector3 defaultValue = default(Vector3), bool forceSave = false)
            // {
            //     value.x = Math.Abs(value.x);
            //     value.y = Math.Abs(value.y);
            //     value.z = Math.Abs(value.z);
            //     value *= 1024;
            //     Scribe_Values.Look(ref value, label, defaultValue, forceSave);
            //     value /= 1024;
            //     value.x = Math.Abs(value.x);
            //     value.y = Math.Abs(value.y);
            //     value.z = Math.Abs(value.z);
            // }
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
            SaveAndLoadValueVec4(ref reayleigh_scatter, "reayleigh_scatter", defaultValue: new Vector4(0.46278f,1.25945f,3.10319f,11.69904f)/scale, forceSave: true);
            SaveAndLoadValueVec4(ref molecule_absorb, "molecule_absorb", defaultValue: Vector4.zero, forceSave: true);
            SaveAndLoadValueVec4(ref OZone_absorb, "OZone_absorb", defaultValue: new Vector4(0.0f,0.0f,0.0f,6.4f)/scale, forceSave: true);
            SaveAndLoadValueVec4(ref mie_scatter, "mie_scatter", defaultValue: Vector4.one * 3.996f / scale, forceSave: true);
            SaveAndLoadValueVec4(ref mie_absorb, "mie_absorb", defaultValue: Vector4.one * 4.44f / scale, forceSave: true);
            SaveAndLoadValueVec4(ref mie_eccentricity, "mie_eccentricity", defaultValue: new Vector4(0.618f,0.618f,0.618f,0.618f), forceSave: true);
            SaveAndLoadValueVec4(ref SunColor, "SunColor", defaultValue: new Vector4(1, 1, 1, 0), forceSave: true);
            SaveAndLoadValueVec4(ref scatterLUT_Size, "scatterLUT_Size", defaultValue: new Vector4( 8, 2, 2, 1), forceSave: true);


            cloudTexPath = cloudTexPath ?? new List<string>();
            cloudTexValue = cloudTexValue ?? new List<Vector4>();
            noiseTexPath= noiseTexPath ?? new List<string>();
            noiseTexValue = noiseTexValue ?? new List<Vector2>();

            cloudTexPath.RemoveAll(x => x.NullOrEmpty());
            for(int i = cloudTexValue.Count; i < cloudTexPath.Count; i++)
            {
                cloudTexValue.Add(new Vector4(1.0f,0.0f,0.5f,0.05f));
            }
            if(cloudTexValue.Count > cloudTexPath.Count) cloudTexValue.RemoveRange(cloudTexPath.Count, cloudTexValue.Count - cloudTexPath.Count);
            for(int i = noiseTexPath.Count; i < noiseTexPath.Count; i++)
            {
                noiseTexPath.Add("EarthCloudTex/noise");
            }
            if(noiseTexPath.Count > cloudTexPath.Count) noiseTexPath.RemoveRange(cloudTexPath.Count, noiseTexPath.Count - cloudTexPath.Count);
            for(int i = noiseTexValue.Count; i < cloudTexPath.Count; i++)
            {
                noiseTexValue.Add(new Vector2(0.0f,0.015625f));
            }
            if(noiseTexValue.Count > cloudTexPath.Count) noiseTexValue.RemoveRange(cloudTexPath.Count, noiseTexValue.Count - cloudTexPath.Count);

            for(int i = 0; i < cloudTexPath.Count; i++)
            {
                cloudTexValue[i] *= 1000;
                noiseTexValue[i] *= 1000;
            }

            Scribe_Collections.Look(ref cloudTexPath, "cloudTexPath", LookMode.Value);
            Scribe_Collections.Look(ref cloudTexValue, "cloudTexValue", LookMode.Value);
            Scribe_Collections.Look(ref noiseTexPath, "noiseTexPath", LookMode.Value);
            Scribe_Collections.Look(ref noiseTexValue, "noiseTexValue", LookMode.Value);

            for(int i = 0; i < cloudTexPath.Count; i++)
            {
                cloudTexValue[i] /= 1000;
                noiseTexValue[i] /= 1000;
            }


            cloudTexPath = cloudTexPath ?? new List<string>();
            cloudTexValue = cloudTexValue ?? new List<Vector4>();
            noiseTexPath= noiseTexPath ?? new List<string>();
            noiseTexValue = noiseTexValue ?? new List<Vector2>();

            cloudTexPath.RemoveAll(x => x.NullOrEmpty());
            for(int i = cloudTexValue.Count; i < cloudTexPath.Count; i++)
            {
                cloudTexValue.Add(new Vector4(1.0f,0.0f,0.5f,0.05f));
            }
            if(cloudTexValue.Count > cloudTexPath.Count) cloudTexValue.RemoveRange(cloudTexPath.Count, cloudTexValue.Count - cloudTexPath.Count);
            for(int i = noiseTexPath.Count; i < noiseTexPath.Count; i++)
            {
                noiseTexPath.Add("EarthCloudTex/noise");
            }
            if(noiseTexPath.Count > cloudTexPath.Count) noiseTexPath.RemoveRange(cloudTexPath.Count, noiseTexPath.Count - cloudTexPath.Count);
            for(int i = noiseTexValue.Count; i < cloudTexPath.Count; i++)
            {
                noiseTexValue.Add(new Vector2(0.0f,0.015625f));
            }
            if(noiseTexValue.Count > cloudTexPath.Count) noiseTexValue.RemoveRange(cloudTexPath.Count, noiseTexValue.Count - cloudTexPath.Count);
        }

        public static void DoWindowContents(Rect inRect)
        {
            cloudTexPath = cloudTexPath ?? new List<string>();
            cloudTexPath.RemoveAll(x => x.NullOrEmpty());
            Widgets.DrawLineHorizontal(0,31,inRect.width);
            Vector2 ScrollViewSize = new Vector2(inRect.width,sizeY);
            if(ScrollViewSize.y > inRect.height-64) ScrollViewSize.x -= 36;
            Widgets.BeginScrollView(new Rect(0,32,inRect.width,inRect.height-64),ref scrollPos,new Rect(Vector2.zero, ScrollViewSize));

            float newValue;

            sizeY = 0;

            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"exposure".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f,32),exposure.ToString("f5")),out newValue);
            exposure = Math.Abs(newValue);
            sizeY+=32;


            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"ground_refract".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f,32),ground_refract.ToString("f5")),out newValue);
            ground_refract = Math.Abs(newValue);
            sizeY+=32;


            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"ground_light".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f,32),ground_light.ToString("f5")),out newValue);
            ground_light = Math.Abs(newValue);
            sizeY+=32;


            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"H_Reayleigh".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f,32),H_Reayleigh.ToString("f5")),out newValue);
            H_Reayleigh = Math.Abs(newValue);
            sizeY+=32;


            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"deltaL".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f,32),deltaL.ToString("f5")),out newValue);
            deltaL = Math.Abs(newValue);
            sizeY+=32;


            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"lengthL".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f,32),lengthL.ToString("f5")),out newValue);
            lengthL = Math.Abs(newValue);
            sizeY+=32;


            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"deltaW".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f,32),deltaW.ToString("f5")),out newValue);
            deltaW = Math.Abs(newValue);
            sizeY+=32;


            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"lengthW".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f,32),lengthW.ToString("f5")),out newValue);
            lengthW = Math.Abs(newValue);
            sizeY+=32;


            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"H_Mie".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f,32),H_Mie.ToString("f5")),out newValue);
            H_Mie = Math.Abs(newValue);
            sizeY+=32;


            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"H_OZone".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f,32),H_OZone.ToString("f5")),out newValue);
            H_OZone = Math.Abs(newValue);
            sizeY+=32;


            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"D_OZone".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f,32),D_OZone.ToString("f5")),out newValue);
            D_OZone = Math.Abs(newValue);
            sizeY+=32;


            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"translucentLUTSize".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f/2f,32),translucentLUT_Size.x.ToString("f5")),out newValue);
            translucentLUT_Size.x = (int)newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*3f/2f,sizeY,ScrollViewSize.x*0.5f/2f,32),translucentLUT_Size.y.ToString("f5")),out newValue);
            translucentLUT_Size.y = (int)newValue;
            sizeY+=32;


            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"reayleigh_scatter".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f/4f,32),reayleigh_scatter.x.ToString("f5")),out newValue);
            reayleigh_scatter.x = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),reayleigh_scatter.y.ToString("f5")),out newValue);
            reayleigh_scatter.y = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*6f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),reayleigh_scatter.z.ToString("f5")),out newValue);
            reayleigh_scatter.z = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*7f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),reayleigh_scatter.w.ToString("f5")),out newValue);
            reayleigh_scatter.w = newValue;
            sizeY+=32;


            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"molecule_absorb".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f/4f,32),molecule_absorb.x.ToString("f5")),out newValue);
            molecule_absorb.x = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),molecule_absorb.y.ToString("f5")),out newValue);
            molecule_absorb.y = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*6f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),molecule_absorb.z.ToString("f5")),out newValue);
            molecule_absorb.z = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*7f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),molecule_absorb.w.ToString("f5")),out newValue);
            molecule_absorb.w = newValue;
            sizeY+=32;


            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"OZone_absorb".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f/4f,32),OZone_absorb.x.ToString("f5")),out newValue);
            OZone_absorb.x = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),OZone_absorb.y.ToString("f5")),out newValue);
            OZone_absorb.y = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*6f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),OZone_absorb.z.ToString("f5")),out newValue);
            OZone_absorb.z = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*7f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),OZone_absorb.w.ToString("f5")),out newValue);
            OZone_absorb.w = newValue;
            sizeY+=32;


            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"mie_scatter".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f/4f,32),mie_scatter.x.ToString("f5")),out newValue);
            mie_scatter.x = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),mie_scatter.y.ToString("f5")),out newValue);
            mie_scatter.y = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*6f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),mie_scatter.z.ToString("f5")),out newValue);
            mie_scatter.z = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*7f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),mie_scatter.w.ToString("f5")),out newValue);
            mie_scatter.w = newValue;
            sizeY+=32;


            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"mie_absorb".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f/4f,32),mie_absorb.x.ToString("f5")),out newValue);
            mie_absorb.x = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),mie_absorb.y.ToString("f5")),out newValue);
            mie_absorb.y = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*6f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),mie_absorb.z.ToString("f5")),out newValue);
            mie_absorb.z = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*7f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),mie_absorb.w.ToString("f5")),out newValue);
            mie_absorb.w = newValue;
            sizeY+=32;


            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"mie_eccentricity".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f/4f,32),mie_eccentricity.x.ToString("f5")),out newValue);
            mie_eccentricity.x = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),mie_eccentricity.y.ToString("f5")),out newValue);
            mie_eccentricity.y = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*6f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),mie_eccentricity.z.ToString("f5")),out newValue);
            mie_eccentricity.z = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*7f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),mie_eccentricity.w.ToString("f5")),out newValue);
            mie_eccentricity.w = newValue;
            sizeY+=32;


            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"SunColor".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f/4f,32),SunColor.x.ToString("f5")),out newValue);
            SunColor.x = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),SunColor.y.ToString("f5")),out newValue);
            SunColor.y = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*6f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),SunColor.z.ToString("f5")),out newValue);
            SunColor.z = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*7f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),SunColor.w.ToString("f5")),out newValue);
            SunColor.w = newValue;
            sizeY+=32;


            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"scatterLUTSize".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,sizeY,ScrollViewSize.x*0.5f/4f,32),scatterLUT_Size.x.ToString("f5")),out newValue);
            scatterLUT_Size.x = (int)newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),scatterLUT_Size.y.ToString("f5")),out newValue);
            scatterLUT_Size.y = (int)newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*6f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),scatterLUT_Size.z.ToString("f5")),out newValue);
            scatterLUT_Size.z = (int)newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*7f/4f,sizeY,ScrollViewSize.x*0.5f/4f,32),scatterLUT_Size.w.ToString("f5")),out newValue);
            scatterLUT_Size.w = (int)newValue;
            sizeY+=32;


            cloudTexPath = cloudTexPath ?? new List<string>();
            cloudTexValue = cloudTexValue ?? new List<Vector4>();
            noiseTexPath= noiseTexPath ?? new List<string>();
            noiseTexValue = noiseTexValue ?? new List<Vector2>();
            cloudTexPath.RemoveAll(x => x.NullOrEmpty());
            for(int i = cloudTexValue.Count; i < cloudTexPath.Count; i++)
            {
                cloudTexValue.Add(new Vector4(1.0f,0.0f,0.5f,0.05f));
            }
            if(cloudTexValue.Count > cloudTexPath.Count) cloudTexValue.RemoveRange(cloudTexPath.Count, cloudTexValue.Count - cloudTexPath.Count);
            for(int i = noiseTexPath.Count; i < noiseTexPath.Count; i++)
            {
                noiseTexPath.Add("EarthCloudTex/noise");
            }
            if(noiseTexPath.Count > cloudTexPath.Count) noiseTexPath.RemoveRange(cloudTexPath.Count, noiseTexPath.Count - cloudTexPath.Count);
            for(int i = noiseTexValue.Count; i < cloudTexPath.Count; i++)
            {
                noiseTexValue.Add(new Vector2(0.0f,0.015625f));
            }
            if(noiseTexValue.Count > cloudTexPath.Count) noiseTexValue.RemoveRange(cloudTexPath.Count, noiseTexValue.Count - cloudTexPath.Count);
            for(int i = 0; i < cloudTexPath.Count; i++)
            {
                Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"cloudTexPath".Translate() + ":" + i.ToString());
                cloudTexPath[i] = Widgets.TextField(new Rect(ScrollViewSize.x*0.5f, sizeY, ScrollViewSize.x*0.5f, 32), cloudTexPath[i]);
                sizeY+=32;

                Widgets.Label(new Rect(ScrollViewSize.x*0.1f,sizeY,ScrollViewSize.x*0.4f,32),"cloudTexValue".Translate());
                Vector4 vector = new Vector4(1.0f,0.01f,0.5f,0.05f);
                if(i < cloudTexValue.Count) vector = cloudTexValue[i];
                else cloudTexValue.Add(vector);
                float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f         , sizeY, ScrollViewSize.x*0.5f/4f, 32),vector.x.ToString("f5")),out newValue);
                vector.x = Math.Abs(newValue);
                float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/4f   , sizeY, ScrollViewSize.x*0.5f/4f, 32),vector.y.ToString("f5")),out newValue);
                vector.y = Math.Abs(newValue);
                float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*6f/4f   , sizeY, ScrollViewSize.x*0.5f/4f, 32),vector.z.ToString("f5")),out newValue);
                vector.z = Math.Abs(newValue);
                float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*7f/4f   , sizeY, ScrollViewSize.x*0.5f/4f, 32),vector.w.ToString("f5")),out newValue);
                vector.w = Math.Abs(newValue);
                cloudTexValue[i] = vector;
                sizeY+=32;

                Widgets.Label(new Rect(ScrollViewSize.x*0.1f,sizeY,ScrollViewSize.x*0.4f,32),"noiseTexPath".Translate());
                string noise = "EarthCloudTex/noise";
                if(i < noiseTexPath.Count) noise = noiseTexPath[i];
                else noiseTexPath.Add(noise);
                noiseTexPath[i] = Widgets.TextField(new Rect(ScrollViewSize.x*0.5f, sizeY, ScrollViewSize.x*0.5f, 32), noise);
                sizeY+=32;

                Widgets.Label(new Rect(ScrollViewSize.x*0.1f,sizeY,ScrollViewSize.x*0.4f,32),"noiseTexValue".Translate());
                vector = new Vector2(0.0f,0.015625f);
                if(i < noiseTexValue.Count) vector = noiseTexValue[i];
                else noiseTexValue.Add(vector);
                float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f         , sizeY, ScrollViewSize.x*0.5f/2f, 32),vector.x.ToString("f5")),out newValue);
                vector.x = newValue;
                float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*3f/2f   , sizeY, ScrollViewSize.x*0.5f/2f, 32),vector.y.ToString("f5")),out newValue);
                vector.y = Mathf.Abs(newValue);
                noiseTexValue[i] = vector;
                sizeY+=32;
            }

            // Log.Message($"new path : {480 + 32 * cloudTexPath.Count}; ScrollViewSize.y : {ScrollViewSize.y}");
            Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x*0.5f,32),"cloudTexPath".Translate() + ":" + "new".Translate());
            string newPath = "";
            newPath = Widgets.TextField(new Rect(ScrollViewSize.x*0.5f, sizeY, ScrollViewSize.x*0.5f, 32), newPath);
            if(newPath.Length > 0)
            {
                cloudTexPath.Add(newPath);
                cloudTexValue.Add(new Vector4(1.0f,0.01f,0.5f,0.05f));
                noiseTexPath.Add("EarthCloudTex/noise");
                noiseTexValue.Add(new Vector3(1.0f,0.0f,0.015625f));
            }
            sizeY+=32;

            Widgets.DrawLineVertical(ScrollViewSize.x*0.5f,0,sizeY);
            Widgets.DrawLineHorizontal(0,sizeY,inRect.width);

            if(Prefs.DevMode)
            {
                Widgets.Label(new Rect(0,sizeY,ScrollViewSize.x,32),"LUT");
                sizeY+=32;
                Widgets.DrawLineHorizontal(0,sizeY,inRect.width);
                int scatterLUT_Mie_Pos = ShaderLoader.scatterLUT_Reayleigh.height;
                int translucentLUT_Pos = scatterLUT_Mie_Pos << 1;
                int height = translucentLUT_Pos + ShaderLoader.translucentLUT.height;
                int width = Mathf.Max(ShaderLoader.scatterLUT_Reayleigh.width,ShaderLoader.translucentLUT.width);
                Widgets.BeginScrollView(new Rect(0,sizeY,inRect.width,height + GUI.skin.horizontalScrollbar.fixedWidth),ref scrollPosDev,new Rect(Vector2.zero, new Vector2(width,height)));
                // Widgets.DrawTextureFitted()
                Widgets.DrawTextureFitted(new Rect(0, 0, ShaderLoader.scatterLUT_Reayleigh.width, ShaderLoader.scatterLUT_Reayleigh.height), ShaderLoader.scatterLUT_Reayleigh,1);
                Widgets.DrawTextureFitted(new Rect(0, scatterLUT_Mie_Pos, ShaderLoader.scatterLUT_Mie.width, ShaderLoader.scatterLUT_Mie.height), ShaderLoader.scatterLUT_Mie,1);
                Widgets.DrawTextureFitted(new Rect(0, translucentLUT_Pos, ShaderLoader.translucentLUT.width, ShaderLoader.translucentLUT.height), ShaderLoader.translucentLUT,1);
                Widgets.EndScrollView();
                sizeY += height + GUI.skin.horizontalScrollbar.fixedWidth;
            }
            Widgets.EndScrollView();

            if(Widgets.ButtonText(new Rect(0,inRect.height-32,inRect.width*0.5f,32), "apply".Translate()))
            {
                updated = false;
            }

            if(Widgets.ButtonText(new Rect(inRect.width*0.5f,inRect.height-32,inRect.width*0.5f,32), "reset".Translate()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach(AtmosphereDef def in DefDatabase<AtmosphereDef>.AllDefs)
                {
                    options.Add(new FloatMenuOption(def.defName,delegate()
                    {
                        exposure = def.exposure;
                        ground_refract = def.ground_refract;
                        ground_light = def.ground_light;
                        deltaL = def.deltaL;
                        lengthL = def.lengthL;
                        deltaW = def.deltaW;
                        lengthW = def.lengthW;
                        H_Reayleigh = def.H_Reayleigh;
                        H_Mie = def.H_Mie;
                        H_OZone = def.H_OZone;
                        D_OZone = def.D_OZone;
                        translucentLUT_Size = def.translucentLUT_Size;
                        mie_scatter = def.mie_scatter;
                        mie_absorb = def.mie_absorb;
                        SunColor = def.SunColor;
                        mie_eccentricity = def.mie_eccentricity;
                        reayleigh_scatter = def.reayleigh_scatter;
                        OZone_absorb = def.OZone_absorb;
                        scatterLUT_Size = def.scatterLUT_Size;
                        cloudTexPath = def.cloudTexPath;
                        cloudTexValue = def.cloudTexValue;
                        noiseTexPath = def.noiseTexPath;
                        noiseTexValue = def.noiseTexValue;
                        translucentLUT_Size.x = (int)Math.Abs(translucentLUT_Size.x);
                        translucentLUT_Size.y = (int)Math.Abs(translucentLUT_Size.y);
                        scatterLUT_Size.x = (int)Math.Abs(scatterLUT_Size.x);
                        scatterLUT_Size.y = (int)Math.Abs(scatterLUT_Size.y);
                        scatterLUT_Size.z = (int)Math.Abs(scatterLUT_Size.z);
                        scatterLUT_Size.w = (int)Math.Abs(scatterLUT_Size.w);
                        updated = false;
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