namespace HexaEngine.Editor.TextEditor
{
    using HexaEngine.Core.Unsafes;
    using System.Globalization;
    using System.Text;

    public unsafe class TextSource
    {
        private StdString* text;
        private NewLineType newLineType;

        public TextSource(string text)
        {
            this.text = AllocT<StdString>();
            *this.text = new(text);
            IsBinary = !IsText(this.text);
            newLineType = GetNewLineType(this.text);
            LineCount = CountLines(this.text);
        }

        public StdString* Text
        {
            get => text;
            set
            {
                text->Release();
                Free(text);
                text = value;
                newLineType = GetNewLineType(text);
                LineCount = CountLines(text);
            }
        }

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public NewLineType NewLineType
        {
            get => newLineType;
            set
            {
                newLineType = value;
                if (value == NewLineType.Mixed)
                {
                    return;
                }
                ConvertNewLineType(text, value);
                LineCount = CountLines(text);
            }
        }

        public bool IsBinary { get; set; }

        public bool Changed { get; set; }

        public int LineCount { get; set; }

        public static bool IsText(StdString* text)
        {
            for (int i = 0; i < text->Size; i++)
            {
                byte c = (*text)[i];
                if (char.GetUnicodeCategory((char)c) == UnicodeCategory.Control)
                {
                    return false;
                }
            }

            return true;
        }

        public static NewLineType GetNewLineType(StdString* text)
        {
            bool crlf = false;
            bool lf = false;
            bool cr = false;

            bool foundFlag = false;

            for (int i = 0; i < text->Size; i++)
            {
                byte c = (*text)[i];
                byte c1 = (*text)[i + 1 == text->Size ? i : i + 1];

                if (c == '\r' && c1 == '\n')
                {
                    if (foundFlag && !crlf)
                    {
                        return NewLineType.Mixed;
                    }
                    crlf = true;
                    foundFlag = true;
                }
                else if (c == '\n')
                {
                    if (foundFlag && !lf)
                    {
                        return NewLineType.Mixed;
                    }
                    lf = true;
                    foundFlag = true;
                }
                else if (c == '\r')
                {
                    if (foundFlag && !cr)
                    {
                        return NewLineType.Mixed;
                    }
                    cr = true;
                    foundFlag = true;
                }
            }

            if (crlf)
            {
                return NewLineType.CRLF;
            }
            if (lf)
            {
                return NewLineType.LF;
            }
            if (cr)
            {
                return NewLineType.CR;
            }

            return NewLineType.Mixed;
        }

        public static void ConvertNewLineType(StdString* text, NewLineType newLineType)
        {
            // don't care just return original, you can't convert it to mixed anyway.
            if (newLineType == NewLineType.Mixed)
            {
                return;
            }

            string separator = newLineType switch
            {
                NewLineType.CRLF => "\r\n",
                NewLineType.LF => "\n",
                NewLineType.CR => "\r",
                _ => throw new NotSupportedException(),
            };
            StringBuilder sb = new();
            for (int i = 0; i < text->Size; i++)
            {
                byte c = (*text)[i];
                byte c1 = (*text)[i + 1 == text->Size ? i : i + 1];

                if (c == '\r' && c1 == '\n' || c == '\n' || c == '\r')
                {
                    sb.Append(separator);
                    if (c1 == '\n')
                    {
                        i++;
                    }
                    continue;
                }

                sb.Append(c);
            }
            return;
        }

        public static int CountLines(StdString* text)
        {
            int lineCount = 1; // Mindestens eine Zeile ist vorhanden, da der Text Inhalt hat.

            foreach (byte c in *text)
            {
                if (c == '\n')
                {
                    lineCount++;
                }
            }

            return lineCount;
        }

        public void Dispose()
        {
            text->Release();
            Free(text);
            text = null;
        }
    }
}