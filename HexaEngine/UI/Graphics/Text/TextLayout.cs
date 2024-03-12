namespace HexaEngine.UI.Graphics.Text
{
    using System.Numerics;

    public struct TextLayoutMetrics
    {
        public float Top;
        public float Left;
        public float Bottom;
        public float Right;

        public TextLayoutMetrics(float top, float left, float bottom, float right)
        {
            Top = top;
            Left = left;
            Bottom = bottom;
            Right = right;
        }

        public TextLayoutMetrics(Vector2 origin, Vector2 size)
        {
            Top = origin.Y;
            Left = origin.X;
            Bottom = origin.Y + size.Y;
            Right = origin.X + size.X;
        }

        public readonly Vector2 Origin => new(Left, Top);

        public readonly Vector2 Size => new Vector2(Right, Bottom) - Origin;

        public readonly float Width => Right - Left;

        public readonly float Height => Bottom - Top;

        public void Merge(TextLayoutMetrics other)
        {
            Top = Math.Min(Top, other.Top);
            Left = Math.Min(Left, other.Left);
            Bottom = Math.Max(Bottom, other.Bottom);
            Right = Math.Max(Right, other.Right);
        }
    }

    public class TextLayout : IDisposable
    {
        private string text;
        private readonly UICommandList preRecordedList = new();
        private TextLayoutMetrics metrics;
        private bool disposedValue;
        private float maxWidth;
        private float maxHeight;

        public TextLayout(string text, TextFormat format, float maxWidth, float maxHeight)
        {
            this.text = text;
            Format = format;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
            Compute();
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
                Compute();
            }
        }

        public TextFormat Format { get; }

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
                Compute();
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
                Compute();
            }
        }

        public IFont Font { get => Format.Font; }

        private void Compute()
        {
            preRecordedList.BeginDraw();
            float penX = 0;
            float penY = 0;

            float lineHeight = Font.GetLineHeight(Format.FontSize);

            float emSize = Font.EmSize;
            float fontSize = Format.FontSize;
            float incrementalTabStop = Format.IncrementalTabStop;

            if (Format.FlowDirection == FlowDirection.BottomToTop)
            {
                penY = MaxHeight - lineHeight;
                lineHeight = -lineHeight;
            }

            TextSpan span = new(text, 0, 0);
            uint previous = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    span.Length = i - span.Start;
                    AddGlyphRun(new(0, penY), span);
                    span.Start = i + 1;

                    penX = 0;
                    penY += lineHeight;

                    if (penY > MaxHeight)
                    {
                        // return here max height reached, no more glyph runs possible.
                        preRecordedList.EndDraw();
                        return;
                    }

                    continue;
                }

                if (c == '\t')
                {
                    penX = ((int)(penX / incrementalTabStop) + 1) * incrementalTabStop;
                    continue;
                }

                GlyphMetrics metrics = Font.GetMetrics(c);

                if (previous != 0 && metrics.Glyph != 0)
                {
                    if (Font.GetKerning(previous, metrics.Glyph, out Vector2 kerning))
                    {
                        penX += kerning.X / emSize * fontSize;
                    }
                }

                float nextPositionX = penX + metrics.HorizontalAdvance / emSize * fontSize;

                if (nextPositionX > MaxWidth && Format.WordWrapping != WordWrapping.NoWrap)
                {
                    if (Format.WordWrapping == WordWrapping.WrapWord)
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

                        span.Length = j - span.Start;
                        AddGlyphRun(new(0, penY), span);
                        span.Start = i;

                        penX = 0;
                        penY += lineHeight;

                        if (penY > MaxHeight)
                        {
                            // return here max height reached, no more glyph runs possible.
                            preRecordedList.EndDraw();
                            return;
                        }

                        continue;
                    }

                    span.Length = i - span.Start;
                    AddGlyphRun(new(0, penY), span);
                    span.Start = i;

                    penX = 0;
                    penY += lineHeight;

                    if (penY > MaxHeight)
                    {
                        // return here max height reached, no more glyph runs possible.
                        preRecordedList.EndDraw();
                        return;
                    }

                    continue;
                }

                penX = nextPositionX;
            }

            if (span.Start < text.Length)
            {
                span.Length = text.Length - span.Start;
                AddGlyphRun(new(0, penY), span);
            }

            preRecordedList.EndDraw();
        }

        private void AddGlyphRun(Vector2 origin, TextSpan span)
        {
            if (span.Length == 0)
            {
                return;
            }

            Vector2 size = Font.MeasureSize(span, Format.FontSize, Format.IncrementalTabStop);
            TextLayoutMetrics metrics = new(origin, size);
            this.metrics.Merge(metrics);
            float usedSpace = size.X;

            float whitespaceScale = 1;

            bool rightToLeft = Format.ReadingDirection == ReadingDirection.RightToLeft;

            switch (Format.TextAlignment)
            {
                case TextAlignment.Leading:
                    if (rightToLeft)
                    {
                        origin.X = MaxWidth - usedSpace;
                    }
                    else
                    {
                        origin.X = 0;
                    }
                    break;

                case TextAlignment.Trailing:
                    if (rightToLeft)
                    {
                        origin.X = 0;
                    }
                    else
                    {
                        origin.X = MaxWidth - usedSpace;
                    }
                    break;

                case TextAlignment.Center:
                    origin.X = (MaxWidth - usedSpace) / 2;
                    break;

                case TextAlignment.Justified:
                    float freeSpace = MaxWidth - usedSpace;
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
                        whitespaceScale = freeSpace / (whitespaceCount * GetWhitespaceWidth(Format.FontSize));
                    }

                    break;
            }

            Font.RenderText(preRecordedList, origin, span, Format.FontSize, whitespaceScale, Format.IncrementalTabStop, Format.ReadingDirection, null);
        }

        public float GetWhitespaceWidth(float fontSize)
        {
            return Font.GetMetrics(' ').HorizontalAdvance / Font.EmSize * fontSize;
        }

        public void DrawText(UICommandList commandList)
        {
            commandList.ExecuteCommandList(preRecordedList);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                preRecordedList.Dispose();
                disposedValue = true;
            }
        }

        ~TextLayout()
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