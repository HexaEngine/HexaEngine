namespace HexaEngine.Materials.Generator
{
    using System.Text;

    public class CodeWriter
    {
        private bool _shouldIndent = true;
        private readonly string[] _indentStrings;
        private string _indentString = "";
        private readonly StringBuilder _writer;

        public int IndentLevel { get; private set; }

        public CodeWriter(StringBuilder stringBuilder)
        {
            _writer = stringBuilder;
            _indentStrings = new string[10];
            for (int i = 0; i < _indentStrings.Length; i++)
            {
                _indentStrings[i] = new string('\t', i);
            }
        }

        public void Dispose()
        {
            EndBlock();
        }

        public void Write(char chr)
        {
            WriteIndented(chr);
        }

        public void Write(string @string)
        {
            WriteIndented(@string);
        }

        public void WriteLine()
        {
            _writer.AppendLine();
            _shouldIndent = true;
        }

        public void WriteLine(string @string)
        {
            WriteIndented(@string);
            _writer.AppendLine();
            _shouldIndent = true;
        }

        public void BeginBlock(string content)
        {
            WriteLine(content);
            WriteLine("{");
            Indent(1);
        }

        public void EndBlock()
        {
            Dedent(1);
            WriteLine("}");
        }

        public void EndBlockSemicolon()
        {
            Dedent(1);
            WriteLine("};");
        }

        public IDisposable PushBlock(string marker = "{") => new CodeBlock(this, marker);

        public IDisposable PushBlockSemicolon(string marker = "{") => new CodeBlockSemicolon(this, marker);

        public void Indent(int count = 1)
        {
            IndentLevel += count;

            if (IndentLevel < _indentStrings.Length)
            {
                _indentString = _indentStrings[IndentLevel];
            }
            else
            {
                _indentString = new string('\t', IndentLevel);
            }
        }

        public void Dedent(int count = 1)
        {
            if (count > IndentLevel)
            {
                throw new ArgumentException("count out of range.", nameof(count));
            }

            IndentLevel -= count;
            if (IndentLevel < _indentStrings.Length)
            {
                _indentString = _indentStrings[IndentLevel];
            }
            else
            {
                _indentString = new string('\t', IndentLevel);
            }
        }

        private void WriteIndented(char chr)
        {
            if (_shouldIndent)
            {
                _writer.Append(_indentString);
                _shouldIndent = false;
            }

            _writer.Append(chr);
        }

        private void WriteIndented(string @string)
        {
            if (_shouldIndent)
            {
                _writer.Append(_indentString);
                _shouldIndent = false;
            }

            _writer.Append(@string);
        }

        private class CodeBlock : IDisposable
        {
            private readonly CodeWriter _writer;

            public CodeBlock(CodeWriter writer, string content)
            {
                _writer = writer;
                _writer.BeginBlock(content);
            }

            public void Dispose()
            {
                _writer.EndBlock();
            }
        }

        private class CodeBlockSemicolon : IDisposable
        {
            private readonly CodeWriter _writer;

            public CodeBlockSemicolon(CodeWriter writer, string content)
            {
                _writer = writer;
                _writer.BeginBlock(content);
            }

            public void Dispose()
            {
                _writer.EndBlockSemicolon();
            }
        }
    }
}