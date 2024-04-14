namespace HexaEngine.Physics
{
    [Flags]
    public enum HitFlags : ushort
    {
        Position = 1,
        Normal = 2,
        Uv = 8,
        AssumeNoInitialOverlap = 0x10,
        AnyHit = 0x20,
        MeshMultiple = 0x40,
        MeshBothSides = 0x80,
        PreciseSweep = 0x100,
        Mtd = 0x200,
        FaceIndex = 0x400,
        Default = 0x403,
        ModifiableFlags = 0x1D0
    }
}