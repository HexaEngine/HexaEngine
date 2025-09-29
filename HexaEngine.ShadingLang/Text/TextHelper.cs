namespace HexaEngine.ShadingLang.Text
{
    using System;
    using System.Runtime.CompilerServices;
    using static System.Net.Mime.MediaTypeNames;

    public static unsafe class TextHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MatchPair(char* text, int index, int length, char current, char first, char second)
        {
            return current == first && index + 1 < length && *(text + index + 1) == second;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindEndOfLine(char* text, int offset, int length)
        {
            char* current = text + offset;
            char* end = text + length;

            while (current != end)
            {
                char c = *current;
                if (c == '\n' || c == '\r')
                {
                    if (current + 1 != end)
                    {
                        current++;
                        c = *current;
                        if (c != '\n' && c != '\r')
                        {
                            current--;
                        }
                    }
                    return (int)(current - text - offset + 1);
                }
                current++;
            }

            return length - offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindWordBoundary(char* text, int offset, int length)
        {
            char* current = text + offset;
            char* end = text + length;

            while (current != end)
            {
                if (!char.IsLetterOrDigit(*current))
                {
                    return (int)(current - text - offset);
                }
                current++;
            }

            return length - offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LookAhead(char* text, int offset, int length, char target, out int trackedLength)
        {
            char* current = text + offset;
            char* end = text + length;

            bool escaped = false;
            while (current < end)
            {
                char c = *current;
                if (c == target && !escaped)
                {
                    trackedLength = (int)(current - text - offset);
                    return true;
                }
                escaped = false;
                if (c == '\\')
                {
                    escaped = true;
                }
                current++;
            }
            trackedLength = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LookAhead(char* text, int offset, int length, string target, out int trackedLength)
        {
            char* current = text + offset;
            char* end = text + length;

            bool escaped = false;
            int ix = 0;
            while (current < end)
            {
                char c = *current;
                if (c == target[ix] && !escaped)
                {
                    ix++;
                    if (ix == target.Length)
                    {
                        trackedLength = (int)(current - text - offset);
                        return true;
                    }
                }
                else
                {
                    ix = 0;
                }

                escaped = false;
                if (c == '\\')
                {
                    escaped = true;
                }
                current++;
            }
            trackedLength = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith(char* text, int offset, int length, ReadOnlySpan<char> value)
        {
            if (length - offset < value.Length) return false;
            char* current = text + offset;
            fixed (char* pat = value)
            {
                char* end = pat + value.Length;
                char* patCurrent = pat;
                while (patCurrent != end)
                {
                    if (*current != *patCurrent)
                    {
                        return false;
                    }
                    current++;
                    patCurrent++;
                }
            }

            return true;
        }

        public static void SkipLeadingWhitespace(char* text, int* index, int length)
        {
            int idx = *index;
            idx += CountLeadingWhitespace(text + idx, length - idx);
            *index = idx;
        }

        public static void SkipLeadingWhitespace(char** text, int length)
        {
            char* pText = *text;
            pText += CountLeadingWhitespace(pText, length);
            *text = pText;
        }

        public static int CountLeadingWhitespace(char* text, int length)
        {
            char* current = text;
            char* end = text + length;
            while (current != end && char.IsWhiteSpace(*current))
            {
                current++;
            }
            return (int)(current - text);
        }

        public static int CountLeadingWhitespace(char* text)
        {
            char* current = text;
            char c;
            while ((c = *current) != '\0' && char.IsWhiteSpace(c))
            {
                current++;
            }
            return (int)(current - text);
        }

        public static void SkipTrailingWhitespace(char* text, int index, int* length)
        {
            int len = *length;
            len -= CountTrailingWhitespace(text + index, len - index);
            *length = len;
        }

        public static void SkipTrailingWhitespace(char* text, int* length)
        {
            int len = *length;
            len -= CountTrailingWhitespace(text, len);
            *length = len;
        }

        public static int CountTrailingWhitespace(char* text, int length)
        {
            char* current = text + length - 1;
            char* start = text;
            while (current >= start && char.IsWhiteSpace(*current))
            {
                current--;
            }
            return (int)(text + length - 1 - current);
        }

        public static unsafe int ParseIdentifier(char* text, int offset, int length)
        {
            if (!TryParseIdentifier(text, offset, length, out var trackedLength))
            {
                throw new Exception("Invalid identifier.");
            }
            return trackedLength;
        }

        public static unsafe bool TryParseIdentifier(char* text, int offset, int length, out int trackedLength)
        {
            char* current = text + offset;
            char* end = text + length;
            if (current == end || !char.IsLetter(*current) && *current != '_')
            {
                trackedLength = default;
                return false;
            }

            current++;
            while (current != end)
            {
                char c = *current;
                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    break;
                }
                current++;
            }

            trackedLength = (int)(current - (text + offset));
            return true;
        }
    }
}