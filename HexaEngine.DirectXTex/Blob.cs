namespace HexaEngine.DirectXTex
{
    public unsafe struct TexBlob
    {
        public readonly void* pBlob;

        public TexBlob()
        {
            pBlob = Native.NewBlob();
        }

        public TexBlob(void* blob)
        {
            pBlob = blob;
        }

        public void Initialize(ulong size)
        {
            Native.BlobInitialize(pBlob, size);
        }

        public void Release()
        {
            fixed (void** p = &pBlob)
            {
                Native.BlobRelease(p);
            }
        }

        public void* GetBufferPointer()
        {
            return Native.BlobGetBufferPointer(pBlob);
        }

        public ulong GetBufferSize()
        {
            return Native.BlobGetBufferSize(pBlob);
        }

        public void Resize(ulong size)
        {
            Native.BlobResize(pBlob, size);
        }

        public void Trim(ulong size)
        {
            Native.BlobTrim(pBlob, size);
        }
    }
}