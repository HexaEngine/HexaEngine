namespace HexaEngine.Audio.Common.Flac
{
    public unsafe struct Padding
    {
        public static void Read(BitReader br, MetadataBlockHeader header)
        {
            br.BytePosition += header.Length;
        }

        public static void Write(BitWriter br, MetadataBlockHeader header)
        {
            br.BytePosition += header.Length;
        }
    }
}