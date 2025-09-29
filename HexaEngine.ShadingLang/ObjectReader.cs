namespace HexaEngine.ShadingLang
{
    using HexaEngine.ShadingLang.Text;
    using System;
    using System.Runtime.CompilerServices;

    public ref struct ObjectReader
    {
        private ref TokenStream stream;
        private bool first = true;
        private ParserStack stack;
        private int current;

        [InlineArray(64)]
        private struct ParserStack
        {
            public bool First;
        }

        public ObjectReader(ref TokenStream stream)
        {
            this.stream = ref stream;
        }

        public void PushStack()
        {
            if (current >= 64)
            {
                throw new Exception("Parser stack overflow: too many nested blocks.");
            }

            stack[current] = first;
            current++;
            first = true;
        }
        public void PopStack()
        {
            current--;
            first = stack[current];
        }

        public bool ParseOuterProperties(out TextSpan identifier)
        {
            if (stream.IsEndOfTokens)
            {
                first = true;
                identifier = default;
                return false;
            }

            if (!first)
            {
                stream.ExpectDelimiter(',');
            }
            first = false;
            stream.ExpectIdentifier(out identifier);
            stream.ExpectDelimiter(':');
            return true;
        }

        public void BeginObject()
        {
            PushStack();
            stream.ExpectDelimiter('{');
        }

        public void EndObject()
        {
            PopStack();
        }

        public bool ParseProperties(out TextSpan identifier)
        {
            if (stream.TryGetDelimiter('}'))
            {
                first = true;
                identifier = default;
                return false;
            }

            if (!first)
            {
                throw new InvalidOperationException("Expected \"'\" delimiter.");
            }
            first = false;
            stream.ExpectIdentifier(out identifier);
            stream.ExpectDelimiter(':');
            return true;
        }

        public void EndProperty()
        {
            first = stream.TryGetDelimiter(',');
        }

        public string ParseString()
        {
            stream.ExpectLiteral(out var textSpan);
            return textSpan.ToString();
        }

        public string ParseCodeblock()
        {
            stream.ExpectCodeblock(out var code);
            return code.ToString();
        }

        public int ParseInt()
        {
            stream.ExpectLiteral(out var textSpan);
            return int.Parse(textSpan.AsSpan());
        }

        public float ParseFloat()
        {
            stream.ExpectLiteral(out var textSpan);
            return float.Parse(textSpan.AsSpan());
        }

        public bool ParseBool()
        {
            stream.ExpectLiteral(out var textSpan);
            return bool.Parse(textSpan.AsSpan());
        }

        public object ParseEnum(Type type)
        {
            stream.ExpectLiteral(out var textSpan);
            return Enum.Parse(type, textSpan.AsSpan());
        }

        public T ParseEnum<T>() where T : struct, Enum
        {
            stream.ExpectLiteral(out var textSpan);
            return Enum.Parse<T>(textSpan.AsSpan());
        }

        public void BeginArray()
        {
            PushStack();
            stream.ExpectDelimiter('[');
        }

        public void EndArray()
        {
            PopStack();
        }

        public bool ParseArray(out TextSpan identifier)
        {
            if (stream.TryGetDelimiter(']'))
            {
                first = true;
                identifier = default;
                return false;
            }

            if (!first)
            {
                throw new InvalidOperationException("Expected \"'\" delimiter.");
            }
            first = false;
            stream.ExpectIdentifier(out identifier);
            stream.ExpectDelimiter(':');
            return true;
        }

        public void EndArrayItem()
        {
            first = stream.TryGetDelimiter(',');
        }

        public bool ParseArray()
        {
            if (stream.TryGetDelimiter(']'))
            {
                first = true;
                return false;
            }

            if (!first)
            {
                stream.ExpectDelimiter(',');
            }
            first = false;
            return true;
        }
    }
}