namespace HexaEngine.Meshes
{
    using System;

    public interface IBinarySerializable
    {
        public int SizeOf();

        public int Write(Span<byte> dest);

        public int Read(Span<byte> src);
    }
}