using UnityEngine;
#if !UNITY
using Verse;
#endif
namespace RW_PlanetAtmosphere
{
#if UNITY
    public abstract class ObjectDef : ScriptableObject
#else
    public abstract class ObjectDef : Def
#endif
    {
        public abstract TransparentObject TransparentObject { get; }
    }

}