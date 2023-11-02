namespace HexaEngine.Security.Credentials
{
    public interface ICredentials : IDisposable
    {
        CredentialType Type { get; }

        public int Write(Span<byte> dest);

        public int Read(Span<byte> src);

        public int SizeOf();
    }
}