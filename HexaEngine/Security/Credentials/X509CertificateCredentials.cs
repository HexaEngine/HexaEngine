namespace HexaEngine.Security.Credentials
{
    using HexaEngine.Core;
    using HexaEngine.Core.IO;
    using System.Security.Cryptography.X509Certificates;

    public class X509CertificateCredentials : DisposableBase, ICredentials
    {
        public X509Certificate Certificate { get; set; } = null!;

        public CredentialType Type => CredentialType.X509Certificate;

        public int Read(Span<byte> src)
        {
            src.ReadInt32(out int length);

            Certificate = X509CertificateLoader.LoadCertificate(src.Slice(4, length).ToArray());
            return 4 + length;
        }

        public int Write(Span<byte> dest)
        {
            var data = Certificate.GetRawCertData();
            dest.WriteInt32(data.Length);
            data.CopyTo(dest[4..]);
            return 4 + data.Length;
        }

        public int SizeOf()
        {
            return 4 + Certificate.GetRawCertData().Length;
        }

        protected override void DisposeCore()
        {
            Certificate.Dispose();
        }
    }
}