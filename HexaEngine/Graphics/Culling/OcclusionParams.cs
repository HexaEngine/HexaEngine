﻿namespace HexaEngine.Graphics.Culling
{
    using System.Numerics;

    public struct OcclusionParams
    {
        public uint NumberOfInstances;
        public uint NumberOfPropTypes;
        public int FrustumCulling;
        public int OcclusionCulling;

        public uint MaxMipLevel;
        public float P00;
        public float P11;
        public float DepthBias;

        public Vector4 Frustum;
    }
}