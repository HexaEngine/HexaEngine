namespace HexaEngine.D3D11
{
    using Hexa.NET.DirectXTex;
    using HexaEngine.Core.Graphics;

    public class D3DImage : IImage
    {
        public readonly Image Image;

        public D3DImage(Image image)
        {
            Image = image;
        }

        public int Width => (int)Image.Width;

        public int Height => (int)Image.Height;

        public Format Format => Helper.ConvertBack((Hexa.NET.DXGI.Format)Image.Format);

        public int RowPitch => (int)Image.RowPitch;

        public int SlicePitch => (int)Image.SlicePitch;

        public unsafe byte* Pixels => Image.Pixels;
    }
}