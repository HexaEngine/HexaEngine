namespace HexaEngine.Editor.Packaging
{
    using System;

    public class PackageData
    {
        public Version TargetVersion { get; set; }

        public PackageFileTree FileTree { get; set; }

        public byte[] Data { get; set; }
    }
}