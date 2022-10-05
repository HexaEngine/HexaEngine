namespace HexaEngine.DirectXTex.Tests
{
    using Silk.NET.DXGI;

    public unsafe class DXGIFormatUtilities
    {
        [Fact]
        public void IsValid()
        {
            if (!DirectXTex.IsValid(Format.FormatR16G16B16A16Uint))
                Trace.Fail("Should be valid");
            if (DirectXTex.IsValid((Silk.NET.DXGI.Format)(-1561658)))
                Trace.Fail("Should be invalid");
        }

        [Fact]
        public void IsCompressed()
        {
            if (!DirectXTex.IsCompressed(Format.FormatBC7Unorm))
                Trace.Fail("Should be compressed");
            if (DirectXTex.IsCompressed(Format.FormatR16G16B16A16Uint))
                Trace.Fail("Shouldn't be compressed");
        }

        [Fact]
        public void IsPacked()
        {
            if (!DirectXTex.IsPacked(Format.FormatR8G8B8G8Unorm))
                Trace.Fail("Should be packed");
            if (DirectXTex.IsPacked(Format.FormatR16G16B16A16Uint))
                Trace.Fail("Shouldn't be packed");
        }

        [Fact]
        public void IsVideo()
        {
            if (!DirectXTex.IsVideo(Format.FormatNV12))
                Trace.Fail("Should be video");
            if (DirectXTex.IsVideo(Format.FormatR16G16B16A16Uint))
                Trace.Fail("Shouldn't be video");
        }

        [Fact]
        public void IsPlanar()
        {
            if (!DirectXTex.IsPlanar(Format.FormatP010))
                Trace.Fail("Should be planar");
            if (DirectXTex.IsPlanar(Format.FormatR16G16B16A16Uint))
                Trace.Fail("Shouldn't be planar");
        }

        [Fact]
        public void IsPalettized()
        {
            if (!DirectXTex.IsPalettized(Format.FormatAI44))
                Trace.Fail("Should be palettized");
            if (DirectXTex.IsPalettized(Format.FormatR16G16B16A16Uint))
                Trace.Fail("Shouldn't be palettized");
        }

        [Fact]
        public void IsDepthStencil()
        {
            if (!DirectXTex.IsDepthStencil(Format.FormatD24UnormS8Uint))
                Trace.Fail("Should be depthStencil");
            if (DirectXTex.IsDepthStencil(Format.FormatR16G16B16A16Uint))
                Trace.Fail("Shouldn't be depthStencil");
        }

        [Fact]
        public void IsSRGB()
        {
            if (!DirectXTex.IsSRGB(Format.FormatB8G8R8A8UnormSrgb))
                Trace.Fail("Should be SRGB");
            if (DirectXTex.IsSRGB(Format.FormatR16G16B16A16Uint))
                Trace.Fail("Shouldn't be SRGB");
        }

        [Fact]
        public void IsTypeless()
        {
            if (!DirectXTex.IsTypeless(Format.FormatB8G8R8A8Typeless))
                Trace.Fail("Should be partial Typeless");
            if (!DirectXTex.IsTypeless(Format.FormatR32FloatX8X24Typeless))
                Trace.Fail("Should be partial Typeless");
            if (DirectXTex.IsTypeless(Format.FormatBC1Unorm))
                Trace.Fail("Shouldn't be partial Typeless");

            if (!DirectXTex.IsTypeless(Format.FormatB8G8R8A8Typeless, false))
                Trace.Fail("Should be Typeless");
            if (DirectXTex.IsTypeless(Format.FormatX32TypelessG8X24Uint, false))
                Trace.Fail("Shouldn't be Typeless");
        }

        [Fact]
        public void HasAlpha()
        {
            if (!DirectXTex.HasAlpha(Format.FormatB8G8R8A8UnormSrgb))
                Trace.Fail("Should have alpha");
            if (DirectXTex.HasAlpha(Format.FormatG8R8G8B8Unorm))
                Trace.Fail("Shouldn't have alpha");
        }

        [Fact]
        public void BitsPerPixel()
        {
            if (DirectXTex.BitsPerPixel(Format.FormatR8G8B8A8Uint) != 32)
                Trace.Fail("Should have 32Bits");
            if (DirectXTex.BitsPerPixel(Format.FormatR32G32B32A32Uint) == 32)
                Trace.Fail("Shouldn't have 32Bits");
        }

        [Fact]
        public void BitsPerColor()
        {
            if (DirectXTex.BitsPerColor(Format.FormatR8G8B8A8Uint) != 8)
                Trace.Fail("Should have 8Bits per color");
            if (DirectXTex.BitsPerColor(Format.FormatR32G32B32A32Uint) == 8)
                Trace.Fail("Shouldn't have 8Bits per color");
        }

        [Fact]
        public void FormatDataType()
        {
            if (DirectXTex.FormatDataType(Format.FormatA8Unorm) != FormatType.UNorm)
                Trace.Fail("Should be unorm");
            if (DirectXTex.FormatDataType(Format.FormatR32G32B32A32Uint) == FormatType.Float)
                Trace.Fail("Shouldn't be float");
        }

        [Fact]
        public void ComputePitch()
        {
            uint width = 64;
            uint height = 64;
            ulong rowPitch = 0;
            ulong slicePitch = 0;
            HResult result = DirectXTex.ComputePitch(Format.FormatR8G8B8A8Uint, width, height, &rowPitch, &slicePitch, CPFlags.None);
            if (!result.IsSuccess)
                result.Throw();

            ulong rowPitch2 = width * 4;
            ulong slicePitch2 = rowPitch2 * height;

            Assert.Equal(rowPitch2, rowPitch);
            Assert.Equal(slicePitch2, slicePitch);
        }

        [Fact]
        public void ComputeScanlines()
        {
            uint height = 64;
            var result = DirectXTex.ComputeScanlines(Format.FormatR8G8B8A8Uint, height);
            ulong expected = 64;
            Assert.Equal(result, expected);
        }

        [Fact]
        public void MakeSRGB()
        {
            var result = DirectXTex.MakeSRGB(Format.FormatBC1Unorm);
            Assert.Equal(Format.FormatBC1UnormSrgb, result);
        }

        [Fact]
        public void MakeTypeless()
        {
            var result = DirectXTex.MakeTypeless(Format.FormatBC1Unorm);
            Assert.Equal(Format.FormatBC1Typeless, result);
        }

        [Fact]
        public void MakeTypelessUNORM()
        {
            var result = DirectXTex.MakeTypelessUNORM(Format.FormatBC1Typeless);
            Assert.Equal(Format.FormatBC1Unorm, result);
        }

        [Fact]
        public void MakeTypelessFLOAT()
        {
            var result = DirectXTex.MakeTypelessFLOAT(Format.FormatR32G32B32A32Typeless);
            Assert.Equal(Format.FormatR32G32B32A32Float, result);
        }
    }
}