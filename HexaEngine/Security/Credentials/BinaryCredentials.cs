namespace HexaEngine.Security.Credentials
{
    using HexaEngine.Core;
    using HexaEngine.Core.IO;

    public class BinaryCredentials : DisposableBase, ICredentials
    {
        public byte[] Data { get; set; }

        public CredentialType Type => CredentialType.Binary;

        public int Read(Span<byte> src)
        {
            int idx = 0;
            idx += src.ReadInt32(out var length);
            Data = src.Slice(idx, length).ToArray();
            return idx + length;
        }

        public int Write(Span<byte> dest)
        {
            int idx = 0;
            idx += dest.WriteInt32(Data.Length);
            Data.CopyTo(dest[idx..]);
            return idx + Data.Length;
        }

        public int SizeOf()
        {
            return 4 + Data.Length;
        }

        protected override void DisposeCore()
        {
            Array.Fill<byte>(Data, 0);
        }
    }
}