namespace HexaEngine.ShadingLang.LexicalAnalysis
{
    using HexaEngine.ShadingLang;
    using HexaEngine.ShadingLang.Text;
    using System.Diagnostics.CodeAnalysis;
    using System.Management;

    public unsafe class Lexer : LogBase
    {
        public static Token TokenizeStep(ref LexerState state, LexerConfig config)
        {
            char* pCurrent = state.Current;
            char current = *pCurrent;
            int i = state.Index;

            if (config.EnableNewline && (current == '\n' || current == '\r'))
            {
                int width = 1;
                char* next = state.Current + 1;
                if (i + 1 < state.Length && *next == '\n')
                {
                    width++;
                }
                state.IndexNext = i + width;
                return new(state.AsTextSpan(i, width), TokenType.NewLine);
            }

            if (config.EnableWhitespace && char.IsWhiteSpace(current))
            {
                int length = TextHelper.CountLeadingWhitespace(pCurrent + 1) + 1;
                state.IndexNext = i + length;
                return new(state.AsTextSpan(i, length), TokenType.Whitespace);
            }
            else
            {
                state.SkipWhitespace();
            }

            if (state.IsEOF)
            {
                return default;
            }

            return TokenizeStepInner(ref state, config);
        }

        private static Token TokenizeStepInner(ref LexerState state, LexerConfig config)
        {
            char* pCurrent = state.Current;
            char current = *pCurrent;
            int i = state.Index;

            if (config.Delimiters.Contains(current))
            {
                if (current == ':')
                {
                    state.TreatIdentiferAsLiteral = config.SpecialParseTreatIdentiferAsLiteral;
                }
                else
                {
                    state.TreatIdentiferAsLiteral = false;
                }
                state.IndexNext += 1;
                return new(state.AsTextSpan(i, 1), TokenType.Delimiter);
            }

            if (config.Keywords.MatchLongestPrefix(state.AsSpan(), out int keyword, out int keywordLength) && keyword != 0)
            {
                int wordLength = state.FindWordBoundary(i + keywordLength);
                if (wordLength == 0)
                {
                    state.IndexNext += keywordLength;
                    return new(state.AsTextSpan(i, keywordLength), TokenType.Keyword, keyword);
                }
            }

            if (config.Operators.MatchLongestPrefix(state.AsSpan(), out int op, out int operatorLength))
            {
                state.IndexNext += operatorLength;
                return new(state.AsTextSpan(i, operatorLength), TokenType.Operator, op);
            }

            if (config.EnableCodeblock && state.MatchPair(current, '<', '!'))
            {
                if (!state.LookAhead(i + 2, "!>", out var trackedLength))
                {
                    LogError("Inbalanced code block.");
                    return default;
                }
                state.IndexNext += trackedLength + 4;
                return new(state.AsTextSpan(i + 2, trackedLength - 1), TokenType.Codeblock);
            }

            if (state.MatchPair(current, '/', '/'))
            {
                int lineCommentLen = state.FindEndOfLine(i);
                state.IndexNext += lineCommentLen + 2;
                return new(state.AsTextSpan(i, lineCommentLen), TokenType.Comment);
            }

            if (state.MatchPair(current, '/', '*'))
            {
                if (!state.LookAhead(i + 2, "*/", out var trackedLength))
                {
                    LogError("Inbalanced comment block.");
                    return default;
                }
                state.IndexNext += trackedLength + 4;
                return new(state.AsTextSpan(i, trackedLength + 3), TokenType.Comment);
            }

            if (current == '"')
            {
                if (!state.LookAhead(i + 1, '"', out var trackedLength))
                {
                    LogError("Inbalanced literal.");
                    return default;
                }
                state.IndexNext += trackedLength + 2;
                return new(state.AsTextSpan(i + 1, trackedLength), TokenType.Literal);
            }

            if (state.TryParseIdentifier(out var identifierLength))
            {
                state.IndexNext += identifierLength;
                return new(state.AsTextSpan(i, identifierLength), state.TreatIdentiferAsLiteral ? TokenType.Literal : TokenType.Identifier);
            }

            if (char.IsDigit(current))
            {
                int wordLength = state.FindWordBoundary(i + 1);
                state.IndexNext += wordLength + 1;
                return new(state.AsTextSpan(i, wordLength + 1), TokenType.Numeric);
            }

            LogError("Unknown token.");
            return default;
        }

        [DoesNotReturn]
        private new static void LogError(string str)
        {
            throw new Exception(str);
        }
    }
}