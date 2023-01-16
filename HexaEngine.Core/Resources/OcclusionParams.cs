namespace HexaEngine.Core.Resources
{
    using System.Numerics;

    public struct OcclusionParams
    {
        public uint NoofInstances;
        public uint NoofPropTypes;
        public int ActivateCulling;
        public uint MaxMipLevel;
        public Vector2 RTSize;
        public float P00;
        public float P11;
    }
}