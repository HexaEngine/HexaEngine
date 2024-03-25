namespace HexaEngine.UI.Graphics.Text
{
    using Hexa.NET.FreeType;
    using HexaEngine.Core.Graphics;
    using HexaEngine.UI.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    public unsafe class SpriteFont : UIRevivableResource, IFont
    {
        private readonly IGraphicsDevice device;
        private readonly FTLibrary library;
        private readonly string path;
        private FTFace faceHandle;
        private FTFaceRec* face;

        private ITexture2D texture2D;
        private IShaderResourceView srv;

        private readonly List<SpriteFontGlyph> glyphList = [];
        private readonly Dictionary<uint, SpriteFontGlyph> glyphs = [];
        private readonly Dictionary<uint, GlyphMetrics> glyphMetrics = [];

        private float fontSize;
        private bool hinting;
        private float emSize;
        private int loadFlags;
        private uint kerningMode;

        public SpriteFont(IGraphicsDevice device, FTLibrary library, string path, float fontSize, bool hinting = false)
        {
            this.device = device;
            this.library = library;
            this.path = path;
            this.fontSize = fontSize;
            this.hinting = hinting;

            ReviveCore();
        }

        public float FontSize
        {
            get => fontSize;
            set
            {
                if (fontSize == value)
                    return;
                fontSize = value;
                if (!hinting)
                    return;
                DisposeCore();
                ReviveCore();
            }
        }

        public bool Hinting
        {
            get => hinting;
            set
            {
                if (hinting == value)
                    return;
                hinting = value;
                DisposeCore();
                ReviveCore();
            }
        }

        public float EmSize => emSize;

        public float GetLineHeight(float fontSize)
        {
            return face->Height / (float)face->UnitsPerEM * fontSize;
        }

        protected override void ReviveCore()
        {
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

        public void RenderText(UICommandList commandList, Vector2 origin, TextRange textSpan, float fontSize, Brush brush)
        {
            int indexCount = 6 * textSpan.Length;
            int vertexCount = 4 * textSpan.Length;

            commandList.PrimReserve(indexCount, vertexCount);

            float originalX = origin.X;

            uint previous = 0;
            for (int i = 0; i < textSpan.Length; i++)
            {
                uint charcode = textSpan[i];

                if (charcode == '\r') continue;

                if (charcode == '\n')
                {
                    origin.X = originalX;
                    origin.Y -= face->Height / (float)face->UnitsPerEM * fontSize;
                    if (hinting)
                    {
                        origin.Y = MathF.Round(origin.Y);
                    }

                    continue;
                }

                if (!glyphs.TryGetValue(charcode, out SpriteFontGlyph glyph))
                {
                    glyph = glyphs['?'];
                }

                if (previous != 0 && glyph.Index != 0)
                {
                    FTVector kerning;
                    FTError error = (FTError)faceHandle.GetKerning(previous, glyph.Index, kerningMode, &kerning);
                    if (error == FTError.FtErrOk)
                    {
                        origin.X += kerning.X / emSize * fontSize;
                    }
                }

                // Do not emit quad for empty glyphs (whitespace).
                if (glyph.Width > 0 && glyph.Height > 0)
                {
                    float u0 = glyph.BearingX / emSize;
                    float v0 = (glyph.BearingY - glyph.Height) / emSize;
                    float u1 = (glyph.BearingX + glyph.Width) / emSize;
                    float v1 = glyph.BearingY / emSize;

                    float x0 = origin.X + u0 * fontSize;
                    float y0 = origin.Y + fontSize - v1 * fontSize;
                    float x1 = origin.X + u1 * fontSize;
                    float y1 = origin.Y + fontSize - v0 * fontSize;

                    commandList.PrimRect(new(x0, y0), new(x1, y1), glyph.UVStart, glyph.UVEnd, uint.MaxValue);
                }

                origin.X += glyph.Advance / emSize * fontSize;

                previous = glyph.Index;
            }

            commandList.RecordDraw(UICommandType.DrawTexture, brush, srv.NativePointer);
        }

        public void RenderText(UICommandList commandList, Vector2 origin, TextRange textSpan, float fontSize, float whitespaceScale, float incrementalTabStop, ReadingDirection readingDirection, Brush brush)
        {
            int vertexCount = 4 * textSpan.Length;
            int indexCount = 6 * textSpan.Length;

            commandList.PrimReserve(indexCount, vertexCount);

            float originalX = origin.X;

            bool rightToLeft = readingDirection == ReadingDirection.RightToLeft;
            int startIndex = rightToLeft ? textSpan.Length - 1 : 0;
            int endIndex = rightToLeft ? -1 : textSpan.Length;

            int step = rightToLeft ? -1 : 1;

            uint previous = 0;
            for (int i = startIndex; i != endIndex; i += step)
            {
                uint charcode = textSpan[i];

                if (charcode == '\r') continue;

                if (charcode == '\n')
                {
                    origin.X = originalX;
                    origin.Y += face->Height / (float)face->UnitsPerEM * fontSize;
                    if (hinting)
                    {
                        origin.Y = MathF.Round(origin.Y);
                    }

                    continue;
                }

                if (charcode == '\t')
                {
                    origin.X = ((int)(origin.X / incrementalTabStop) + 1) * incrementalTabStop;
                    continue;
                }

                if (!glyphs.TryGetValue(charcode, out SpriteFontGlyph glyph))
                {
                    glyph = glyphs['?'];
                }

                if (previous != 0 && glyph.Index != 0)
                {
                    FTVector kerning;
                    FTError error = (FTError)faceHandle.GetKerning(previous, glyph.Index, kerningMode, &kerning);
                    if (error == FTError.FtErrOk)
                    {
                        origin.X += kerning.X / emSize * fontSize;
                    }
                }

                // Do not emit quad for empty glyphs (whitespace).
                if (glyph.Width > 0 && glyph.Height > 0)
                {
                    float u0 = glyph.BearingX / emSize;
                    float v0 = (glyph.BearingY - glyph.Height) / emSize;
                    float u1 = (glyph.BearingX + glyph.Width) / emSize;
                    float v1 = glyph.BearingY / emSize;

                    float x0 = origin.X + u0 * fontSize;
                    float y0 = origin.Y + fontSize - v1 * fontSize;
                    float x1 = origin.X + u1 * fontSize;
                    float y1 = origin.Y + fontSize - v0 * fontSize;

                    commandList.PrimRect(new(x0, y0), new(x1, y1), glyph.UVStart, glyph.UVEnd, uint.MaxValue);
                }

                if (charcode == ' ')
                {
                    origin.X += glyph.Advance / emSize * fontSize * whitespaceScale;
                }
                else
                {
                    origin.X += glyph.Advance / emSize * fontSize;
                }

                previous = glyph.Index;
            }

            commandList.RecordDraw(UICommandType.DrawTexture, brush, srv.NativePointer);
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

        public Vector2 MeasureSize(TextRange text, float fontSize, float incrementalTabStop)
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

        protected override void DisposeCore()
        {
            glyphList.Clear();
            glyphs.Clear();
            glyphMetrics.Clear();
            texture2D.Dispose();
            srv.Dispose();
            faceHandle.DoneFace();
        }
    }
}