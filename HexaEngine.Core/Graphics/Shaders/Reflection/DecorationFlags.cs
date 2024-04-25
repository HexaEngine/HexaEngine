namespace HexaEngine.Core.Graphics.Shaders.Reflection
{
    [Flags]
    public enum DecorationFlags
    {
        None = 0,
        Block = 1,
        BufferBlock = 2,
        RowMajor = 4,
        ColumnMajor = 8,
        BuiltIn = 0x10,
        NoPerspective = 0x20,
        Flat = 0x40,
        NonWritable = 0x80,
        RelaxedPrecision = 0x100,
        NonReadable = 0x200,
        Patch = 0x400,
        PerVertex = 0x800,
        PerTask = 0x1000,
        WeightTexture = 0x2000,
        BlockMatchTexture = 0x4000
    }
}