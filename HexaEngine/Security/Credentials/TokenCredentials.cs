namespace HexaEngine.Security.Credentials
{
    using HexaEngine.Core;
    using HexaEngine.Security;
    using System.Security;

    public class TokenCredentials : DisposableBase, ICredentials
    {
        public SecureString Token { get; set; } = null!;

        public CredentialType Type => CredentialType.Token;

        public int Read(Span<byte> src)
        {
            int idx = 0;
            idx += src.ReadSecureString(out var token);
            Token = token;
            return idx;
        }

        public int Write(Span<byte> dest)
        {
            int idx = 0;
            idx += Token.WriteSecureString(dest);
            return idx;
        }

        public int SizeOf()
        {
            return Token.SizeOfSecureString();
        }

        protected override void DisposeCore()
        {
            Token?.Dispose();
        }
    }
}