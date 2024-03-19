namespace HexaEngine.UI.Graphics.Text
{
    using System.Numerics;

    public class TextLayout : UIResource
    {
        private string text;
        private TextFormat format;
        private readonly UICommandList preRecordedList = new();
        private TextLayoutMetrics metrics;
        private float maxWidth;
        private float maxHeight;

        private readonly List<LineSpan> lines = [];

        public TextLayout(string text, TextFormat format, float maxWidth, float maxHeight)
        {
            format.AddRef();
            this.text = text;
            this.format = format;
            this.maxWidth = maxWidth;
            this.maxHeight = maxHeight;
            UpdateLayout();
        }

        private struct LineSpan
        {
            public TextSpan Text;
            public Vector2 Position;
            public Vector2 Size;

            public readonly int Length => Text.Length;

            public readonly Vector2 Min => Position;

            public readonly Vector2 Max => Position + Size;

            public char this[int index]
            {
                get => Text[index];
            }

            public LineSpan(TextSpan text, Vector2 position, Vector2 size)
            {
                Text = text;
                Position = position;
                Size = size;
            }
        }

        public string Text
        {
            get => text;
            set
            {
                if (text == value)
                {
                    return;
                }
                text = value;
                UpdateLayout();
            }
        }

        public TextFormat Format
        {
            get => format;
            set
            {
                format.Dispose();
                value.AddRef();
                format = value;
                UpdateLayout();
            }
        }

        public TextLayoutMetrics Metrics => metrics;

        public float MaxWidth
        {
            get => maxWidth;
            set
            {
                if (maxWidth == value)
                {
                    return;
                }
                maxWidth = value;
            }
        }

        public float MaxHeight
        {
            get => maxHeight;
            set
            {
                if (maxHeight == value)
                {
                    return;
                }
                maxHeight = value;
            }
        }

        public IFont Font { get => Format.Font; }

        public void UpdateLayout()
        {
            lines.Clear();

            IFont font = Format.Font;
            float emSize = Font.EmSize;
            float fontSize = Format.FontSize;
            float lineHeight = Font.GetLineHeight(Format.FontSize);
            float incrementalTabStop = Format.IncrementalTabStop;
            float wordSpacing = Format.WordSpacing;
            float lineSpacing = Format.LineSpacing;
            float whitespaceWidth = GetWhitespaceWidth(font, wordSpacing, emSize, Format.FontSize);

            lineHeight += lineSpacing / emSize * fontSize;

            WordWrapping wordWrapping = Format.WordWrapping;

            // measure text and split into lines.
            Measure(font, wordWrapping, emSize, fontSize, lineHeight, incrementalTabStop);

            // compute actual max and min width and height.
            float minWidth = 0;
            float minHeight = 0;
            float maxWidth = 0;
            float maxHeight = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var min = line.Min;
                var max = line.Max;
                minWidth = Math.Min(min.X, minWidth);
                minHeight = Math.Min(min.Y, minHeight);
                maxWidth = MathF.Max(max.X, maxWidth);
                maxHeight = MathF.Max(max.Y, maxHeight);
            }

            metrics = new(minWidth, minHeight, maxWidth, maxHeight);

            FlowDirection flowDirection = Format.FlowDirection;
            ReadingDirection readingDirection = Format.ReadingDirection;
            TextAlignment alignment = Format.TextAlignment;

            // reverse alignment if RTL layout.
            if (readingDirection == ReadingDirection.RightToLeft)
            {
                alignment = alignment switch
                {
                    TextAlignment.Leading => TextAlignment.Trailing,
                    TextAlignment.Trailing => TextAlignment.Leading,
                    TextAlignment.Center => TextAlignment.Center,
                    TextAlignment.Justified => TextAlignment.Justified,
                    _ => throw new NotSupportedException(),
                };
            }

            preRecordedList.BeginDraw();

            for (int i = 0; i < lines.Count; i++)
            {
                AddGlyphRun(font, lines[i], flowDirection, readingDirection, alignment, incrementalTabStop, whitespaceWidth);
            }

            preRecordedList.EndDraw();
        }

        private void Measure(IFont font, WordWrapping wordWrapping, float emSize, float fontSize, float lineHeight, float incrementalTabStop)
        {
            float penX = 0;
            float penY = 0;

            TextSpan span = new(text, 0, 0);
            uint previous = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    if (EmitLine(ref penX, ref penY, lineHeight, ref span, i))
                    {
                        return;
                    }

                    continue;
                }

                if (c == '\t')
                {
                    penX = ((int)(penX / incrementalTabStop) + 1) * incrementalTabStop;
                    continue;
                }

                GlyphMetrics metrics = font.GetMetrics(c);

                if (previous != 0 && metrics.Glyph != 0)
                {
                    if (font.GetKerning(previous, metrics.Glyph, out Vector2 kerning))
                    {
                        penX += kerning.X / emSize * fontSize;
                    }
                }

                float nextPositionX = penX + metrics.HorizontalAdvance / emSize * fontSize;

                if (nextPositionX > maxWidth && wordWrapping != WordWrapping.NoWrap)
                {
                    if (wordWrapping == WordWrapping.WrapWord)
                    {
                        // Reverse search for ' ' space char to find word boundary.
                        int j = i - 1;
                        while (j >= span.Start && text[j] != ' ')
                        {
                            j--;
                        }

                        // no word found, don't wrap here to avoid empty lines.
                        if (j == i || j == span.Start)
                        {
                            penX = nextPositionX;
                            continue;
                        }

                        // +1 to skip the space.
                        i = j + 1;

                        if (EmitLine(ref penX, ref penY, lineHeight, ref span, j))
                        {
                            return;
                        }

                        continue;
                    }

                    if (EmitLine(ref penX, ref penY, lineHeight, ref span, i))
                    {
                        return;
                    }

                    continue;
                }

                penX = nextPositionX;
            }

            if (span.Start < text.Length)
            {
                span.Length = text.Length - span.Start;
                AddLine(new(0, penY), span);
            }
        }

        private void AddLine(Vector2 origin, TextSpan span)
        {
            Vector2 size = Font.MeasureSize(span, Format.FontSize, Format.IncrementalTabStop);
            LineSpan line = new(span, origin, size);
            lines.Add(line);
        }

        private bool EmitLine(ref float penX, ref float penY, float lineHeight, ref TextSpan span, int i)
        {
            span.Length = i - span.Start;
            AddLine(new(0, penY), span);
            span.Start = i + 1;

            penX = 0;
            penY += lineHeight;

            return penY > maxHeight;
        }

        private void AddGlyphRun(IFont font, LineSpan span, FlowDirection flowDirection, ReadingDirection readingDirection, TextAlignment alignment, float incrementalTabStop, float whitespaceWidth)
        {
            if (span.Length == 0)
            {
                return;
            }

            Vector2 origin = span.Position;
            Vector2 size = span.Size;

            float usedSpace = size.X;
            float whitespaceScale = 1;
            bool bottomToTop = flowDirection == FlowDirection.BottomToTop;

            if (bottomToTop)
            {
                origin.Y = metrics.Height - (origin.Y - size.Y);
            }

            switch (alignment)
            {
                case TextAlignment.Leading:
                    origin.X = 0;
                    break;

                case TextAlignment.Trailing:
                    origin.X = metrics.Width - usedSpace;
                    break;

                case TextAlignment.Center:
                    origin.X = (metrics.Width - usedSpace) / 2;
                    break;

                case TextAlignment.Justified:
                    float freeSpace = metrics.Width - usedSpace;
                    int whitespaceCount = 0;
                    for (int i = 0; i < span.Length; i++)
                    {
                        if (span[i] == ' ')
                        {
                            whitespaceCount++;
                        }
                    }
                    if (whitespaceCount > 0)
                    {
                        whitespaceScale = freeSpace / (whitespaceCount * whitespaceWidth);
                    }

                    break;
            }

            font.RenderText(preRecordedList, origin, span.Text, Format.FontSize, whitespaceScale, incrementalTabStop, readingDirection, null);
        }

        private static float GetWhitespaceWidth(IFont font, float wordSpacing, float emSize, float fontSize)
        {
            return font.GetMetrics(' ').HorizontalAdvance / emSize * fontSize + wordSpacing / emSize * fontSize;
        }

        public void DrawText(UICommandList commandList, Brush? brush)
        {
            for (int i = 0; i < preRecordedList.CmdBuffer.Count; i++)
            {
                UIDrawCommand cmd = preRecordedList.CmdBuffer[i];
                cmd.Brush = brush;
                preRecordedList.CmdBuffer[i] = cmd;
            }

            commandList.ExecuteCommandList(preRecordedList);
        }

        protected override void DisposeCore()
        {
            preRecordedList.Dispose();
            format.Dispose();
        }
    }
}