namespace HexaEngine.Security.Credentials
{
    using HexaEngine.Core.IO;
    using HexaEngine.Security;
    using HashAlgorithm = HashAlgorithm;

    public struct CredentialsContainerHeader
    {
        public static readonly Version MinVersion = 1;
        public static readonly Version Version = 1;

        public static readonly byte[] MagicNumber = [0x54, 0x72, 0x61, 0x6E, 0x73, 0x43, 0x72, 0x65, 0x64, 0x73, 0x00];

        public Cipher Cipher;
        public HashAlgorithm Hash;
        public PasswordHashAlgorithm PasswordHash;
        public int RecordsCount;
        public int RecordTableLength;
        public int DataOffset;
        public int DataLength;

        public readonly int Write(Span<byte> dest)
        {
            int idx = MagicNumber.Length;
            MagicNumber.CopyTo(dest);
            idx += dest[idx..].WriteUInt64(Version);
            idx += dest[idx..].WriteInt32((int)Cipher);
            idx += dest[idx..].WriteInt32((int)Hash);
            idx += dest[idx..].WriteInt32((int)PasswordHash);
            idx += dest[idx..].WriteInt32(RecordsCount);
            idx += dest[idx..].WriteInt32(RecordTableLength);
            idx += dest[idx..].WriteInt32(DataOffset);
            idx += dest[idx..].WriteInt32(DataLength);
            return idx;
        }

        public int Read(ReadOnlySpan<byte> src)
        {
            int idx = MagicNumber.Length;
            if (!src[..MagicNumber.Length].SequenceEqual(MagicNumber))
            {
                throw new FormatException("Magic number mismatch");
            }

            idx += src[idx..].ReadUInt64(out var version);

            if (version < MinVersion || version > Version)
            {
                throw new FormatException($"Version mismatch, file: {version} min: {MinVersion} max: {Version}");
            }

            idx += src[idx..].ReadInt32(out var cipher);
            idx += src[idx..].ReadInt32(out var hash);
            idx += src[idx..].ReadInt32(out var passwordHash);
            Cipher = (Cipher)cipher;
            Hash = (HashAlgorithm)hash;
            PasswordHash = (PasswordHashAlgorithm)passwordHash;
            idx += src[idx..].ReadInt32(out RecordsCount);
            idx += src[idx..].ReadInt32(out RecordTableLength);
            idx += src[idx..].ReadInt32(out DataOffset);
            idx += src[idx..].ReadInt32(out DataLength);
            return idx;
        }

        public bool TryRead(ReadOnlySpan<byte> src, out int read)
        {
            int idx = MagicNumber.Length;
            if (!src[..MagicNumber.Length].SequenceEqual(MagicNumber))
            {
                read = idx;
                return false;
            }

            idx += src[idx..].ReadUInt64(out var version);

            if (version < MinVersion || version > Version)
            {
                read = idx;
                return false;
            }

            idx += src[idx..].ReadInt32(out var cipher);
            idx += src[idx..].ReadInt32(out var hash);
            idx += src[idx..].ReadInt32(out var passwordHash);
            Cipher = (Cipher)cipher;
            Hash = (HashAlgorithm)hash;
            PasswordHash = (PasswordHashAlgorithm)passwordHash;
            idx += src[idx..].ReadInt32(out RecordsCount);
            idx += src[idx..].ReadInt32(out RecordTableLength);
            idx += src[idx..].ReadInt32(out DataOffset);
            idx += src[idx..].ReadInt32(out DataLength);
            read = idx;
            return true;
        }

        public static int SizeOf()
        {
            return MagicNumber.Length + 8 + 4 + 4 + 4 + 4 + 4 + 4 + 4;
        }
    }
}