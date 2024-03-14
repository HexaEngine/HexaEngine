namespace HexaEngine.UI.Graphics.Text
{
    using Hexa.NET.FreeType;
    using HexaEngine.Core.Graphics;
    using HexaEngine.UI.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    public unsafe class SpriteFont : IFont
    {
        private readonly FTFace faceHandle;
        private readonly FTFaceRec* face;
        private readonly ITexture2D texture2D;
        private readonly IShaderResourceView srv;
        private readonly List<SpriteFontGlyph> glyphList = [];
        private readonly Dictionary<uint, SpriteFontGlyph> glyphs = [];
        private readonly Dictionary<uint, GlyphMetrics> glyphMetrics = new();

        private readonly float fontSize;
        private readonly float emSize;
        private readonly bool hinting;
        private readonly int loadFlags;
        private readonly uint kerningMode;
        private bool disposedValue;

        public SpriteFont(IGraphicsDevice device, FTLibrary library, string path, float fontSize, bool hinting = false)
        {
            this.fontSize = fontSize;
            this.hinting = hinting;
            FTError error;

            FTFace faceHandle;
            error = (FTError)FreeType.FTNewFace(library, path, 0, &faceHandle);
            if (error != FTError.FtErrOk)
            {
                throw new($"Failed to load font file, {error}");
            }
            this.faceHandle = faceHandle;

            face = (FTFaceRec*)faceHandle.Handle;

            if (hinting)
            {
                loadFlags = 0;
                kerningMode = (uint)FTKerningMode.Default;
                emSize = fontSize * 64f;

                error = (FTError)faceHandle.SetPixelSizes(0, (uint)MathF.Ceiling(fontSize));
                if (error != FTError.FtErrOk)
                {
                    throw new($"Failed to load font file, {error}");
                }
            }
            else
            {
                loadFlags = 1 << 0 | 1 << 1;
                kerningMode = (uint)FTKerningMode.Unscaled;
                emSize = face->UnitsPerEM;
            }

            SpriteFontAtlasBuilder builder = new(2048, 256);

            for (uint charcode = 0; charcode < char.MaxValue; charcode++)
            {
                var glyphIndex = faceHandle.GetCharIndex(charcode);

                if (glyphIndex == 0)
                {
                    continue;
                }

                error = (FTError)faceHandle.LoadGlyph(glyphIndex, loadFlags);
                if (error != FTError.FtErrOk)
                {
                    throw new($"Failed to load font file, {error}");
                }

                BuildGlyph(builder, charcode, glyphIndex);
            }

            builder.Finish();

            Vector2 size = new(builder.Width, builder.Height);
            for (int i = 0; i < glyphList.Count; i++)
            {
                var character = glyphList[i];
                character.UVStart /= size;
                character.UVEnd = character.UVStart + character.UVEnd / size;
                glyphList[i] = character;
                glyphs.Add(character.Char, character);
            }

            Texture2DDescription desc = new(Format.R8G8B8A8UNorm, (int)builder.Width, (int)builder.Height, 1, 1, BindFlags.ShaderResource, Usage.Immutable);

            SubresourceData data = new((nint)builder.Data, (int)(builder.Width * sizeof(uint)));
            texture2D = device.CreateTexture2D(desc, [data]);
            srv = device.CreateShaderResourceView(texture2D);

            builder.Release();
        }

        private void BuildGlyph(SpriteFontAtlasBuilder builder, uint charcode, uint glyphIndex)
        {
            FTGlyphSlotRec* glyph = face->Glyph;

            if (glyph->Format != FTGlyphFormat.Bitmap)
            {
                FTError error = (FTError)FreeType.FTRenderGlyph(new FTGlyphSlot((nint)face->Glyph), FTRenderMode.Normal);

                if (error != FTError.FtErrOk)
                {
                    throw new($"Failed to load font file, {error}");
                }
            }

            var position = builder.Append(glyph);
            SpriteFontGlyph character = new()
            {
                UVStart = position,
                UVEnd = new Vector2(face->Glyph->Bitmap.Width, face->Glyph->Bitmap.Rows),
                Width = face->Glyph->Metrics.Width,
                Height = face->Glyph->Metrics.Height,
                BearingX = face->Glyph->Metrics.HoriBearingX,
                BearingY = face->Glyph->Metrics.HoriBearingY,
                Advance = face->Glyph->Metrics.HoriAdvance,
                Index = glyphIndex,
                Char = charcode,
            };
            glyphList.Add(character);

            GlyphMetrics metrics = new()
            {
                Glyph = glyphIndex,
                Width = glyph->Metrics.Width,
                Height = glyph->Metrics.Height,
                HorizontalBearingX = glyph->Metrics.HoriBearingX,
                HorizontalBearingY = glyph->Metrics.HoriBearingY,
                HorizontalAdvance = glyph->Metrics.HoriAdvance,
                VerticalBearingX = glyph->Metrics.VertBearingX,
                VerticalBearingY = glyph->Metrics.VertBearingY,
                VerticalAdvance = glyph->Metrics.VertAdvance,
            };
            glyphMetrics.Add(charcode, metrics);
        }

        public float FontSize => fontSize;

        public void RenderText(UICommandList list, string text, Vector2 position, Brush brush, float fontSize)
        {
            int indexCount = 6 * text.Length;
            int vertexCount = 4 * text.Length;

            list.PrimReserve(indexCount, vertexCount);

            float originalX = position.X;

            uint previous = 0;
            for (int i = 0; i < text.Length; i++)
            {
                uint charcode = text[i];

                if (charcode == '\r') continue;

                if (charcode == '\n')
                {
                    position.X = originalX;
                    position.Y -= face->Height / (float)face->UnitsPerEM * fontSize;
                    if (hinting)
                    {
                        position.Y = MathF.Round(position.Y);
                    }

                    continue;
                }

                SpriteFontGlyph glyph = glyphs[charcode];

                if (previous != 0 && glyph.Index != 0)
                {
                    FTVector kerning;
                    FTError error = (FTError)faceHandle.GetKerning(previous, glyph.Index, kerningMode, &kerning);
                    if (error == FTError.FtErrOk)
                    {
                        position.X += kerning.X / emSize * fontSize;
                    }
                }

                // Do not emit quad for empty glyphs (whitespace).
                if (glyph.Width > 0 || glyph.Height > 0)
                {
                    float u0 = glyph.BearingX / emSize;
                    float v0 = (glyph.BearingY - glyph.Height) / emSize;
                    float u1 = (glyph.BearingX + glyph.Width) / emSize;
                    float v1 = glyph.BearingY / emSize;

                    float x0 = position.X + u0 * fontSize;
                    float y0 = position.Y + fontSize - v1 * fontSize;
                    float x1 = position.X + u1 * fontSize;
                    float y1 = position.Y + fontSize - v0 * fontSize;

                    list.PrimRect(new(x0, y0), new(x1, y1), glyph.UVStart, glyph.UVEnd, uint.MaxValue);
                }

                position.X += glyph.Advance / emSize * fontSize;

                previous = glyph.Index;
            }

            list.RecordDraw(UICommandType.DrawTexture, brush, srv.NativePointer);
        }

        public float GetLineHeight(float fontSize)
        {
            return face->Height / (float)face->UnitsPerEM * fontSize;
        }

        public float EmSize
        {
            get => emSize;
        }

        public bool GetKerning(uint left, uint right, out Vector2 kerning)
        {
            FTVector ikerning;
            FTError error = (FTError)faceHandle.GetKerning(left, right, kerningMode, &ikerning);
            if (error == FTError.FtErrOk)
            {
                kerning = new(ikerning.X, ikerning.Y);
                return true;
            }
            kerning = default;
            return false;
        }

        public GlyphMetrics GetMetrics(uint character)
        {
            if (glyphMetrics.TryGetValue(character, out var metrics))
            {
                return metrics;
            }
            return default;
        }

        public Vector2 MeasureSize(TextSpan text, float fontSize, float incrementalTabStop)
        {
            float x = 0;
            float y = 0;
            uint last = 0;
            for (int i = text.Start; i < text.Length; i++)
            {
                var c = text[i];

                GlyphMetrics metrics = GetMetrics(c);

                FTVector kerning;
                faceHandle.GetKerning(last, metrics.Glyph, (uint)FTKerningMode.Default, &kerning);
                last = metrics.Glyph;

                float kernX = kerning.X / metrics.HorizontalAdvance;
                float kernY = kerning.Y / metrics.VerticalAdvance;

                x += metrics.HorizontalAdvance + kernX;
                y = MathF.Max(metrics.VerticalAdvance + kernY, y);
            }
            return new Vector2(x, y);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                texture2D.Dispose();
                srv.Dispose();
                glyphList.Clear();
                glyphs.Clear();
                glyphMetrics.Clear();
                faceHandle.DoneFace();
                disposedValue = true;
            }
        }

        ~SpriteFont()
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

        public void RenderText(UICommandList commandList, Vector2 origin, TextSpan textSpan, float fontSize, Brush brush)
        {
            throw new NotImplementedException();
        }

        public void RenderText(UICommandList commandList, Vector2 origin, TextSpan textSpan, float fontSize, float whitespaceScale, float tabStopIncrement, ReadingDirection readingDirection, Brush brush)
        {
            throw new NotImplementedException();
        }
    }
}