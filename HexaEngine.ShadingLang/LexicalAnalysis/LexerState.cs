namespace HexaEngine.ShadingLang.LexicalAnalysis
{
    using HexaEngine.ShadingLang.Text;
    using System;
    using System.Runtime.CompilerServices;

    public unsafe struct LexerState
    {
        public char* Text;
        public int Length;
        public int Index;
        public int IndexNext;
        public bool TreatIdentiferAsLiteral;

        public LexerState(char* text, int length)
        {
            Text = text;
            Length = length;
        }

        public readonly char* Current => Text + Index;

        public readonly bool IsEOF => Index >= Length;

        public void Advance()
        {
            Index = IndexNext;
        }

        public void SkipWhitespace()
        {
            var index = Index;
            TextHelper.SkipLeadingWhitespace(Text, &index, Length);
            Index = index;
            IndexNext = index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool MatchPair(char current, char first, char second)
        {
            return TextHelper.MatchPair(Text, Index, Length, current, first, second);
        }

        public readonly int FindEndOfLine(int start)
        {
            return TextHelper.FindEndOfLine(Text, start, Length);
        }

        public readonly int FindWordBoundary(int start)
        {
            return TextHelper.FindWordBoundary(Text, start, Length);
        }

        public readonly bool LookAhead(int start, char target, out int trackedLength)
        {
            return TextHelper.LookAhead(Text, start, Length, target, out trackedLength);
        }

        public readonly bool LookAhead(int start, string target, out int trackedLength)
        {
            return TextHelper.LookAhead(Text, start, Length, target, out trackedLength);
        }

        public readonly bool StartsWith(ReadOnlySpan<char> value)
        {
            return TextHelper.StartsWith(Text, Index, Length, value);
        }

        public readonly bool TryParseIdentifier(out int trackedLength)
        {
            return TextHelper.TryParseIdentifier(Text, Index, Length, out trackedLength);
        }

        public readonly TextSpan AsTextSpan(int start, int length)
        {
            return new TextSpan(Text, start, length);
        }

        public readonly ReadOnlySpan<char> AsSpan()
        {
            return new TextSpan(Text, Index, Length);
        }
    }
}