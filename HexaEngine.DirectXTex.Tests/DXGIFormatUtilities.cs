namespace HexaEngine.DirectXTex.Tests
{
    public unsafe class DXGIFormatUtilities
    {
        [Fact]
        public void IsValid()
        {
            if (!DirectXTex.IsValid(Silk.NET.DXGI.Format.FormatR16G16B16A16Uint))
                Trace.Fail("Should be valid");
            if (DirectXTex.IsValid((Silk.NET.DXGI.Format)(-1561658)))
                Trace.Fail("Should be invalid");
        }

        [Fact]
        public void IsCompressed()
        {
            if (!DirectXTex.IsCompressed(Silk.NET.DXGI.Format.FormatBC7Unorm))
                Trace.Fail("Should be compressed");
            if (DirectXTex.IsCompressed(Silk.NET.DXGI.Format.FormatR16G16B16A16Uint))
                Trace.Fail("Shouldn't be compressed");
        }

        [Fact]
        public void IsPacked()
        {
            if (!DirectXTex.IsPacked(Silk.NET.DXGI.Format.FormatR8G8B8G8Unorm))
                Trace.Fail("Should be packed");
            if (DirectXTex.IsPacked(Silk.NET.DXGI.Format.FormatR16G16B16A16Uint))
                Trace.Fail("Shouldn't be packed");
        }

        [Fact]
        public void IsVideo()
        {
            if (!DirectXTex.IsVideo(Silk.NET.DXGI.Format.FormatNV12))
                Trace.Fail("Should be video");
            if (DirectXTex.IsVideo(Silk.NET.DXGI.Format.FormatR16G16B16A16Uint))
                Trace.Fail("Shouldn't be video");
        }

        [Fact]
        public void IsPlanar()
        {
            if (!DirectXTex.IsPlanar(Silk.NET.DXGI.Format.FormatP010))
                Trace.Fail("Should be planar");
            if (DirectXTex.IsPlanar(Silk.NET.DXGI.Format.FormatR16G16B16A16Uint))
                Trace.Fail("Shouldn't be planar");
        }

        [Fact]
        public void IsPalettized()
        {
            if (!DirectXTex.IsPalettized(Silk.NET.DXGI.Format.FormatAI44))
                Trace.Fail("Should be palettized");
            if (DirectXTex.IsPalettized(Silk.NET.DXGI.Format.FormatR16G16B16A16Uint))
                Trace.Fail("Shouldn't be palettized");
        }

        [Fact]
        public void IsDepthStencil()
        {
            if (!DirectXTex.IsDepthStencil(Silk.NET.DXGI.Format.FormatD24UnormS8Uint))
                Trace.Fail("Should be depthStencil");
            if (DirectXTex.IsDepthStencil(Silk.NET.DXGI.Format.FormatR16G16B16A16Uint))
                Trace.Fail("Shouldn't be depthStencil");
        }

        [Fact]
        public void IsSRGB()
        {
            if (!DirectXTex.IsSRGB(Silk.NET.DXGI.Format.FormatB8G8R8A8UnormSrgb))
                Trace.Fail("Should be SRGB");
            if (DirectXTex.IsSRGB(Silk.NET.DXGI.Format.FormatR16G16B16A16Uint))
                Trace.Fail("Shouldn't be SRGB");
        }

        [Fact]
        public void IsTypeless()
        {
            if (!DirectXTex.IsTypeless(Silk.NET.DXGI.Format.FormatB8G8R8A8Typeless))
                Trace.Fail("Should be partial Typeless");
            if (!DirectXTex.IsTypeless(Silk.NET.DXGI.Format.FormatR32FloatX8X24Typeless))
                Trace.Fail("Should be partial Typeless");
            if (DirectXTex.IsTypeless(Silk.NET.DXGI.Format.FormatBC1Unorm))
                Trace.Fail("Shouldn't be partial Typeless");

            if (!DirectXTex.IsTypeless(Silk.NET.DXGI.Format.FormatB8G8R8A8Typeless, false))
                Trace.Fail("Should be Typeless");
            if (DirectXTex.IsTypeless(Silk.NET.DXGI.Format.FormatX32TypelessG8X24Uint, false))
                Trace.Fail("Shouldn't be Typeless");
        }

        [Fact]
        public void HasAlpha()
        {
            if (!DirectXTex.HasAlpha(Silk.NET.DXGI.Format.FormatB8G8R8A8UnormSrgb))
                Trace.Fail("Should have alpha");
            if (DirectXTex.HasAlpha(Silk.NET.DXGI.Format.FormatG8R8G8B8Unorm))
                Trace.Fail("Shouldn't have alpha");
        }
    }
}