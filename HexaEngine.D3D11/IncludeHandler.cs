﻿namespace HexaEngine.D3D11
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
                assetPath = new(Path.Combine(basePath, fileName));
            }
           
            var data = FileSystem.ReadAllBytes(assetPath);

            paths.Push(basePath);
            var dirName = Path.GetDirectoryName(assetPath.Path);
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