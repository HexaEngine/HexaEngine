namespace HexaEngine.Graphics.Culling
{
    using System.Numerics;

    public struct OcclusionParams
    {
        public uint NumberOfInstances;
        public uint NumberOfPropTypes;
        public int ActivateCulling;
        public uint MaxMipLevel;
        public Vector2 RTSize;
        public float P00;
        public float P11;
    }
}