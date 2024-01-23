namespace HexaEngine.D3D11
{
    using System;

    [Flags]
    public enum RegisterComponentMaskFlags : byte
    {
        None = 0,
        ComponentX = 1,
        ComponentY = 2,
        ComponentZ = 4,
        ComponentW = 8,
        All
    }
}