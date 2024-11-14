namespace HexaEngine.D3D12
{
    using Hexa.NET.DXC;
    using HexaGen.Runtime;
    using System;

    public unsafe class IncludeHandler
    {
        private string basePath;

        public IncludeHandler(string basePath)
        {
            this.basePath = basePath;
        }

        public int QueryInterface(Guid* riid, void** ppvObject)
        {
            return (int)ResultCode.E_NOINTERFACE;
        }

        public uint AddRef()
        {
            return 0;
        }

        public uint Release()
        {
            return 0;
        }

        public HResult LoadSource(char* fileName, IDxcBlob** ppIncludeSource)
        {
            return 0;
        }
    }
}