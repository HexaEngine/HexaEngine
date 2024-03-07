namespace HexaEngine.Physics
{
    public enum QueryFlags : ushort
    {
        Static = 1,
        Dynamic = 2,
        Prefilter = 4,
        Postfilter = 8,
        AnyHit = 0x10,
        NoBlock = 0x20,
        DisableHardcodedFilter = 0x40,
        Reserved = 0x8000
    }
}