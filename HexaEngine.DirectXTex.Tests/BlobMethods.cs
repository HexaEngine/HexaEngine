namespace HexaEngine.DirectXTex.Tests
{
    public unsafe class BlobMethods
    {
        [Fact]
        public void CreateAndRelease()
        {
            TexBlob blob = new();
            blob.Release();
            if (blob.pBlob != null)
            {
                Assert.Fail("Mem leak");
            }
        }

        [Fact]
        public void Init()
        {
            TexBlob blob = new();
            blob.Initialize(256);
            Assert.Equal(256u, blob.GetBufferSize());
            blob.Release();
            if (blob.pBlob != null)
            {
                Assert.Fail("Mem leak");
            }
        }

        [Fact]
        public void GetBufferPointerAndBufferSizeAndWrite()
        {
            TexBlob blob = new();
            blob.Initialize(256);
            ulong size = blob.GetBufferSize();
            Assert.Equal(256u, size);
            void* pointer = blob.GetBufferPointer();
            Span<byte> bytes = new(pointer, (int)size);
            bytes.Fill(1);

            blob.Release();
            if (blob.pBlob != null)
            {
                Assert.Fail("Mem leak");
            }
        }

        [Fact]
        public void Resize()
        {
            TexBlob blob = new();
            blob.Initialize(256);
            ulong size = blob.GetBufferSize();
            Assert.Equal(256u, size);

            blob.Resize(1024);
            size = blob.GetBufferSize();
            Assert.Equal(1024u, size);

            blob.Release();
            if (blob.pBlob != null)
            {
                Assert.Fail("Mem leak");
            }
        }

        [Fact]
        public void Trim()
        {
            TexBlob blob = new();
            blob.Initialize(256);
            ulong size = blob.GetBufferSize();
            Assert.Equal(256u, size);

            blob.Trim(128);
            size = blob.GetBufferSize();
            Assert.Equal(128u, size);

            blob.Release();
            if (blob.pBlob != null)
            {
                Assert.Fail("Mem leak");
            }
        }
    }
}