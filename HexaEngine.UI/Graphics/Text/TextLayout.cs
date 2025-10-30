namespace HexaEngine.UI.Graphics.Text
{
    using System;
    using System.Numerics;

    public class TextLayout : UIResource
    {
        private string text;
        private TextFormat format;
        private readonly UICommandList preRecordedList = new();
        private TextLayoutMetrics metrics;
        private float maxWidth;
        private float maxHeight;

        private readonly List<LineMetrics> lines = [];
        private readonly List<CharacterMetrics> characterMetrics = [];

        public TextLayout(string text, TextFormat format, float maxWidth, float maxHeight)
        {
            format.AddRef();
            this.text = text;
            this.format = format;
            this.maxWidth = maxWidth;
            this.maxHeight = maxHeight;
            UpdateLayout();
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
            characterMetrics.Clear();

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

            metrics = new(minWidth, minHeight, maxWidth, maxHeight, lineHeight, lines);

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

            TextRange span = new(text, 0, 0);
            uint previous = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\r')
                {
                    previous = 0;
                    characterMetrics.Add(new(text, i, 0));
                    continue;
                }

                if (c == '\n')
                {
                    if (EmitLine(ref penX, ref penY, lineHeight, ref span, i))
                    {
                        return;
                    }

                    previous = 0;
                    characterMetrics.Add(new(text, i, 0));
                    continue;
                }

                if (c == '\t')
                {
                    float nextPosition = ((int)(penX / incrementalTabStop) + 1) * incrementalTabStop;
                    characterMetrics.Add(new(text, i, nextPosition - penX));
                    penX = nextPosition;

                    continue;
                }

                GlyphMetrics metrics = font.GetMetrics(c);

                if (previous != 0 && metrics.Glyph != 0)
                {
                    if (font.GetKerning(previous, metrics.Glyph, out Vector2 kerning))
                    {
                        float k = kerning.X / emSize * fontSize;
                        penX += k;
                        if (i > 0)
                        {
                            var met = characterMetrics[i - 1];
                            met.Width += k;
                            characterMetrics[i - 1] = met;
                        }
                    }
                }

                previous = metrics.Glyph;

                float nextPositionX = penX + metrics.HorizontalAdvance / emSize * fontSize;

                characterMetrics.Add(new(text, i, nextPositionX - penX));

                if (nextPositionX > maxWidth)
                {
                    if (wordWrapping == WordWrapping.NoWrap)
                    {
                        EmitLine(ref penX, ref penY, lineHeight, ref span, i--);
                        return;
                    }

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
                        previous = 0;
                        continue;
                    }

                    if (EmitLine(ref penX, ref penY, lineHeight, ref span, i--))
                    {
                        return;
                    }
                    previous = 0;
                    continue;
                }

                penX = nextPositionX;
            }

            if (span.Start <= text.Length)
            {
                span.Length = text.Length - span.Start;
                AddLine(new(0, penY), span);
            }
        }

        private void AddLine(Vector2 origin, TextRange span)
        {
            Vector2 size = Font.MeasureSize(span, Format.FontSize, Format.IncrementalTabStop);
            LineMetrics line = new(span, origin, size);
            lines.Add(line);
        }

        private bool EmitLine(ref float penX, ref float penY, float lineHeight, ref TextRange span, int i)
        {
            // adjust forward to account \n too.
            span.Length = i - span.Start + 1;
            AddLine(new(0, penY), span);
            span.Start = i + 1;

            penX = 0;
            penY += lineHeight;

            return penY > maxHeight;
        }

        private void AddGlyphRun(IFont font, LineMetrics span, FlowDirection flowDirection, ReadingDirection readingDirection, TextAlignment alignment, float incrementalTabStop, float whitespaceWidth)
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

        public Vector2 GetCursorPosition(int index)
        {
            if (index == -1)
            {
                return default;
            }

            float emSize = Font.EmSize;
            float fontSize = Format.FontSize;
            float lineHeight = Font.GetLineHeight(Format.FontSize);
            float lineSpacing = Format.LineSpacing;

            lineHeight += lineSpacing / emSize * fontSize;

            float penX = 0;
            float penY = 0;

            int selectedLineMetricsIndex = 0;
            LineMetrics selectedLineMetrics = default;
            for (int i = 0; i < metrics.LineMetrics.Count; i++)
            {
                var lineMetrics = metrics.LineMetrics[i];
                if (lineMetrics.Text.Start <= index && lineMetrics.Text.End >= index)
                {
                    selectedLineMetricsIndex = i;
                    selectedLineMetrics = lineMetrics;
                    break;
                }
                penY += lineHeight;
            }

            if (selectedLineMetrics.Length == 0)
            {
                return default;
            }

            var textSpan = selectedLineMetrics.Text.AsSpan();
            var column = index - selectedLineMetrics.Text.Start;
            if (column > 1 && textSpan[column - 1] == '\n')
            {
                selectedLineMetrics = metrics.LineMetrics[selectedLineMetricsIndex + 1];
                penY += lineHeight;
            }

            for (int i = selectedLineMetrics.Text.Start; i < selectedLineMetrics.Text.End; i++)
            {
                if (i == index)
                {
                    break;
                }

                penX += characterMetrics[i].Width;
            }

            return new(penX, penY);
        }

        public int HitTest(Vector2 position)
        {
            float emSize = Font.EmSize;
            float fontSize = Format.FontSize;
            float lineHeight = Font.GetLineHeight(Format.FontSize);
            float lineSpacing = Format.LineSpacing;

            lineHeight += lineSpacing / emSize * fontSize;

            int lineIndex = (int)MathF.Floor(position.Y / lineHeight);

            if (lineIndex < 0 || lineIndex >= metrics.LineMetrics.Count)
            {
                return text.Length;
            }

            var line = metrics.LineMetrics[lineIndex];

            float penX = 0;

            for (int i = line.Text.Start; i < line.Text.End; i++)
            {
                float nextPositionX = penX + characterMetrics[i].Width;

                float last = penX;

                penX = nextPositionX;

                if (penX > position.X)
                {
                    if (Math.Abs(penX - position.X) < Math.Abs(last - position.X))
                    {
                        return i + 1;
                    }

                    return i;
                }
            }

            return text.Length;
        }

        protected override void DisposeCore()
        {
            preRecordedList.Dispose();
            format.Dispose();
        }
    }
}