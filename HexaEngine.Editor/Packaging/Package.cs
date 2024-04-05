namespace HexaEngine.Editor.Packaging
{
    using System.Collections.Generic;

    public class Package
    {
        public const string PackageFormatVersion = "1.0.0.0";

        public PackageMetadata Metadata { get; set; }

        public List<PackageData> Data { get; set; }
    }
}