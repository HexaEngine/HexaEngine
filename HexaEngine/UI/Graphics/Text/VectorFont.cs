namespace HexaEngine.UI.Graphics.Text
{
    using Hexa.NET.FreeType;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.UI.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    public unsafe class VectorFont : UIRevivableResource, IFont
    {
        private readonly IGraphicsDevice device;
        private readonly FTLibrary library;
        private readonly string path;
        private FTFace faceHandle;
        private FTFaceRec* face;

        private readonly Dictionary<uint, VectorFontGlyph> glyphs = [];
        private readonly Dictionary<uint, GlyphMetrics> glyphMetrics = [];

        private IBuffer glyphBuffer;
        private IBuffer curveBuffer;
        private IShaderResourceView glyphBufferSRV;
        private IShaderResourceView curveBufferSRV;

        private float fontSize;
        private bool hinting;
        private float emSize;
        private int loadFlags;
        private uint kerningMode;
        private float dilation = 0.1f;

        private UnsafeList<BufferGlyph> bufferGlyphs = [];
        private UnsafeList<BufferCurve> bufferCurves = [];

        public VectorFont(IGraphicsDevice device, FTLibrary library, string path, float fontSize = 1.0f, bool hinting = false)
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
                {
                    return;
                }

                fontSize = value;
                if (!hinting)
                {
                    return;
                }

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
                {
                    return;
                }

                hinting = value;
                DisposeCore();
                ReviveCore();
            }
        }

        public float EmSize => emSize;

        public float Dilation { get => dilation; set => dilation = value; }

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

            var family = ToStringFromUTF8(face->FamilyName);
            var style = ToStringFromUTF8(face->StyleName);

            TtOs2* os2 = (TtOs2*)faceHandle.GetSfntTable(FTSfntTag.Os2);

            FontWeight fontWeight = FontWeight.Regular;

            if (os2 != null)
            {
                fontWeight = (FontWeight)os2->UsWeightClass;
            }

            if (hinting)
            {
                loadFlags = 1 << 3;
                kerningMode = (uint)FTKerningMode.Default;
                emSize = fontSize * 64f;

                error = (FTError)faceHandle.SetPixelSizes(0, (uint)MathF.Ceiling(fontSize));
                if (error != FTError.FtErrOk)
                {
                    throw new($"Failed to set pixel sizes, {error}");
                }
            }
            else
            {
                loadFlags = 1 << 0 | 1 << 1 | 1 << 3;
                kerningMode = (uint)FTKerningMode.Unscaled;
                emSize = face->UnitsPerEM;
            }

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
                    throw new($"Failed to load glyph for character {charcode} : {error}");
                }

                BuildGlyph(charcode, glyphIndex);
            }

            glyphBuffer = device.CreateBuffer(bufferGlyphs.Data, bufferGlyphs.Size, new BufferDescription((int)(bufferGlyphs.Size * sizeof(BufferGlyph)), BindFlags.ShaderResource, Usage.Immutable, CpuAccessFlags.None, ResourceMiscFlag.BufferStructured, sizeof(BufferGlyph)));
            curveBuffer = device.CreateBuffer(bufferCurves.Data, bufferCurves.Size, new BufferDescription((int)(bufferCurves.Size * sizeof(BufferCurve)), BindFlags.ShaderResource, Usage.Immutable, CpuAccessFlags.None, ResourceMiscFlag.BufferStructured, sizeof(BufferCurve)));

            glyphBufferSRV = device.CreateShaderResourceView(glyphBuffer);
            curveBufferSRV = device.CreateShaderResourceView(curveBuffer);

            bufferGlyphs.Release();
            bufferCurves.Release();
        }

        private void BuildGlyph(uint charcode, uint glyphIndex)
        {
            BufferGlyph bufferGlyph;
            bufferGlyph.Index = (int)bufferCurves.Size;

            short start = 0;
            for (int i = 0; i < face->Glyph->Outline.NContours; i++)
            {
                // Note: The end indices in face->glyph->outline.contours are inclusive.
                ConvertContour(ref bufferCurves, &face->Glyph->Outline, start, face->Glyph->Outline.Contours[i], emSize);
                start = (short)(face->Glyph->Outline.Contours[i] + 1);
            }

            bufferGlyph.Count = (int)(bufferCurves.Size - bufferGlyph.Index);

            uint bufferIndex = bufferGlyphs.Size;
            bufferGlyphs.Add(bufferGlyph);

            VectorFontGlyph glyph = new()
            {
                Index = glyphIndex,
                BufferIndex = bufferIndex,
                CurveCount = bufferGlyph.Count,
                Width = face->Glyph->Metrics.Width,
                Height = face->Glyph->Metrics.Height,
                BearingX = face->Glyph->Metrics.HoriBearingX,
                BearingY = face->Glyph->Metrics.HoriBearingY,
                Advance = face->Glyph->Metrics.HoriAdvance
            };

            glyphs.Add(charcode, glyph);

            GlyphMetrics metrics = new()
            {
                Glyph = glyphIndex,
                Width = face->Glyph->Metrics.Width,
                Height = face->Glyph->Metrics.Height,
                HorizontalBearingX = face->Glyph->Metrics.HoriBearingX,
                HorizontalBearingY = face->Glyph->Metrics.HoriBearingY,
                HorizontalAdvance = face->Glyph->Metrics.HoriAdvance,
                VerticalBearingX = face->Glyph->Metrics.VertBearingX,
                VerticalBearingY = face->Glyph->Metrics.VertBearingY,
                VerticalAdvance = face->Glyph->Metrics.VertAdvance,
            };
            glyphMetrics.Add(charcode, metrics);
        }

        // This function takes a single contour (defined by firstIndex and
        // lastIndex, both inclusive) from outline and converts it into individual
        // quadratic bezier curves, which are added to the curves vector.
        private void ConvertContour(ref UnsafeList<BufferCurve> curves, FTOutline* outline, short firstIndex, short lastIndex, float emSize)
        {
            // See https://freetype.org/freetype2/docs/glyphs/glyphs-6.html
            // for a detailed description of the outline format.
            //
            // In short, a contour is a list of points describing line segments
            // and quadratic or cubic bezier curves that form a closed shape.
            //
            // TrueType fonts only contain quadratic bezier curves. OpenType fonts
            // may contain outline data in TrueType format or in Compact Font
            // Format, which also allows cubic beziers. However, in FreeType it is
            // (theoretically) possible to mix the two types of bezier curves, so
            // we handle both at the same time.
            //
            // Each point in the contour has a tag specifying its type
            // (FT_CURVE_TAG_ON, FT_CURVE_TAG_CONIC or FT_CURVE_TAG_CUBIC).
            // FT_CURVE_TAG_ON points sit exactly on the outline, whereas the
            // other types are control points for quadratic/conic bezier curves,
            // which in general do not sit exactly on the outline and are also
            // called off points.
            //
            // Some examples of the basic segments:
            // ON - ON ... line segment
            // ON - CONIC - ON ... quadratic bezier curve
            // ON - CUBIC - CUBIC - ON ... cubic bezier curve
            //
            // Cubic bezier curves must always be described by two CUBIC points
            // inbetween two ON points. For the points used in the TrueType format
            // (ON, CONIC) there is a special rule, that two consecutive points of
            // the same type imply a virtual point of the opposite type at their
            // exact midpoint.
            //
            // For example the sequence ON - CONIC - CONIC - ON describes two
            // quadratic bezier curves where the virtual point forms the joining
            // end point of the two curves: ON - CONIC - [ON] - CONIC - ON.
            //
            // Similarly the sequence ON - ON can be thought of as a line segment
            // or a quadratic bezier curve (ON - [CONIC] - ON). Because the
            // virtual point is at the exact middle of the two endpoints, the
            // bezier curve is identical to the line segment.
            //
            // The font shader only supports quadratic bezier curves, so we use
            // this virtual point rule to represent line segments as quadratic
            // bezier curves.
            //
            // Cubic bezier curves are slightly more difficult, since they have a
            // higher degree than the shader supports. Each cubic curve is
            // approximated by two quadratic curves according to the following
            // paper. This preserves C1-continuity (location of and tangents at
            // the end points of the cubic curve) and the paper even proves that
            // splitting at the parametric center minimizes the error due to the
            // degree reduction. One could also analyze the approximation error
            // and split the cubic curve, if the error is too large. However,
            // almost all fonts use "nice" cubic curves, resulting in very small
            // errors already (see also the section on Font Design in the paper).
            //
            // Quadratic Approximation of Cubic Curves
            // Nghia Truong, Cem Yuksel, Larry Seiler
            // https://ttnghia.github.io/pdf/QuadraticApproximation.pdf
            // https://doi.org/10.1145/3406178

            if (firstIndex == lastIndex)
            {
                return;
            }

            short dIndex = 1;
            if ((outline->Flags & FreeType.FT_OUTLINE_REVERSE_FILL) != 0)
            {
                (firstIndex, lastIndex) = (lastIndex, firstIndex);
                dIndex = -1;
            }

            Vector2 Convert(FTVector v)
            {
                return new Vector2(v.X / emSize, v.Y / emSize);
            };

            Vector2 Midpoint(Vector2 a, Vector2 b)
            {
                return 0.5f * (a + b);
            };

            // Find a point that is on the curve and remove it from the list.
            Vector2 first;
            bool firstOnCurve = (outline->Tags[firstIndex] & FreeType.FT_CURVE_TAG_ON) != 0;
            if (firstOnCurve)
            {
                first = Convert(outline->Points[firstIndex]);
                firstIndex += dIndex;
            }
            else
            {
                bool lastOnCurve = (outline->Tags[lastIndex] & FreeType.FT_CURVE_TAG_ON) != 0;
                if (lastOnCurve)
                {
                    first = Convert(outline->Points[lastIndex]);
                    lastIndex -= dIndex;
                }
                else
                {
                    first = Midpoint(Convert(outline->Points[firstIndex]), Convert(outline->Points[lastIndex]));
                    // This is a virtual point, so we don't have to remove it.
                }
            }

            Vector2 start = first;
            Vector2 control = first;
            Vector2 previous = first;
            char previousTag = (char)FreeType.FT_CURVE_TAG_ON;
            for (short index = firstIndex; index != lastIndex + dIndex; index += dIndex)
            {
                Vector2 current = Convert(outline->Points[index]);
                char currentTag = (char)outline->Tags[index];
                if (currentTag == FreeType.FT_CURVE_TAG_CUBIC)
                {
                    // No-op, wait for more points.
                    control = previous;
                }
                else if (currentTag == FreeType.FT_CURVE_TAG_ON)
                {
                    if (previousTag == FreeType.FT_CURVE_TAG_CUBIC)
                    {
                        Vector2 b0 = start;
                        Vector2 b1 = control;
                        Vector2 b2 = previous;
                        Vector2 b3 = current;

                        Vector2 c0 = b0 + 0.75f * (b1 - b0);
                        Vector2 c1 = b3 + 0.75f * (b2 - b3);

                        Vector2 d = Midpoint(c0, c1);

                        curves.Add(new(b0, c0, d));
                        curves.Add(new(d, c1, b3));
                    }
                    else if (previousTag == FreeType.FT_CURVE_TAG_ON)
                    {
                        // Linear segment.
                        curves.Add(new(previous, Midpoint(previous, current), current));
                    }
                    else
                    {
                        // Regular bezier curve.
                        curves.Add(new(start, previous, current));
                    }
                    start = current;
                    control = current;
                }
                else /* currentTag == FT_CURVE_TAG_CONIC */
                {
                    if (previousTag == FreeType.FT_CURVE_TAG_ON)
                    {
                        // No-op, wait for third point.
                    }
                    else
                    {
                        // Create virtual on point.
                        Vector2 mid = Midpoint(previous, current);
                        curves.Add(new(start, previous, mid));
                        start = mid;
                        control = mid;
                    }
                }
                previous = current;
                previousTag = currentTag;
            }

            // Close the contour.
            if (previousTag == FreeType.FT_CURVE_TAG_CUBIC)
            {
                Vector2 b0 = start;
                Vector2 b1 = control;
                Vector2 b2 = previous;
                Vector2 b3 = first;

                Vector2 c0 = b0 + 0.75f * (b1 - b0);
                Vector2 c1 = b3 + 0.75f * (b2 - b3);

                Vector2 d = Midpoint(c0, c1);

                curves.Add(new(b0, c0, d));
                curves.Add(new(d, c1, b3));
            }
            else if (previousTag == FreeType.FT_CURVE_TAG_ON)
            {
                // Linear segment.
                curves.Add(new(previous, Midpoint(previous, first), first));
            }
            else
            {
                curves.Add(new(start, previous, first));
            }
        }

        public void RenderText(UICommandList commandList, Vector2 origin, TextRange textSpan, float fontSize, Brush brush)
        {
            RenderText(commandList, origin, textSpan.AsSpan(), fontSize, brush);
        }

        public void RenderText(UICommandList commandList, Vector2 origin, ReadOnlySpan<char> textSpan, float fontSize, Brush brush)
        {
            int vertexCount = 4 * textSpan.Length;
            int indexCount = 6 * textSpan.Length;

            commandList.PrimReserve(indexCount, vertexCount);

            float originalX = origin.X;

            uint previous = 0;
            for (int i = 0; i < textSpan.Length; i++)
            {
                uint charcode = textSpan[i];

                if (charcode == '\r')
                {
                    continue;
                }

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

                if (!glyphs.TryGetValue(charcode, out VectorFontGlyph glyph))
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
                if (glyph.CurveCount > 0)
                {
                    uint d = (uint)(emSize * dilation);

                    float u0 = (glyph.BearingX - d) / emSize;
                    float v0 = (glyph.BearingY - glyph.Height - d) / emSize;
                    float u1 = (glyph.BearingX + glyph.Width + d) / emSize;
                    float v1 = (glyph.BearingY + d) / emSize;

                    float x0 = origin.X + u0 * fontSize;
                    float y0 = origin.Y + fontSize - v1 * fontSize;
                    float x1 = origin.X + u1 * fontSize;
                    float y1 = origin.Y + fontSize - v0 * fontSize;

                    uint idx0 = commandList.AddVertex(new(new(x0, y0), new(u0, v1), glyph.BufferIndex));
                    uint idx1 = commandList.AddVertex(new(new(x1, y0), new(u1, v1), glyph.BufferIndex));
                    uint idx2 = commandList.AddVertex(new(new(x1, y1), new(u1, v0), glyph.BufferIndex));
                    uint idx3 = commandList.AddVertex(new(new(x0, y1), new(u0, v0), glyph.BufferIndex));
                    commandList.AddFace(idx0, idx1, idx2);
                    commandList.AddFace(idx0, idx2, idx3);
                }

                origin.X += glyph.Advance / emSize * fontSize;

                previous = glyph.Index;
            }

            commandList.RecordDraw(UICommandType.DrawTextVector, brush, glyphBufferSRV.NativePointer, curveBufferSRV.NativePointer);
        }

        public void RenderText(UICommandList commandList, Vector2 origin, TextRange text, float fontSize, float whitespaceScale, float incrementalTabStop, ReadingDirection readingDirection, Brush brush)
        {
            RenderText(commandList, origin, text.AsSpan(), fontSize, whitespaceScale, incrementalTabStop, readingDirection, brush);
        }

        public void RenderText(UICommandList commandList, Vector2 origin, ReadOnlySpan<char> text, float fontSize, float whitespaceScale, float incrementalTabStop, ReadingDirection readingDirection, Brush brush)
        {
            int vertexCount = 4 * text.Length;
            int indexCount = 6 * text.Length;

            commandList.PrimReserve(indexCount, vertexCount);

            float originalX = origin.X;

            bool rightToLeft = readingDirection == ReadingDirection.RightToLeft;
            int startIndex = rightToLeft ? text.Length - 1 : 0;
            int endIndex = rightToLeft ? -1 : text.Length;

            int step = rightToLeft ? -1 : 1;

            uint previous = 0;
            for (int i = startIndex; i != endIndex; i += step)
            {
                uint charcode = text[i];

                if (charcode == '\r')
                {
                    continue;
                }

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

                if (!glyphs.TryGetValue(charcode, out VectorFontGlyph glyph))
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
                if (glyph.CurveCount > 0)
                {
                    uint d = (uint)(emSize * dilation);

                    float u0 = (glyph.BearingX - d) / emSize;
                    float v0 = (glyph.BearingY - glyph.Height - d) / emSize;
                    float u1 = (glyph.BearingX + glyph.Width + d) / emSize;
                    float v1 = (glyph.BearingY + d) / emSize;

                    float x0 = origin.X + u0 * fontSize;
                    float y0 = origin.Y + (fontSize - v1 * fontSize);
                    float x1 = origin.X + u1 * fontSize;
                    float y1 = origin.Y + (fontSize - v0 * fontSize);

                    uint idx0 = commandList.AddVertex(new(new(x0, y0), new(u0, v1), glyph.BufferIndex));
                    uint idx1 = commandList.AddVertex(new(new(x1, y0), new(u1, v1), glyph.BufferIndex));
                    uint idx2 = commandList.AddVertex(new(new(x1, y1), new(u1, v0), glyph.BufferIndex));
                    uint idx3 = commandList.AddVertex(new(new(x0, y1), new(u0, v0), glyph.BufferIndex));
                    commandList.AddFace(idx0, idx1, idx2);
                    commandList.AddFace(idx0, idx2, idx3);
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

            commandList.RecordDraw(UICommandType.DrawTextVector, brush, glyphBufferSRV.NativePointer, curveBufferSRV.NativePointer);
        }

        public GlyphMetrics GetMetrics(uint character)
        {
            if (glyphMetrics.TryGetValue(character, out var metrics))
            {
                return metrics;
            }
            return default;
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

        public Vector2 MeasureSize(TextRange text, float fontSize, float incrementalTabStop)
        {
            return MeasureSize(text.AsSpan(), fontSize, incrementalTabStop);
        }

        public Vector2 MeasureSize(ReadOnlySpan<char> text, float fontSize, float incrementalTabStop)
        {
            float x = 0;
            uint previous = 0;
            for (int i = 0; i < text.Length; i++)
            {
                var charcode = text[i];

                if (charcode == '\t')
                {
                    x = ((int)(x / incrementalTabStop) + 1) * incrementalTabStop;
                    continue;
                }

                GlyphMetrics metrics = GetMetrics(charcode);

                if (previous != 0 && metrics.Glyph != 0)
                {
                    FTVector kerning;
                    FTError error = (FTError)faceHandle.GetKerning(previous, metrics.Glyph, kerningMode, &kerning);
                    if (error == FTError.FtErrOk)
                    {
                        x += kerning.X / emSize * fontSize;
                    }
                }

                x += metrics.HorizontalAdvance / emSize * fontSize;
            }
            return new Vector2(x, GetLineHeight(fontSize));
        }

        protected override void DisposeCore()
        {
            glyphs.Clear();
            glyphMetrics.Clear();
            glyphBuffer.Dispose();
            glyphBufferSRV.Dispose();
            curveBuffer.Dispose();
            curveBufferSRV.Dispose();
            faceHandle.DoneFace();
        }
    }
}