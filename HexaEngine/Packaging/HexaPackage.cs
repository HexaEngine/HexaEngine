namespace HexaEngine.Packaging
{
    public class PackageHeader
    {
        public PackageMetadata Metadata { get; set; }

        public List<string> Dependencies { get; set; }

        public List<byte[]> Certificates { get; set; }

        public string Hash { get; set; }
    }

    public class HexaPackage
    {
        public PackageHeader Header { get; set; }
    }
}