using Silk.NET.DXGI;

namespace HexaEngine.DirectXTex
{
    public unsafe struct Image
    {
        public ulong Width;
        public ulong Height;
        public Format Format;
        public ulong RowPitch;
        public ulong SlicePitch;
        public byte* Pixels;
    }
}