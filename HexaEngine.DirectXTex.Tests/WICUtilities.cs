namespace HexaEngine.DirectXTex.Tests
{
    public unsafe class WICUtilities
    {
        [Fact]
        public void GetWICCodec()
        {
            var ptr = DirectXTex.GetWICCodec(WICCodecs.PNG);
            if (ptr == null)
            {
                Assert.Fail("Ptr is null");
            }
        }

        [Fact]
        public void GetSetWICFactory()
        {
            bool isWIC2;
            var factory = DirectXTex.GetWICFactory(&isWIC2);
            DirectXTex.SetWICFactory(factory);
        }
    }
}