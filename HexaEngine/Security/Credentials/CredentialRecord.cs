namespace HexaEngine.Security.Credentials
{
    using HexaEngine.Core.IO;
    using System.Text;

    public struct CredentialRecord(string name, CredentialType type, int offset, int size)
    {
        public string Name = name;
        public CredentialType Type = type;
        public int Offset = offset;
        public int Size = size;

        public readonly int Write(Span<byte> dest)
        {
            int idx = 0;
            idx += dest.WriteString(Name, Encoding.UTF8);
            idx += dest[idx..].WriteInt32((int)Type);
            idx += dest[idx..].WriteInt32(Offset);
            idx += dest[idx..].WriteInt32(Size);
            return idx;
        }

        public int Read(ReadOnlySpan<byte> src)
        {
            int idx = 0;
            idx += src.ReadString(Encoding.UTF8, out Name);
            idx += src[idx..].ReadInt32(out var type);
            Type = (CredentialType)type;
            idx += src[idx..].ReadInt32(out Offset);
            idx += src[idx..].ReadInt32(out Size);
            return idx;
        }

        public readonly int SizeOf()
        {
            return 4 + Encoding.UTF8.GetByteCount(Name) + 4 + 4 + 4;
        }
    }
}