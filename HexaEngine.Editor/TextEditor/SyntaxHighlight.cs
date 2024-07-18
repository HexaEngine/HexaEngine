namespace Hexa.NET.ImGui
{
}

namespace HexaEngine.Editor.TextEditor
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Editor.TextEditor.Highlight.CSharp;
    using HexaEngine.Editor.Themes;
    using System.Numerics;
    using System.Text;
    using System.Text.RegularExpressions;

    public class SyntaxHighlightCollection
    {
    }

    public static class SyntaxHighlightDefaults
    {
        static SyntaxHighlightDefaults()
        {
            var syntaxHighlightCSharp = new SyntaxHighlight("C#", @".*\.cs");

            // Keywords
            syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("Keywords", @"\b(global|abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|virtual|volatile|void|while|get|set|var)\b", new ColorRGBA(0x569cd6ff)));

            // Comments
            syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("Comments", @"//.*|/\*[\s\S]*?\*/", Colors.Green));

            // Strings
            syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("Strings", "\".*?\"", Colors.Red));

            // Numbers
            syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("Numbers", @"\b(\d+(\.\d*)?|\.\d+)\b", Colors.Purple));

            // Directives (e.g., using, #if, #endif)
            syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("Directives", @"^#[^\r\n]*", Colors.DarkGray));

            // User-defined types (e.g., class names)
            syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("UserTypes", @"\b[A-Z][A-Za-z0-9_]*\b", new ColorRGBA(0x4ec9b0ff)));

            // Preprocessor directives (e.g., #define, #if, #endif)
            syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("Preprocessor", @"^#[A-Za-z]+\b", Colors.DarkMagenta));

            // Punctuation
            //syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("Punctuation", @"[\(\)\{\}\[\];,]", Colors.Wheat));

            // Operators
            syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("Operators", @"[\+\-\*/%=&\|<>\^!~\?]", Colors.Gray));

            // Attributes
            syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("Attributes", @"\[.*?\]", Colors.Pink));

            syntaxHighlightCSharp.Initialize();

            CSharp = new CSharpSyntaxHighlight();
        }

        public static SyntaxHighlight CSharp { get; }
    }

    public class SyntaxHighlightDefinition(string name, string pattern, ColorRGBA color)
    {
        public string Name { get; set; } = name;

        public string Pattern { get; set; } = pattern;

        public ColorRGBA Color { get; set; } = color;
    }

    public class SyntaxHighlight
    {
        private readonly List<Regex> regexes = [];

        public SyntaxHighlight(string name, string pattern)
        {
            Name = name;
            Pattern = pattern;
        }

        public string Name { get; set; }

        public string Pattern { get; set; }

        public List<SyntaxHighlightDefinition> Definitions { get; } = [];

        public void Initialize()
        {
            regexes.Clear();
            for (int i = 0; i < Definitions.Count; i++)
            {
                var definition = Definitions[i];
                Regex regex = new(definition.Pattern, RegexOptions.Compiled);

                regexes.Add(regex);
            }
        }

        private readonly List<TextHighlightSpan> matchSpans = [];

        public unsafe void Analyze(StdWString* text, List<TextSpan> lines, List<TextHighlightSpan> spans, float lineHeight)
        {
            matchSpans.Clear();
            Text = text;

            var span = text->AsSpan();

            FindMatches(text, span);
            RemoveOverlapping();
            FillGaps(text, spans);
            ComputeOrigin(lines, lineHeight, spans);
        }

        private static unsafe void ComputeOrigin(List<TextSpan> lines, float lineHeight, List<TextHighlightSpan> spans)
        {
            for (int i = 0; i < spans.Count; i++)
            {
                var span = spans[i];
                span.Origin = ComputeOrigin(lines, lineHeight, span.Start);
                spans[i] = span;
            }
        }

        private unsafe void FillGaps(StdWString* text, List<TextHighlightSpan> spans)
        {
            spans.Clear();
            int currentPos = 0;
            for (int i = 0; i < matchSpans.Count; i++)
            {
                var match = matchSpans[i];
                if (match.Start != currentPos)
                {
                    SplitLines(text, currentPos, match.Start - currentPos, spans);
                }
                spans.Add(match);
                currentPos = match.End;
            }

            if (currentPos < text->Size)
            {
                SplitLines(text, currentPos, text->Size - currentPos, spans);
            }

            spans.Sort(TextHighlightSpanStartComparer.Instance);
        }

        protected unsafe StdWString* Text { get; private set; }

        protected virtual unsafe void FindMatches(StdWString* text, Span<char> span)
        {
            for (int i = 0; i < regexes.Count; i++)
            {
                var def = Definitions[i];
                var col = def.Color;
                var regex = regexes[i];

                foreach (var match in regex.EnumerateMatches(span))
                {
                    var idx = match.Index;
                    var len = match.Length;
                    EmitSpan(idx, len, col);
                }
            }
        }

        protected unsafe void EmitSpan(int idx, int len, ColorRGBA color)
        {
            TextHighlightSpan textSpan = new(Text, default, color, idx, len);
            matchSpans.Add(textSpan);
        }

        private unsafe void RemoveOverlapping()
        {
            matchSpans.Sort(TextHighlightSpanStartComparer.Instance);

            // remove overlapping.
            int lastMatch = -1;
            int lastIndex = -1;
            int lastLength = 0;

            for (int i = 0; i < matchSpans.Count; i++)
            {
                var currentSpan = matchSpans[i];
                if (currentSpan.Start >= lastIndex + lastLength)
                {
                    lastMatch = i;
                    lastIndex = currentSpan.Start;
                    lastLength = currentSpan.Length;
                }
                else
                {
                    if (currentSpan.Start == lastIndex && currentSpan.Length == lastLength)
                    {
                        matchSpans.RemoveAt(i);
                        i--;
                    }

                    Split(lastMatch, i);
                }
            }
        }

        private unsafe void Split(int lastMatch, int i)
        {
            var span1 = matchSpans[lastMatch];
            var span2 = matchSpans[i];

            int overlapStart = Math.Max(span1.Start, span2.Start);
            int overlapEnd = Math.Min(span1.End, span2.End);

            // Handle the overlap by creating or adjusting spans
            if (span2.Start < span1.Start)
            {
                // Part before the overlapping section
                matchSpans[i] = new TextHighlightSpan(span2.String, default, span2.Start, span1.Start - span2.Start);
                matchSpans.Insert(i + 1, new TextHighlightSpan(span2.String, default, span2.Color, overlapStart, overlapEnd - overlapStart));
            }
            else
            {
                // Overlapping section
                if (overlapStart < overlapEnd)
                {
                    matchSpans.Insert(i, new TextHighlightSpan(span2.String, default, span2.Color, overlapStart, overlapEnd - overlapStart));
                    i++; // Move to the newly inserted span
                }

                // Part after the overlapping section
                if (span2.End > overlapEnd)
                {
                    matchSpans[i] = new TextHighlightSpan(span2.String, default, overlapEnd, span2.End - overlapEnd);
                }
                else
                {
                    matchSpans.RemoveAt(i);
                }
            }
        }

        private unsafe void SplitLines(StdWString* text, int currentPos, int length, List<TextHighlightSpan> outSpans)
        {
            char* pText = text->Data;
            int lineStart = 0;

            for (int index = 0; index < length; index++)
            {
                var c = pText[currentPos + index];
                if (c == '\r' || c == '\n')
                {
                    if (c == '\r' && index + 1 < length && pText[currentPos + index + 1] == '\n')
                    {
                        index++;
                    }

                    TextHighlightSpan textSpan = new(text, default, currentPos + lineStart, index - lineStart);
                    outSpans.Add(textSpan);
                    lineStart = index + 1;
                }
            }

            if (lineStart < length)
            {
                TextHighlightSpan textSpan = new(text, default, currentPos + lineStart, length - lineStart);
                if (textSpan.Length > 0)
                {
                    outSpans.Add(textSpan);
                }
            }
        }

        private static unsafe Vector2 ComputeOrigin(List<TextSpan> lines, float lineHeight, int idx)
        {
            var lineIndex = FindLineIndexOfCharacter(lines, idx);
            var line = lines[lineIndex];
            var characterIndex = FindCharacterIndexInLine(line, idx);
            var width = ImGuiWChar.CalcTextSize(line.Data, line.Data + characterIndex).X;

            var origin = new Vector2(width, lineIndex * lineHeight);
            return origin;
        }

        public static int FindLineIndexOfCharacter(List<TextSpan> lines, int idx)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (line.Start <= idx && line.End > idx)
                {
                    return i;
                }
            }
            return lines.Count - 1;
        }

        public static int FindCharacterIndexInLine(TextSpan line, int idx)
        {
            return idx - line.Start;
        }
    }

    public class TextSpanStartComparer : IComparer<TextSpan>
    {
        public static readonly TextSpanStartComparer Instance = new();

        public int Compare(TextSpan x, TextSpan y)
        {
            if (x.Start > y.Start)
            {
                return 1;
            }
            else if (x.Start < y.Start)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }

    public class TextHighlightSpanStartComparer : IComparer<TextHighlightSpan>
    {
        public static readonly TextHighlightSpanStartComparer Instance = new();

        public int Compare(TextHighlightSpan x, TextHighlightSpan y)
        {
            if (x.Start > y.Start)
            {
                return 1;
            }
            else if (x.Start < y.Start)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }

    public unsafe struct TextHighlightSpan
    {
        public StdWString* String;
        public Vector2 Origin;
        public int Start;
        public int End;
        public float Size;
        public uint Color;
        public bool HasColor;

        public TextHighlightSpan(StdWString* str, Vector2 origin, ColorRGBA color, int start, int length)
        {
            String = str;
            Origin = origin;
            Start = start;
            End = start + length;
            Color = ImGui.ColorConvertFloat4ToU32(color);
            HasColor = true;
        }

        public TextHighlightSpan(StdWString* str, Vector2 origin, int start, int length)
        {
            String = str;
            Origin = origin;
            Start = start;
            End = start + length;
            HasColor = false;
        }

        public readonly int Length => End - Start;

        public readonly char* Data => String->Data + Start;

        public readonly ReadOnlySpan<char> AsReadOnlySpan()
        {
            return new ReadOnlySpan<char>(String->Data + Start, Length);
        }

        public readonly Span<char> AsSpan()
        {
            return new Span<char>(String->Data + Start, Length);
        }

        public override readonly string ToString()
        {
            return $"[{Start}-{End}] {Color:X}, {new string(Data)}";
        }
    }

    public unsafe struct TextSpan
    {
        public StdWString* String;
        public int Start;
        public int End;
        public float Size;

        public TextSpan(StdWString* str, int start, int length)
        {
            String = str;
            Start = start;
            End = start + length;
        }

        public readonly int Length => End - Start;

        public readonly char* Data => String->Data + Start;

        public readonly char* DataEnd => String->Data + End;

        public readonly ReadOnlySpan<char> AsReadOnlySpan()
        {
            return new ReadOnlySpan<char>(String->Data + Start, Length);
        }

        public readonly Span<char> AsSpan()
        {
            return new Span<char>(String->Data + Start, Length);
        }
    }
}