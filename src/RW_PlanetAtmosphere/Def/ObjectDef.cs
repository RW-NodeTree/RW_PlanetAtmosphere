using UnityEngine;
using Verse;

namespace RW_PlanetAtmosphere
{
    public abstract class ObjectDef : Def
    {
        public abstract TransparentObject TransparentObject { get; }
    }

}