using UnityEngine;
using System;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;

namespace RW_PlanetAtmosphere
{
    public abstract class Def : ScriptableObject
    {
        public abstract TransparentObject TransparentObject { get; }
    }

}