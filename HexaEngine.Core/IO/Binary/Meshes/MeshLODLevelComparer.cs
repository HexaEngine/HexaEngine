namespace HexaEngine.Core.IO.Binary.Meshes
{
    using System.Collections.Generic;

    public class MeshLODLevelComparer : IComparer<MeshLODData>
    {
        public static readonly MeshLODLevelComparer Instance = new();

        public int Compare(MeshLODData? x, MeshLODData? y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            return x.LODLevel.CompareTo(y.LODLevel);
        }
    }
}