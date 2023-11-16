namespace HexaEngine.Editor.TextEditor
{
    using HexaEngine.Core.Unsafes;
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
            syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("Keywords", @"\b(abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|virtual|volatile|void|while)\b", Colors.Blue));

            // Comments
            syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("Comments", @"//.*|/\*[\s\S]*?\*/", Colors.Green));

            // Strings
            syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("Strings", "\".*?\"", Colors.Red));

            // Numbers
            syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("Numbers", @"\b(\d+(\.\d*)?|\.\d+)\b", Colors.Purple));

            // Types
            syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("Types", @"(bool|byte|char|decimal|double|float|int|long|sbyte|short|uint|ulong|ushort|void)", Colors.Orange));

            // Directives (e.g., using, #if, #endif)
            syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("Directives", @"^#[^\r\n]*", Colors.DarkGray));

            // User-defined types (e.g., class names)
            syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("UserTypes", @"\b[A-Z][A-Za-z0-9_]*\b", Colors.Teal));

            // Preprocessor directives (e.g., #define, #if, #endif)
            syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("Preprocessor", @"^#[A-Za-z]+\b", Colors.DarkMagenta));

            // Punctuation
            syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("Punctuation", @"[\(\)\{\}\[\];,]", Colors.Black));

            // Operators
            syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("Operators", @"[\+\-\*/%=&\|<>\^!~\?]", Colors.DarkRed));

            // Attributes
            syntaxHighlightCSharp.Definitions.Add(new SyntaxHighlightDefinition("Attributes", @"\[.*?\]", Colors.Pink));

            syntaxHighlightCSharp.Initialize();

            CSharp = syntaxHighlightCSharp;
        }

        public static SyntaxHighlight CSharp { get; }
    }

    public class SyntaxHighlightDefinition(string name, string pattern, ColorRGBA color)
    {
        public string Name { get; set; } = name;

        public string Pattern { get; set; } = pattern;

        public ColorRGBA Color { get; set; } = color;
    }

    public class SyntaxHighlight(string name, string pattern)
    {
        private readonly List<Regex> regexes = [];

        public string Name { get; set; } = name;

        public string Pattern { get; set; } = pattern;

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

        public unsafe void Analyse(StdString* text, List<TextSpan> spans)
        {
            spans.Clear();
            var wText = text->ToWString();
            var span = wText.AsSpan();
            for (int i = 0; i < regexes.Count; i++)
            {
                var def = Definitions[i];
                var col = def.Color;
                var regex = regexes[i];

                foreach (var match in regex.EnumerateMatches(span))
                {
                    var idx = match.Index;
                    var len = match.Length;

                    TextSpan textSpan = new(text, col, idx, len);
                    spans.Add(textSpan);
                }
            }

            spans.Sort(TextSpanStartComparer.Instance);

            wText.Release();
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

    public unsafe struct TextSpan
    {
        public StdString* String;
        public int Start;
        public int End;
        public ColorRGBA Color;
        public bool HasColor;

        public TextSpan(StdString* str, ColorRGBA color, int start, int length)
        {
            String = str;
            Start = start;
            End = start + length;
            Color = color;
            HasColor = true;
        }

        public TextSpan(StdString* str, int start, int length)
        {
            String = str;
            Start = start;
            End = start + length;
            HasColor = false;
        }

        public readonly int Length => End - Start;

        public readonly byte* Data => String->Data + Start;

        public readonly ReadOnlySpan<byte> AsReadOnlySpan()
        {
            return new ReadOnlySpan<byte>(String->Data + Start, Length);
        }

        public readonly Span<byte> AsSpan()
        {
            return new Span<byte>(String->Data + Start, Length);
        }
    }
}