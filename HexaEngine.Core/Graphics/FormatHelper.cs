namespace HexaEngine.Core.Graphics
{
    public static class FormatHelper
    {
        public static int GetBitsPerPixel(Format fmt)
        {
            return fmt switch
            {
                Format.RGBA32Typeless or Format.RGBA32Float or Format.RGBA32UInt or Format.RGBA32SInt => 128,
                Format.RGB32Typeless or Format.RGB32Float or Format.RGB32UInt or Format.RGB32SInt => 96,
                Format.RGBA16Typeless or Format.RGBA16Float or Format.RGBA16UNorm or Format.RGBA16UInt or Format.RGBA16SNorm or Format.RGBA16SInt or Format.RG32Typeless or Format.RG32Float or Format.RG32UInt or Format.RG32SInt or Format.R32G8X24Typeless or Format.Depth32FloatStencil8X24UInt or Format.R32FloatX8X24Typeless or Format.X32TypelessG8X24UInt => 64,
                Format.RGB10A2Typeless or Format.RGB10A2UNorm or Format.RG11B10Float or Format.RGBA8Typeless or Format.RGBA8UNorm or Format.RGBA8UNormSrgb or Format.RGBA8UInt or Format.RGBA8SNorm or Format.RGBA8SInt or Format.RG16Typeless or Format.RG16Float or Format.RG16UNorm or Format.RG16UInt or Format.RG16SNorm or Format.RG16SInt or Format.R32Typeless or Format.D32Float or Format.R32Float or Format.R32UInt or Format.R32SInt or Format.R24G8Typeless or Format.Depth24UNormStencil8 or Format.R24UNormX8Typeless or Format.X24TypelessG8UInt or Format.BGRA8UNorm or Format.BGRA8Typeless or Format.BGRA8UNormSrgb or Format.BGRX8Typeless => 32,
                Format.RG8Typeless or Format.RG8UNorm or Format.RG8UInt or Format.RG8SNorm or Format.RG8SInt or Format.R16Typeless or Format.R16Float or Format.D16UNorm or Format.R16UNorm or Format.R16UInt or Format.R16SNorm or Format.R16SInt => 16,
                Format.R8Typeless or Format.R8UNorm or Format.R8UInt or Format.R8SNorm or Format.R8SInt or Format.BC2UNorm or Format.BC2UNormSrgb or Format.BC3UNorm or Format.BC3UNormSrgb or Format.BC5UNorm or Format.BC5SNorm or Format.BC6HTypeless or Format.BC7Typeless or Format.BC7UNorm or Format.BC7UNormSrgb => 8,
                _ => 0,
            };
        }

        public static bool IsCompressed(Format format)
        {
            return format switch
            {
                Format.BC1UNorm or Format.BC1UNormSrgb or Format.BC2UNorm or Format.BC2UNormSrgb or Format.BC3UNorm or Format.BC3UNormSrgb or Format.BC4UNorm or Format.BC4SNorm or Format.BC5UNorm or Format.BC5SNorm or Format.BC6HUFloat or Format.BC6HFloat or Format.BC7UNorm or Format.BC7UNormSrgb => true,
                _ => false,
            };
        }
    }
}