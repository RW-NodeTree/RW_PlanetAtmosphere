using UnityEngine;

namespace RW_PlanetAtmosphere
{
#if UNITY
    [CreateAssetMenu(fileName = "RingDef", menuName = "Scriptable Object/RingDef", order = 3)]
#endif
    public class RingDef : ObjectDef
    {
        public bool renderingShadow     = true;
        public bool hideWhenGetClose    = true;
        public float refraction         = 1;
        public float luminescen         = 0;
        public float opacity            = 1;
        public float opacityType        = 0;
        public Vector2 ringFromTo       = new Vector2(100, 150) * PlanetAtmosphereRenderer.scale;
        public Vector3 normal           = Vector3.up;
        public Vector3 postion          = Vector3.zero;
        public string ringMapPath       = "Ring/2k_saturn_ring_alpha";

        public override TransparentObject TransparentObject => new TransparentObject_Ring(this);
    }

}