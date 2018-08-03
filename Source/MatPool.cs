using UnityEngine;
using Verse;

namespace Wizardry
{
    [StaticConstructorOnStartup]
    public static class MatPool
    {
        public static readonly Material rendEarthMat7 = MaterialPool.MatFrom("Meshes/EarthQuake7", true);
    }
}
