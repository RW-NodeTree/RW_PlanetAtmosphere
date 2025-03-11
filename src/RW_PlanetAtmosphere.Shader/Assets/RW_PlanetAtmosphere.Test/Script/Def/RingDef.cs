using UnityEngine;
using System;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;

namespace RW_PlanetAtmosphere
{
    [CreateAssetMenu(fileName = "RingDef", menuName = "Scriptable Object/RingDef", order = 2)]
    public class RingDef : RendererDef
    {
        public bool renderingShadow = true;
        public float refraction     = 1;
        public float luminescen     = 0;
        public Vector2 ringFromTo   = new Vector2(100, 150);
        public Vector3 normal       = Vector3.up;
        public Vector3 postion      = Vector3.zero;
        public string ringMapPath   = null;

        public override TransparentObject TransparentObject => new TransparentObject_Ring(this);
    }

}