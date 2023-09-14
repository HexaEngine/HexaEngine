namespace HexaEngine.Meshes
{
    using System.Numerics;

    public struct CBVoxel
    {
        public Vector3 GridCenter;
        public float DataSize;        // voxel half-extent in world space units
        public float DataSizeRCP;    // 1.0 / voxel-half extent
        public uint DataRes;         // voxel grid resolution
        public float DataResRCP;     // 1.0 / voxel grid resolution
        public uint NumCones;
        public float NumConesRCP;
        public float MaxDistance;
        public float RayStepSize;
        public uint Mips;
    };
}