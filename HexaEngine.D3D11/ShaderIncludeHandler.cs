namespace HexaEngine.D3D11
{
    using HexaEngine.Core.IO;
    using SharpGen.Runtime;
    using Vortice.Direct3D;

    public class ShaderIncludeHandler : CallbackBase, Include
    {
        public string TargetPath { get; }

        public ShaderIncludeHandler(string targetPath)
        {
            TargetPath = targetPath;
        }

        public Stream Open(IncludeType type, string fileName, Stream? parentStream)
        {
            var includeFile = GetFilePath(fileName);

            if (!FileSystem.Exists(includeFile))
                throw new FileNotFoundException($"Include file '{fileName}' not found.");

            var includeStream = FileSystem.Open(includeFile);

            return includeStream;
        }

        private string GetFilePath(string fileName)
        {
            var path = Path.Combine(Path.GetDirectoryName(TargetPath) ?? string.Empty, fileName);
            return path;
        }

        public void Close(Stream stream)
        {
            stream.Dispose();
        }
    }
}