namespace TestApp
{
    using HexaEngine;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Windows;
    using HexaEngine.Editor.Packaging;
    using HexaEngine.Editor.Projects;

    public static unsafe partial class Program
    {
        public static void Main()
        {
            PackageMetadata metadata = PackageMetadataParser.ParseFrom("packageMetadata.xml");
        }
    }
}