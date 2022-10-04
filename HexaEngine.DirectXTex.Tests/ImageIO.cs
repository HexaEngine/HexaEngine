namespace HexaEngine.DirectXTex.Tests
{
    public unsafe class ImageIO
    {
        private static byte[] LoadTexture() => File.ReadAllBytes("assets/textures/test.dds");

        [Fact]
        public void LoadAndSaveFromDDSMemory()
        {
            ScratchImage image = new();
            Span<byte> src = LoadTexture();
            TexBlob blob;
            DirectXTex.LoadFromDDSMemory(src, DDSFlags.NONE, &image);
            TexMetadata meta = image.GetMetadata();

            DirectXTex.SaveToDDSFile(&image, DDSFlags.NONE, "test.dds");


            DirectXTex.SaveToDDSMemory(&image, DDSFlags.NONE, &blob);

            ulong size = blob.GetBufferSize();
            void* pointer = blob.GetBufferPointer();
            Span<byte> bytes = new(pointer, (int)size);

            File.WriteAllBytes("test1.dds", bytes.ToArray());


            //Assert.True(src.SequenceEqual(dest));
            blob.Release();
            image.Release();
            if (image.pScratchImage != null)
                Trace.Fail("Mem leak");
        }
    }
}
