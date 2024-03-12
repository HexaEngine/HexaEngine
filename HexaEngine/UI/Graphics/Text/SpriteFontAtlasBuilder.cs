namespace HexaEngine.UI.Graphics.Text
{
    using Hexa.NET.FreeType;
    using System;
    using System.Numerics;
    using System.Runtime.InteropServices;

    public unsafe class SpriteFontAtlasBuilder
    {
        private uint x;
        private uint y;
        private readonly uint width;
        private uint height;
        private uint* data;
        private uint paddingX = 1;
        private uint paddingY = 1;
        private uint maxHeight = 0;

        public SpriteFontAtlasBuilder(uint width, uint height)
        {
            this.width = width;
            this.height = height;
            data = (uint*)Marshal.AllocHGlobal((nint)(width * height * sizeof(uint)));
        }

        public uint Width => width;

        public uint Height => height;

        public uint* Data => data;

        public uint PaddingX { get => paddingX; set => paddingX = value; }

        public uint PaddingY { get => paddingY; set => paddingY = value; }

        public void Reset()
        {
            height = 256;
            data = (uint*)Marshal.ReAllocHGlobal((nint)data, (nint)(width * height * sizeof(uint)));
            x = 0;
            y = 0;
        }

        public Vector2 Append(FTGlyphSlotRec* glyph)
        {
            var position = new Vector2(x, y);

            if (x + glyph->Bitmap.Width >= width)
            {
                y += maxHeight + paddingY;
                x = 0;
                maxHeight = 0;
                position = new Vector2(x, y);
            }

            maxHeight = Math.Max(maxHeight, glyph->Bitmap.Rows);

            while (y + maxHeight >= height)
            {
                IncreaseCapacity();
            }

            x += glyph->Bitmap.Width + paddingX;

            CopyToBuffer(glyph->Bitmap, (uint)position.X, (uint)position.Y);

            return position;
        }

        public void Finish()
        {
            height = y + maxHeight;
            data = (uint*)Marshal.ReAllocHGlobal((nint)data, (nint)(width * height * sizeof(uint)));
        }

        public void Release()
        {
            Marshal.FreeHGlobal((nint)data);
        }

        private void CopyToBuffer(FTBitmap bitmap, uint x, uint y)
        {
            FTPixelMode pixelMode = (FTPixelMode)bitmap.PixelMode;

            for (int i = 0; i < bitmap.Rows; i++)
            {
                for (int j = 0; j < bitmap.Width; j++)
                {
                    uint pixel = 0;
                    switch (pixelMode)
                    {
                        case FTPixelMode.None:
                            continue;

                        case FTPixelMode.Mono:
                            break;

                        case FTPixelMode.Gray:
                            byte intensity = bitmap.Buffer[i * bitmap.Width + j];
                            pixel = (uint)(intensity << 24 | intensity << 16 | intensity << 8 | intensity);
                            break;

                        case FTPixelMode.Gray2:
                            break;

                        case FTPixelMode.Gray4:
                            break;

                        case FTPixelMode.Lcd:
                            break;

                        case FTPixelMode.Lcdv:
                            break;

                        case FTPixelMode.Bgra:
                            break;

                        case FTPixelMode.Max:
                            break;
                    }

                    data[(y + i) * width + x + j] = pixel;
                }
            }
        }

        private void IncreaseCapacity()
        {
            height += 256;
            data = (uint*)Marshal.ReAllocHGlobal((nint)data, (nint)(width * height * sizeof(uint)));
        }
    }
}