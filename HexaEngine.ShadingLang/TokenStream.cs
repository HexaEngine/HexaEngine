namespace HexaEngine.ShadingLang
{
    using HexaEngine.ShadingLang.LexicalAnalysis;
    using HexaEngine.ShadingLang.Text;
    using System;
    using System.Runtime.CompilerServices;

    public struct TokenStream
    {
        private LexerState lexerState;
        private Token lastToken;
        private Token currentToken;
        private readonly LexerConfig config;
        private StateStack stack;
        private int currentStack;

        private struct TokenStreamState
        {
            public LexerState LexerState;
            public Token LastToken;
            public Token CurrentToken;

            public TokenStreamState(LexerState lexerState, Token lastToken, Token currentToken)
            {
                LexerState = lexerState;
                LastToken = lastToken;
                CurrentToken = currentToken;
            }
        }

        [InlineArray(64)]
        private struct StateStack
        {
            private TokenStreamState state;
        }

        public TokenStream(LexerState lexerState, LexerConfig config)
        {
            this.lexerState = lexerState;
            this.config = config;
        }

        public readonly Token LastToken => lastToken;

        public readonly Token Current => currentToken;

        public readonly bool IsEndOfTokens => lexerState.IsEOF;

        public void PushState()
        {
            stack[currentStack] = new TokenStreamState(lexerState, lastToken, currentToken);
            currentStack++;
        }

        public void PopState(bool restore = true)
        {
            currentStack--;
            if (restore)
            {
                var state = stack[currentStack];
                lexerState = state.LexerState;
                lastToken = state.LastToken;
                currentToken = state.CurrentToken;
            }
        }

        public void Advance()
        {
            lastToken = currentToken;
            if (IsEndOfTokens)
            {
                throw new InvalidOperationException("Unexpected end of stream.");
            }
            do
            {
                currentToken = Lexer.TokenizeStep(ref lexerState, config);
                lexerState.Advance();
            } while (currentToken.Type == TokenType.Comment);
        }

        public bool TryAdvance()
        {
            lastToken = currentToken;
            do
            {
                if (IsEndOfTokens)
                {
                    return false;
                }

                currentToken = Lexer.TokenizeStep(ref lexerState, config);
                lexerState.Advance();
            } while (currentToken.Type == TokenType.Comment);
            return true;
        }

        public bool TryGetDelimiter(char delimiter)
        {
            if (!TryGetToken(TokenType.Delimiter, out var current))
            {
                return false;
            }
            if (current.Span[0] == delimiter)
            {
                TryAdvance();
                return true;
            }
            return false;
        }

        public unsafe bool TryGetKeyword<T>(T keyword) where T : unmanaged, Enum
        {
            return TryGetKeyword(*(int*)&keyword);
        }

        public bool TryGetKeyword(int keyword)
        {
            return TryGetTypeValue(TokenType.Keyword, keyword);
        }

        public unsafe bool TryGetOperator<T>(T op) where T : unmanaged, Enum
        {
            return TryGetOperator(*(int*)&op);
        }

        public bool TryGetOperator(int op)
        {
            return TryGetTypeValue(TokenType.Operator, op);
        }

        public bool TryGetIdentifier(out TextSpan span)
        {
            if (TryGetToken(TokenType.Identifier, out var current))
            {
                TryAdvance();
                span = current.Span;
                return true;
            }

            span = default;
            return false;
        }

        public bool TryGetTypeValue(TokenType type, int value)
        {
            if (!TryGetToken(type, out var current))
            {
                return false;
            }
            if (current.Value == value)
            {
                TryAdvance();
                return true;
            }
            return false;
        }

        public readonly bool TryGetToken(TokenType type, out Token current)
        {
            current = Current;
            return current.Type == type;
        }

        public Token Expect(TokenType type)
        {
            var current = Current;
            if (current.Type != type) throw new InvalidOperationException(FormatMessage($"Unexpected token, expected an '{type}'", current));
            Advance();
            return current;
        }

        public void ExpectLiteral(out TextSpan literal)
        {
            var token = Expect(TokenType.Literal);
            literal = token.Span;
        }

        public void ExpectIdentifier(out TextSpan identifier)
        {
            var token = Expect(TokenType.Identifier);
            identifier = token.Span;
        }

        public TextSpan ExpectIdentifier()
        {
            var token = Expect(TokenType.Identifier);
            return token.Span;
        }

        public void ExpectCodeblock(out TextSpan literal)
        {
            var token = Expect(TokenType.Codeblock);
            literal = token.Span;
        }

        public Token ExpectDelimiter(char delimiter)
        {
            var token = Expect(TokenType.Delimiter);
            if (token.Span[0] != delimiter) throw new InvalidOperationException(FormatMessage($"Unexpected delimiter, expected an '{delimiter}'", token));
            return token;
        }

        public static string FormatMessage(string message, Token token)
        {
            return $"{message}, in {token.Span.Start}-{token.Span.Length + token.Span.Start}";
        }
    }
}