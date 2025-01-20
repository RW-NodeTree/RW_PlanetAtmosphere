using UnityEngine;
using System;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;

namespace RW_PlanetAtmosphere
{
    public abstract class RendererDef : ScriptableObject
    {
        public float refraction     = 1;
        public float luminescen     = 0;
        public abstract TransparentObject TransparentObject { get; }
    }

}