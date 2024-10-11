using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;
using System;

namespace RW_PlanetAtmosphere
{
    public static class HelperMethod_Scribe_Values
    {
        public static void SaveAndLoadValueFloat(ref float value, string label, int decimalPlaces = 1, float defaultValue = 0, bool forceSave = false)
        {
            decimalPlaces = Math.Max(decimalPlaces,1) - 1;
            int mulValue = 1;
            for(int i = 0; i < decimalPlaces; i++) mulValue *= 10;
            value *= mulValue;
            Scribe_Values.Look(ref value, label, defaultValue, forceSave);
            value /= mulValue;
        }
        public static void SaveAndLoadValueVec2(ref Vector2 value, string label, int decimalPlaces = 1, Vector2 defaultValue = default(Vector2), bool forceSave = false)
        {
            decimalPlaces = Math.Max(decimalPlaces,1) - 1;
            int mulValue = 1;
            for(int i = 0; i < decimalPlaces; i++) mulValue *= 10;
            value *= mulValue;
            Scribe_Values.Look(ref value, label, defaultValue, forceSave);
            value /= mulValue;
        }
        public static void SaveAndLoadValueVec3(ref Vector3 value, string label, int decimalPlaces = 1, Vector3 defaultValue = default(Vector3), bool forceSave = false)
        {
            decimalPlaces = Math.Max(decimalPlaces,1) - 1;
            int mulValue = 1;
            for(int i = 0; i < decimalPlaces; i++) mulValue *= 10;
            value *= mulValue;
            Scribe_Values.Look(ref value, label, defaultValue, forceSave);
            value /= mulValue;
        }
        public static void SaveAndLoadValueVec4(ref Vector4 value, string label, int decimalPlaces = 1, Vector4 defaultValue = default(Vector4), bool forceSave = false)
        {
            decimalPlaces = Math.Max(decimalPlaces,1) - 1;
            int mulValue = 1;
            for(int i = 0; i < decimalPlaces; i++) mulValue *= 10;
            value *= mulValue;
            Scribe_Values.Look(ref value, label, defaultValue, forceSave);
            value /= mulValue;
        }
    }

    public static class HelperMethod_GUI
    {
        public static void GUILabelInFontSize(Rect rect, string str)
        {
            if(str == null) return;
            int fontSize = Text.CurFontStyle.fontSize;
            Text.CurFontStyle.fontSize = (int)rect.height;
            Widgets.Label(rect,str);
            Text.CurFontStyle.fontSize = fontSize;
        }
        public static bool GUIDragDownButton(Vector2 pos, bool state, float size)
        {
            Rect dropDownMark = new Rect(pos.x,pos.y,size,size);
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            GUILabelInFontSize(dropDownMark,state?"▼":"▶");
            Text.Anchor = anchor;
            if(Widgets.ButtonInvisible(dropDownMark,false)) state = !state;
            else Widgets.DrawHighlightIfMouseover(dropDownMark);
            return state;
        }
        public static void GUIBoolean(ref float posY, ref bool value, string name, float width, Vector2 outFromTo, float sizeY = 32)
        {
            if(
                posY            < outFromTo.y &&
                outFromTo.x     < posY + sizeY
            )
            {
                Widgets.Label(new Rect(0,posY,width*0.5f,sizeY),name);
                Widgets.Checkbox(width - sizeY, posY, ref value, sizeY);
            }
            posY+=sizeY;
        }
        public static void GUIString(ref float posY, ref string value, string name, float width, Vector2 outFromTo, float sizeY = 32)
        {
            if(
                posY            < outFromTo.y &&
                outFromTo.x     < posY + sizeY
            )
            {
                Widgets.Label(new Rect(0,posY,width*0.5f,sizeY),name);
                value = Widgets.TextField(new Rect(width*0.5f,              posY,width*0.5f,sizeY),value);
            }
            posY+=sizeY;
        }
        public static void GUIFloat(ref float posY, ref float value, string name, float width, Vector2 outFromTo, int decimalPlaces = 0, float sizeY = 32)
        {
            if(
                posY            < outFromTo.y &&
                outFromTo.x     < posY + sizeY
            )
            {
                decimalPlaces = Math.Max(decimalPlaces,0);
                Widgets.Label(new Rect(0,posY,width*0.5f,sizeY),name);
                float.TryParse(Widgets.TextField(new Rect(width*0.5f,       posY,width*0.5f,sizeY),value.ToString("f"+decimalPlaces)),out value);
            }
            posY+=sizeY;
        }
        public static void GUIVec2(ref float posY, ref Vector2 value, string name, float width, Vector2 outFromTo, int decimalPlaces = 0, float sizeY = 32)
        {
            if(
                posY            < outFromTo.y &&
                outFromTo.x     < posY + sizeY
            )
            {
                decimalPlaces = Math.Max(decimalPlaces,0);
                float newValue;
                Widgets.Label(new Rect(0,posY,width*0.5f,sizeY),name);
                float.TryParse(Widgets.TextField(new Rect(width*0.5f,       posY,width*0.5f/2f,sizeY),value.x.ToString("f"+decimalPlaces)),out newValue);
                value.x = newValue;
                float.TryParse(Widgets.TextField(new Rect(width*0.5f*3f/2f, posY,width*0.5f/2f,sizeY),value.y.ToString("f"+decimalPlaces)),out newValue);
                value.y = newValue;
            }
            posY+=sizeY;
        }
        public static void GUIVec3(ref float posY, ref Vector3 value, string name, float width, Vector2 outFromTo, int decimalPlaces = 0, float sizeY = 32)
        {
            if(
                posY            < outFromTo.y &&
                outFromTo.x     < posY + sizeY
            )
            {
                decimalPlaces = Math.Max(decimalPlaces,0);
                float newValue;
                Widgets.Label(new Rect(0,posY,width*0.5f,sizeY),name);
                float.TryParse(Widgets.TextField(new Rect(width*0.5f,       posY,width*0.5f/3f,sizeY),value.x.ToString("f"+decimalPlaces)),out newValue);
                value.x = newValue;
                float.TryParse(Widgets.TextField(new Rect(width*0.5f*4f/3f, posY,width*0.5f/3f,sizeY),value.y.ToString("f"+decimalPlaces)),out newValue);
                value.y = newValue;
                float.TryParse(Widgets.TextField(new Rect(width*0.5f*5f/3f, posY,width*0.5f/3f,sizeY),value.z.ToString("f"+decimalPlaces)),out newValue);
                value.z = newValue;
            }
            posY+=sizeY;
        }
        public static void GUIVec4(ref float posY, ref Vector4 value, string name, float width, Vector2 outFromTo, int decimalPlaces = 0, float sizeY = 32)
        {
            if(
                posY            < outFromTo.y &&
                outFromTo.x     < posY + sizeY
            )
            {
                decimalPlaces = Math.Max(decimalPlaces,0);
                float newValue;
                Widgets.Label(new Rect(0,posY,width*0.5f,sizeY),name);
                float.TryParse(Widgets.TextField(new Rect(width*0.5f,       posY,width*0.5f/4f,sizeY),value.x.ToString("f"+decimalPlaces)),out newValue);
                value.x = newValue;
                float.TryParse(Widgets.TextField(new Rect(width*0.5f*5f/4f, posY,width*0.5f/4f,sizeY),value.y.ToString("f"+decimalPlaces)),out newValue);
                value.y = newValue;
                float.TryParse(Widgets.TextField(new Rect(width*0.5f*6f/4f, posY,width*0.5f/4f,sizeY),value.z.ToString("f"+decimalPlaces)),out newValue);
                value.z = newValue;
                float.TryParse(Widgets.TextField(new Rect(width*0.5f*7f/4f, posY,width*0.5f/4f,sizeY),value.w.ToString("f"+decimalPlaces)),out newValue);
                value.w = newValue;
            }
            posY+=sizeY;
        }
        public static void GUIEnum<T>(ref float posY, T value, string name, float width, Vector2 outFromTo, Action<T> setter, float sizeY = 32) where T : struct, Enum
        {
            if(
                posY            < outFromTo.y &&
                outFromTo.x     < posY + sizeY
            )
            {
                Widgets.Label(new Rect(0,posY,width*0.5f,sizeY),name);
                if(Widgets.ButtonText(new Rect(width*0.5f,posY,width*0.5f,sizeY), value.ToString()))
                {
                    Array all = Enum.GetValues(typeof(T));
                    List<FloatMenuOption> options = new List<FloatMenuOption>(all.Length);
                    foreach(T val in all)
                    {
                        options.Add(new FloatMenuOption(val.ToString(),delegate()
                        {
                            if(setter != null) setter(val);
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            }
            posY+=sizeY;
        }
    }
}