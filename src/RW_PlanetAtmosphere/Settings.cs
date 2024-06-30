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
        public static bool updated = false;
        public static float exposure = 4;
        public static float ground_refract = 1;
        public static float ground_light = 0.01f;
        public static float mie_amount = 3.996f/scale;
        public static float mie_absorb = 1.11f;
        public static float H_Reayleigh = 0.08f*scale;
        public static float H_Mie = 0.02f*scale;
        public static float H_OZone = 0.25f*scale;
        public static float D_OZone = 0.15f*scale;
        public static Vector2 translucentLUTSize = new Vector2(16, 16);
        public static Vector3 SunColor = new Vector3(1,1,1);
        public static Vector3 mie_eccentricity = new Vector3(0.618f,0.618f,0.618f);
        public static Vector3 reayleighScatterFactor = new Vector3(0.46278f,1.25945f,3.10319f)/scale;
        public static Vector3 OZoneAbsorbFactor = new Vector3(0.21195f,0.20962f,0.01686f)/scale;
        public static Vector4 scatterLUTSize = new Vector4( 8, 2, 2, 1);
        public static List<string> cloudTexPath = new List<string>(){"EarthCloudTex/8k_earth_clouds"};
        public static List<Vector3> cloudTexValue = new List<Vector3>(){new Vector3(1.0f,0.01f,0.5f)};


        private static Vector2 scrollPos = Vector2.zero;


        private const float scale = 100f/63.71393f;

        public override void ExposeData()
        {
            base.ExposeData();
            void SaveAndLoadValueFloat(ref float value, string label, float defaultValue = 0, bool forceSave = false)
            {
                value = Math.Abs(value);
                value *= 1024;
                Scribe_Values.Look(ref value, label, defaultValue, forceSave);
                value /= 1024;
                value = Math.Abs(value);
            }
            SaveAndLoadValueFloat(ref exposure, "exposure", defaultValue: 4, forceSave: true);
            SaveAndLoadValueFloat(ref ground_refract, "ground_refract", defaultValue: 1, forceSave: true);
            SaveAndLoadValueFloat(ref ground_light, "ground_light", defaultValue: 0.01f, forceSave: true);
            SaveAndLoadValueFloat(ref mie_amount, "mie_amount", defaultValue: 3.996f/scale, forceSave: true);
            SaveAndLoadValueFloat(ref mie_absorb, "mie_absorb", defaultValue: 1.11f, forceSave: true);
            SaveAndLoadValueFloat(ref H_Reayleigh, "H_Reayleigh", defaultValue: 0.08f*scale, forceSave: true);
            SaveAndLoadValueFloat(ref H_Mie, "H_Mie", defaultValue: 0.02f*scale, forceSave: true);
            SaveAndLoadValueFloat(ref H_OZone, "H_OZone", defaultValue: 0.25f*scale, forceSave: true);
            SaveAndLoadValueFloat(ref D_OZone, "D_OZone", defaultValue: 0.15f*scale, forceSave: true);
            void SaveAndLoadValueVec2(ref Vector2 value, string label, Vector2 defaultValue = default(Vector2), bool forceSave = false)
            {
                value.x = Math.Abs(value.x);
                value.y = Math.Abs(value.y);
                value *= 1024;
                Scribe_Values.Look(ref value, label, defaultValue, forceSave);
                value /= 1024;
                value.x = Math.Abs(value.x);
                value.y = Math.Abs(value.y);
            }
            SaveAndLoadValueVec2(ref translucentLUTSize, "translucentLUTSize", defaultValue: new Vector2(16, 16), forceSave: true);
            void SaveAndLoadValueVec3(ref Vector3 value, string label, Vector3 defaultValue = default(Vector3), bool forceSave = false)
            {
                value.x = Math.Abs(value.x);
                value.y = Math.Abs(value.y);
                value.z = Math.Abs(value.z);
                value *= 1024;
                Scribe_Values.Look(ref value, label, defaultValue, forceSave);
                value /= 1024;
                value.x = Math.Abs(value.x);
                value.y = Math.Abs(value.y);
                value.z = Math.Abs(value.z);
            }
            SaveAndLoadValueVec3(ref SunColor, "SunColor", defaultValue: new Vector3(1, 1, 1), forceSave: true);
            SaveAndLoadValueVec3(ref mie_eccentricity, "mie_eccentricity", defaultValue: new Vector3(0.618f,0.618f,0.618f), forceSave: true);
            SaveAndLoadValueVec3(ref reayleighScatterFactor, "reayleighScatterFactor", defaultValue: new Vector3(0.46278f,1.25945f,3.10319f)/scale, forceSave: true);
            SaveAndLoadValueVec3(ref OZoneAbsorbFactor, "OZoneAbsorbFactor", defaultValue: new Vector3(0.21195f,0.20962f,0.01686f)/scale, forceSave: true);
            void SaveAndLoadValueVec4(ref Vector4 value, string label, Vector4 defaultValue = default(Vector4), bool forceSave = false)
            {
                value.x = Math.Abs(value.x);
                value.y = Math.Abs(value.y);
                value.z = Math.Abs(value.z);
                value.w = Math.Abs(value.w);
                value *= 1024;
                Scribe_Values.Look(ref value, label, defaultValue, forceSave);
                value /= 1024;
                value.x = Math.Abs(value.x);
                value.y = Math.Abs(value.y);
                value.z = Math.Abs(value.z);
                value.w = Math.Abs(value.w);
            }
            SaveAndLoadValueVec4(ref scatterLUTSize, "scatterLUTSize", defaultValue: new Vector4( 8, 2, 2, 1), forceSave: true);


            cloudTexPath = cloudTexPath ?? new List<string>();
            cloudTexValue = cloudTexValue ?? new List<Vector3>();
            for(int i = 0; i < cloudTexPath.Count; i++)
            {
                if(cloudTexPath[i].NullOrEmpty())
                {
                    cloudTexPath.RemoveAt(i);
                    if(i < cloudTexValue.Count) cloudTexValue.RemoveAt(i);
                    i--;
                }
                if(i >= cloudTexValue.Count) cloudTexValue.Add(new Vector3(1.0f,0.01f,0.5f));
                cloudTexValue[i] *= 1024f;
            }
            Scribe_Collections.Look(ref cloudTexPath, "cloudTexPath", LookMode.Value);
            Scribe_Collections.Look(ref cloudTexValue, "cloudTexValue", LookMode.Value);
            cloudTexPath = cloudTexPath ?? new List<string>();
            cloudTexValue = cloudTexValue ?? new List<Vector3>();
            for(int i = 0; i < cloudTexPath.Count; i++)
            {
                if(cloudTexPath[i].NullOrEmpty())
                {
                    cloudTexPath.RemoveAt(i);
                    if(i < cloudTexValue.Count) cloudTexValue.RemoveAt(i);
                    i--;
                }
                if(i >= cloudTexValue.Count) cloudTexValue.Add(new Vector3(1.0f,0.01f,0.5f) * 1024f);
                cloudTexValue[i] /= 1024f;
            }
        }

        public static void DoWindowContents(Rect inRect)
        {
            cloudTexPath = cloudTexPath ?? new List<string>();
            cloudTexPath.RemoveAll(x => x.NullOrEmpty());
            Widgets.DrawLineHorizontal(0,31,inRect.width);
            Vector2 ScrollViewSize = new Vector2(inRect.width,512 + cloudTexPath.Count * 64);
            if(ScrollViewSize.y > inRect.height-64) ScrollViewSize.x -= 36;
            Widgets.BeginScrollView(new Rect(0,32,inRect.width,inRect.height-64),ref scrollPos,new Rect(Vector2.zero, ScrollViewSize));

            float newValue;

            Widgets.Label(new Rect(0,0,ScrollViewSize.x*0.5f,32),"exposure".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,0,ScrollViewSize.x*0.5f,32),exposure.ToString("f5")),out newValue);
            exposure = Math.Abs(newValue);


            Widgets.Label(new Rect(0,32,ScrollViewSize.x*0.5f,32),"ground_refract".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,32,ScrollViewSize.x*0.5f,32),ground_refract.ToString("f5")),out newValue);
            ground_refract = Math.Abs(newValue);


            Widgets.Label(new Rect(0,64,ScrollViewSize.x*0.5f,32),"ground_light".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,64,ScrollViewSize.x*0.5f,32),ground_light.ToString("f5")),out newValue);
            ground_light = Math.Abs(newValue);


            Widgets.Label(new Rect(0,96,ScrollViewSize.x*0.5f,32),"mie_amount".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,96,ScrollViewSize.x*0.5f,32),mie_amount.ToString("f5")),out newValue);
            mie_amount = Math.Abs(newValue);


            Widgets.Label(new Rect(0,128,ScrollViewSize.x*0.5f,32),"mie_absorb".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,128,ScrollViewSize.x*0.5f,32),mie_absorb.ToString("f5")),out newValue);
            mie_absorb = Math.Abs(newValue);


            Widgets.Label(new Rect(0,160,ScrollViewSize.x*0.5f,32),"H_Reayleigh".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,160,ScrollViewSize.x*0.5f,32),H_Reayleigh.ToString("f5")),out newValue);
            H_Reayleigh = Math.Abs(newValue);


            Widgets.Label(new Rect(0,192,ScrollViewSize.x*0.5f,32),"H_Mie".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,192,ScrollViewSize.x*0.5f,32),H_Mie.ToString("f5")),out newValue);
            H_Mie = Math.Abs(newValue);


            Widgets.Label(new Rect(0,224,ScrollViewSize.x*0.5f,32),"H_OZone".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,224,ScrollViewSize.x*0.5f,32),H_OZone.ToString("f5")),out newValue);
            H_OZone = Math.Abs(newValue);


            Widgets.Label(new Rect(0,256,ScrollViewSize.x*0.5f,32),"D_OZone".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,256,ScrollViewSize.x*0.5f,32),D_OZone.ToString("f5")),out newValue);
            D_OZone = Math.Abs(newValue);


            Widgets.Label(new Rect(0,288,ScrollViewSize.x*0.5f,32),"translucentLUTSize".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,288,ScrollViewSize.x*0.5f/2f,32),translucentLUTSize.x.ToString("f5")),out newValue);
            translucentLUTSize.x = (int)newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*3f/2f,288,ScrollViewSize.x*0.5f/2f,32),translucentLUTSize.y.ToString("f5")),out newValue);
            translucentLUTSize.y = (int)newValue;


            Widgets.Label(new Rect(0,320,ScrollViewSize.x*0.5f,32),"SunColor".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,320,ScrollViewSize.x*0.5f/3f,32),SunColor.x.ToString("f5")),out newValue);
            SunColor.x = Math.Abs(newValue);
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*4f/3f,320,ScrollViewSize.x*0.5f/3f,32),SunColor.y.ToString("f5")),out newValue);
            SunColor.y = Math.Abs(newValue);
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/3f,320,ScrollViewSize.x*0.5f/3f,32),SunColor.z.ToString("f5")),out newValue);
            SunColor.z = Math.Abs(newValue);


            Widgets.Label(new Rect(0,352,ScrollViewSize.x*0.5f,32),"mie_eccentricity".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,352,ScrollViewSize.x*0.5f/3f,32),mie_eccentricity.x.ToString("f5")),out newValue);
            mie_eccentricity.x = Math.Abs(newValue);
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*4f/3f,352,ScrollViewSize.x*0.5f/3f,32),mie_eccentricity.y.ToString("f5")),out newValue);
            mie_eccentricity.y = Math.Abs(newValue);
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/3f,352,ScrollViewSize.x*0.5f/3f,32),mie_eccentricity.z.ToString("f5")),out newValue);
            mie_eccentricity.z = Math.Abs(newValue);


            Widgets.Label(new Rect(0,384,ScrollViewSize.x*0.5f,32),"reayleighScatterFactor".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,384,ScrollViewSize.x*0.5f/3f,32),reayleighScatterFactor.x.ToString("f5")),out newValue);
            reayleighScatterFactor.x = Math.Abs(newValue);
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*4f/3f,384,ScrollViewSize.x*0.5f/3f,32),reayleighScatterFactor.y.ToString("f5")),out newValue);
            reayleighScatterFactor.y = Math.Abs(newValue);
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/3f,384,ScrollViewSize.x*0.5f/3f,32),reayleighScatterFactor.z.ToString("f5")),out newValue);
            reayleighScatterFactor.z = Math.Abs(newValue);


            Widgets.Label(new Rect(0,416,ScrollViewSize.x*0.5f,32),"OZoneAbsorbFactor".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,416,ScrollViewSize.x*0.5f/3f,32),OZoneAbsorbFactor.x.ToString("f5")),out newValue);
            OZoneAbsorbFactor.x = Math.Abs(newValue);
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*4f/3f,416,ScrollViewSize.x*0.5f/3f,32),OZoneAbsorbFactor.y.ToString("f5")),out newValue);
            OZoneAbsorbFactor.y = Math.Abs(newValue);
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/3f,416,ScrollViewSize.x*0.5f/3f,32),OZoneAbsorbFactor.z.ToString("f5")),out newValue);
            OZoneAbsorbFactor.z = Math.Abs(newValue);


            Widgets.Label(new Rect(0,448,ScrollViewSize.x*0.5f,32),"scatterLUTSize".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,448,ScrollViewSize.x*0.5f/4f,32),scatterLUTSize.x.ToString("f5")),out newValue);
            scatterLUTSize.x = (int)newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/4f,448,ScrollViewSize.x*0.5f/4f,32),scatterLUTSize.y.ToString("f5")),out newValue);
            scatterLUTSize.y = (int)newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*6f/4f,448,ScrollViewSize.x*0.5f/4f,32),scatterLUTSize.z.ToString("f5")),out newValue);
            scatterLUTSize.z = (int)newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*7f/4f,448,ScrollViewSize.x*0.5f/4f,32),scatterLUTSize.w.ToString("f5")),out newValue);
            scatterLUTSize.w = (int)newValue;

            Widgets.Label(new Rect(0,480,ScrollViewSize.x*0.5f,32),"cloudTexPath".Translate());
            for(int i = 0; i < cloudTexPath.Count; i++)
            {
                if(cloudTexPath[i].NullOrEmpty())
                {
                    cloudTexPath.RemoveAt(i);
                    if(i < cloudTexValue.Count) cloudTexValue.RemoveAt(i);
                    i--;
                }
                if(i >= cloudTexValue.Count) cloudTexValue.Add(new Vector3(1.0f,0.01f,0.5f));
            }

            
            for(int i = 0; i < cloudTexPath.Count; i++)
            {
                cloudTexPath[i] = Widgets.TextField(new Rect(ScrollViewSize.x*0.5f, 480 + 64 * i, ScrollViewSize.x*0.5f, 32), cloudTexPath[i]);
                Vector3 vector = new Vector3(1.0f,0.01f,0.5f);
                if(i < cloudTexValue.Count) vector = cloudTexValue[i];
                else cloudTexValue.Add(vector);
                float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f, 512 + 64 * i, ScrollViewSize.x*0.5f/3f, 32),vector.x.ToString("f5")),out newValue);
                vector.x = Math.Abs(newValue);
                float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*4f/3f, 512 + 64 * i, ScrollViewSize.x*0.5f/3f, 32),vector.y.ToString("f5")),out newValue);
                vector.y = Math.Abs(newValue);
                float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/3f, 512 + 64 * i, ScrollViewSize.x*0.5f/3f, 32),vector.z.ToString("f5")),out newValue);
                vector.z = Math.Abs(newValue);
                cloudTexValue[i] = vector;
            }

            // Log.Message($"new path : {480 + 32 * cloudTexPath.Count}; ScrollViewSize.y : {ScrollViewSize.y}");
            string newPath = "";
            newPath = Widgets.TextField(new Rect(ScrollViewSize.x*0.5f, 480 + 64 * cloudTexPath.Count, ScrollViewSize.x*0.5f, 32), newPath);
            if(newPath.Length > 0)
            {
                cloudTexPath.Add(newPath);
                cloudTexValue.Add(new Vector3(1.0f,0.01f,0.5f));
            }

            Widgets.DrawLineVertical(ScrollViewSize.x*0.5f,0,ScrollViewSize.y);
            Widgets.EndScrollView();

            if(Widgets.ButtonText(new Rect(0,inRect.height-32,inRect.width*0.5f,32), "apply".Translate()))
            {
                updated = false;
            }

            if(Widgets.ButtonText(new Rect(inRect.width*0.5f,inRect.height-32,inRect.width*0.5f,32), "reset".Translate()))
            {
                exposure = 4;
                ground_refract = 1;
                ground_light = 0.01f;
                mie_amount = 3.996f/scale;
                mie_absorb = 1.11f;
                H_Reayleigh = 0.08f*scale;
                H_Mie = 0.02f*scale;
                H_OZone = 0.25f*scale;
                D_OZone = 0.15f*scale;
                translucentLUTSize = new Vector2(16, 16);
                SunColor = new Vector3(1,1,1);
                mie_eccentricity = new Vector3(0.618f,0.618f,0.618f);
                reayleighScatterFactor = new Vector3(0.46278f,1.25945f,3.10319f)/scale;
                OZoneAbsorbFactor = new Vector3(0.21195f,0.20962f,0.01686f)/scale;
                scatterLUTSize = new Vector4( 8, 2, 2, 1);
                cloudTexPath = new List<string>(){"EarthCloudTex/8k_earth_clouds"};
                cloudTexValue = new List<Vector3>(){new Vector3(1.0f,0.01f,0.5f)};
                updated = false;
            }

            ShaderLoader.materialSkyLUT.SetFloat("exposure", exposure);
            ShaderLoader.materialSkyLUT.SetFloat("ground_refract", ground_refract);
            ShaderLoader.materialSkyLUT.SetFloat("ground_light", ground_light);
            ShaderLoader.materialSkyLUT.SetVector("SunColor", SunColor);
            ShaderLoader.materialSkyLUT.SetVector("mie_eccentricity", mie_eccentricity);

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