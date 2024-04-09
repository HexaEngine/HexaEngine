namespace HexaEngine.Security.Credentials
{
    using HexaEngine.Core;

    public class UnknownCredentials : DisposableBase, ICredentials
    {
        public UnknownCredentials(CredentialType type)
        {
            Type = type;
        }

        public byte[] Data { get; set; }

        public CredentialType Type { get; }

        public int Read(Span<byte> src)
        {
            Data = src.ToArray();
            return src.Length;
        }

        public int Write(Span<byte> dest)
        {
            Data.CopyTo(dest);
            return Data.Length;
        }

        public int SizeOf()
        {
            return Data.Length;
        }

        protected override void DisposeCore()
        {
            Array.Clear(Data);
        }
    }
}