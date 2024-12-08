namespace HexaEngine.Editor.Packaging
{
    using System;

    public class PackageData
    {
        public Version TargetVersion { get; set; } = null!;

        public PackageFileTree FileTree { get; set; } = null!;

        public byte[] Data { get; set; } = null!;
    }
}