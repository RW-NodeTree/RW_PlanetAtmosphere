using UnityEngine;
using System;

namespace RW_PlanetAtmosphere
{
    public abstract class SkyNode // : IExposable
    {
        public abstract float CameraDepth(Vector3 cameraPos);
        public abstract float LightDepth(Vector3 lightDir);

        public abstract void UpdateNode();
        public abstract void ApplyShadow(SkyNode other);
        // public abstract void ExposeData();
    }

    // public class SkyNode_Cloud : SkyNode
    // {

    // }
}