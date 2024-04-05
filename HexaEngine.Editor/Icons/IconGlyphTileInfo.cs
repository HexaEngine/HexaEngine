namespace HexaEngine.Editor.Icons
{
    using HexaEngine.Core.Security.Cryptography;
    using HexaEngine.Mathematics;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    public struct IconGlyphTileInfo
    {
        public Guid Key;
        public Point2 Pos;
        public Point2 Size;

        public IconGlyphTileInfo(string name, Point2 pos, Point2 size)
        {
            Key = new(MD5.HashData(MemoryMarshal.AsBytes(name.AsSpan())));
            Pos = pos;
            Size = size;
        }

        public IconGlyphTileInfo(Guid key, Point2 pos, Point2 size)
        {
            Key = key;
            Pos = pos;
            Size = size;
        }
    }
}