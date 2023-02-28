﻿namespace HexaEngine.Core.Graphics.Structs
{
    using System.Numerics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct FogDescription
    {
        public float FogStart;
        public float FogEnd;
        public Vector2 reserved;
    }
}