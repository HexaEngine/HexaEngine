namespace HexaEngine.Editor.Packaging
{
    using System;

    public class PackageData
    {
        public Version TargetVersion { get; set; }

        public FileTree FileTree { get; set; }

        public byte[] Data { get; set; }
    }
}