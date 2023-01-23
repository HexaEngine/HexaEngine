namespace HexaEngine.Core.IO
{
    public struct ShaderBytecodeHeader
    {
        public static readonly byte[] MagicNumber = { 0x54, 0x72, 0x61, 0x6E, 0x73, 0x53, 0x68, 0x61, 0x64, 0x65, 0x72, 0x00 };
        public const int Version = 1;
        public uint BytecodeLength;
        public uint InputElementCount;
        public uint MacroCount;
        public uint SourceLength;
    }
}