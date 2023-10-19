namespace HexaEngine.UI.Text
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO.Fonts;
    using System.Numerics;

    public class FontAtlas : IDisposable
    {
        private readonly Texture2D texture;
        private readonly ISamplerState samplerState;
        private readonly Dictionary<char, TextGlyph> glyphs = new();
        private readonly Dictionary<(char, char), float> kerningPairs = new();
        private bool disposedValue;

        public FontAtlas(IGraphicsDevice device, string path) : this(device, FontFile.Load(path), SamplerStateDescription.LinearClamp)
        {
        }

        public FontAtlas(IGraphicsDevice device, string path, SamplerStateDescription samplerDescription) : this(device, FontFile.Load(path), samplerDescription)
        {
        }

        public FontAtlas(IGraphicsDevice device, FontFile font) : this(device, font, SamplerStateDescription.LinearClamp)
        {
        }

        public unsafe FontAtlas(IGraphicsDevice device, FontFile font, SamplerStateDescription samplerDescription)
        {
            var width = (int)font.Header.BitmapWidth;
            var height = (int)font.Header.BitmapHeight;

            Spacing = new(font.Header.SpacingHorizontal, font.Header.SpacingVertical);

            for (var i = 0; i < font.Glyphs.Count; i++)
            {
                var glyph = font.Glyphs[i];
                TextGlyph textGlyph;
                textGlyph.Glyph = glyph.Id;
                textGlyph.Offset = new Vector2(glyph.X, glyph.Y);
                textGlyph.Size = new Vector2(glyph.Width, glyph.Height);
                glyphs.Add(glyph.Id, textGlyph);
            }

            for (int i = 0; i < font.KerningPairs.Count; i++)
            {
                var pair = font.KerningPairs[i];
                kerningPairs.Add((pair.First, pair.Second), pair.Amount);
            }

            var pixels = font.PixelData;
            fixed (Vector4* pPixels = pixels)
            {
                Texture2DDescription description = new(Format.R32G32B32A32Float, width, height, 1, 1, BindFlags.ShaderResource, Usage.Immutable);
                SubresourceData subresourceData = new(pPixels, sizeof(Vector4) * width);
                texture = new(device, description, subresourceData);
            }

            samplerState = device.CreateSamplerState(samplerDescription);
        }

        public Vector2 AtlasSize => new(texture.Width, texture.Height);

        public IShaderResourceView SRV => texture.SRV;

        public ISamplerState SamplerState => samplerState;

        public Vector2 Spacing { get; set; }

        public void CreateRun(string? text, VertexBuffer<TextVertex> vertexBuffer, IndexBuffer<uint> indexBuffer)
        {
#if DEBUG
            Logger.Assert(!disposedValue, "FontAtlas is disposed");
#endif
            if (text == null)
                return;

            float x = 0;
            float y = 0;
            uint v = 0;
            char last = '\0';
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                var glyph = glyphs[c];

                if (kerningPairs.TryGetValue((last, c), out var amount))
                {
                    x -= amount;
                }

                var p = new Vector3(x, y, 0);
                TextVertex v0 = new(p, glyph.Offset);                                                                   // 0  0 top left
                TextVertex v1 = new(p + new Vector3(glyph.Size.X, 0, 0), glyph.Offset + new Vector2(glyph.Size.X, 0));  // 1  0 top right
                TextVertex v2 = new(p + new Vector3(0, glyph.Size.Y, 0), glyph.Offset + new Vector2(0, glyph.Size.Y));  // 0  1 bottom left
                TextVertex v3 = new(p + new Vector3(glyph.Size, 0), glyph.Offset + glyph.Size);                         // 1  1 bottom right

                vertexBuffer.Add(v0);
                vertexBuffer.Add(v1);
                vertexBuffer.Add(v2);
                vertexBuffer.Add(v3);

                // first triangle (bottom left - top left - top right)
                uint i0 = v + 2;
                uint i1 = v;
                uint i2 = v + 1;

                // second triangle (bottom left - top right - bottom right)
                uint i3 = v + 2;
                uint i4 = v + 1;
                uint i5 = v + 3;

                indexBuffer.Add(i0);
                indexBuffer.Add(i1);
                indexBuffer.Add(i2);
                indexBuffer.Add(i3);
                indexBuffer.Add(i4);
                indexBuffer.Add(i5);
                v += 4;

                x += glyph.Size.X + Spacing.X;

                if (c == '\n')
                {
                    y += glyph.Size.Y + Spacing.Y;
                }
                last = c;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                texture.Dispose();
                samplerState.Dispose();
                disposedValue = true;
            }
        }

        ~FontAtlas()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}