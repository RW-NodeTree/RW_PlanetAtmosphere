using UnityEngine;
using Verse;

namespace RW_PlanetAtmosphere
{
    [CreateAssetMenu(fileName = "CloudDef", menuName = "Scriptable Object/CloudDef", order = 1)]
    public class CloudDef : ObjectDef
    {
        public bool renderingShadow     = true;
        public float exposure           = 2;
        public float refraction         = 1;
        public float luminescen         = 0;
        public float radius             = 63.76393f * AtmosphereSettings.scale;
        public Vector3 normal           = Vector3.up;
        public Vector3 tangent          = Vector3.right;
        public Vector3 postion          = Vector3.zero;
        public string cloudTexturePath  = "EarthCloudTex/8k_earth_clouds";
        //public string noiseTexturePath  = null;

        public override TransparentObject TransparentObject => new TransparentObject_Cloud(this);
    }

}