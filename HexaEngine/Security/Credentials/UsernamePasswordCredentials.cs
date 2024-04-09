namespace HexaEngine.Security.Credentials
{
    using HexaEngine.Core;
    using HexaEngine.Security;
    using System.Security;

    public class UsernamePasswordCredentials : DisposableBase, ICredentials
    {
        public SecureString Username { get; set; }

        public SecureString Password { get; set; }

        public CredentialType Type => CredentialType.UsernamePassword;

        public int Read(Span<byte> src)
        {
            int idx = 0;
            idx += src.ReadSecureString(out var username);
            idx += src[idx..].ReadSecureString(out var password);
            Username = username;
            Password = password;
            return idx;
        }

        public int Write(Span<byte> dest)
        {
            int idx = 0;
            idx += Username.WriteSecureString(dest);
            idx += Password.WriteSecureString(dest[idx..]);
            return idx;
        }

        public int SizeOf()
        {
            return Username.SizeOfSecureString() + Password.SizeOfSecureString();
        }

        protected override void DisposeCore()
        {
            Username?.Dispose();
            Password?.Dispose();
        }
    }
}