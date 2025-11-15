namespace HexaEngine.D3D11
{
    using HexaEngine.Core.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public unsafe class IncludeHandler
    {
        private readonly Stack<string> paths = new();
        private string basePath;
        private readonly string systemInclude;

        public IncludeHandler(string basepath, string systemInclude)
        {
            basePath = basepath;
            this.systemInclude = systemInclude;
        }

        public unsafe int Open(ID3DInclude* pInclude, IncludeType includeType, byte* pFileName, void* pParentData, void** ppData, uint* pBytes)
        {
            string fileName = Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(pFileName));

            AssetPath assetPath = new(fileName);

            if (!assetPath.HasNamespace)
            {
                // Count ../ at the beginning
                int upCount = 0;
                int fileNameStart = 0;
                while (fileNameStart + 2 < fileName.Length && fileName[fileNameStart] == '.' && fileName[fileNameStart + 1] == '.' && (fileName[fileNameStart + 2] == '/' || fileName[fileNameStart + 2] == '\\'))
                {
                    upCount++;
                    fileNameStart += 3;
                }

                // Go back upCount directories from basePath
                int basePathEnd = basePath.Length;
                for (int j = 0; j < upCount; j++)
                {
                    if (basePathEnd > 0 && (basePath[basePathEnd - 1] == '/' || basePath[basePathEnd - 1] == '\\'))
                        basePathEnd--;
                    while (basePathEnd > 0 && basePath[basePathEnd - 1] != '/' && basePath[basePathEnd - 1] != '\\')
                        basePathEnd--;
                }

                var baseSpan = basePath.AsSpan(0, basePathEnd);
                if (baseSpan.EndsWith('/') || baseSpan.EndsWith('\\'))
                {
                    baseSpan = baseSpan[..^1];
                }
                string absPath = $"{baseSpan}/{fileName.AsSpan(fileNameStart)}";
                assetPath = new(absPath);
            }

            byte[] data;
            try
            {
                data = FileSystem.ReadAllBytes(assetPath);
            }
            catch (FileNotFoundException)
            {
                data = Encoding.UTF8.GetBytes($"#error File '{assetPath.Raw}' not found");
            }

            paths.Push(basePath);
            var dirName = Path.GetDirectoryName(assetPath.Raw);
            basePath = dirName.ToString();

            *ppData = AllocCopyT(data);
            *pBytes = (uint)data.Length;
            return 0;
        }

        public unsafe int Close(ID3DInclude* pInclude, void* pData)
        {
            basePath = paths.Pop();
            Free(pData);
            return 0;
        }
    }
}