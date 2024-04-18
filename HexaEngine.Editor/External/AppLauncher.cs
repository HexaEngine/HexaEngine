using System.Text;

namespace HexaEngine.Editor.External
{
    public class AppLauncher
    {

    }


    public interface IArgument
    {
        public string Value { get; set; }

        public void Sanitze()
        {
            Value = new string(Value.Trim().Where(x => char.IsLetterOrDigit(x) || char.IsWhiteSpace(x)).ToArray());
        }

        public void Append(List<string> arguments);
    }

    public struct OptionArgument : IArgument
    {
        public string Value { get; set; }

        public string OptionMark { get; set; }

        public void Sanitze()
        {
            Value = new string(Value.Trim().Where(x => char.IsLetterOrDigit(x) || char.IsWhiteSpace(x)).ToArray());
            switch (OptionMark)
            {
                case "-":
                case "/":
                case "--":
                case "+":
                    break;
                default:
                    OptionMark = "";
                    break;
            }
        }

        public readonly void Append(List<string> arguments)
        {
            arguments.Add($"{OptionMark}{Value}");
        }
    }

    public class ArgumentsParser
    {

        public string Parse(string args, Dictionary<string, string> placeholders)
        {
            StringBuilder sb = new();
            ReadOnlySpan<char> argsSpan = args.AsSpan().Trim();
            for (int i = 0; i < argsSpan.Length; i++)
            {
                char c = argsSpan[i];
                switch (c)
                {
                    case '-':
                    case '/':
                    case '+':
                        CaptureArgumentGroup(sb, ref i, argsSpan);

                        break;
                    case '$':
                        CapturePlaceholderGroup(sb, ref i, argsSpan, placeholders);

                        break;
                    default:
                        CaptureLiteralGroup(sb, ref i, argsSpan);
                        break;
                }

                if (i < argsSpan.Length && argsSpan[i] == ' ')
                {
                    sb.Append(' ');
                }
            }

            return sb.ToString();
        }

        private static void CaptureLiteralGroup(StringBuilder sb, ref int index, ReadOnlySpan<char> argsSpan)
        {
            int start = index;
            bool isExplicitLiteral = argsSpan[start] == '\'' || argsSpan[start] == '\"' || argsSpan[start] == '`';
            while (index < argsSpan.Length)
            {
                if (isExplicitLiteral)
                {
                    if (argsSpan[start] == argsSpan[index] && start != index)
                    {
                        index++;
                        break;
                    }
                }
                else
                {
                    if (char.IsWhiteSpace(argsSpan[index]))
                    {
                        break;
                    }
                }
                index++;
            }

            ReadOnlySpan<char> literal = argsSpan[start..index];

            if (literal.IsEmpty || literal[0] == '`' || isExplicitLiteral && literal[0] != literal[^1])
            {
                index = argsSpan.Length;
                return;
            }

            bool invalidLiteral = false;

            invalidLiteral |= literal.Contains("export", StringComparison.InvariantCultureIgnoreCase);
            invalidLiteral |= literal.Contains("set", StringComparison.InvariantCultureIgnoreCase);

            if (invalidLiteral)
            {
                index = argsSpan.Length;
                return;
            }

            for (int i = 0; i < literal.Length; i++)
            {
                char c = literal[i];

                if (isExplicitLiteral && (i == 0 || i == literal.Length - 1)) // skip check for explicit literal bounds like "" or '' 
                {
                    sb.Append(c);
                    continue;
                }

                if (!char.IsLetterOrDigit(c) && c != '.' && c != '\\' && c != '/')
                {
                    invalidLiteral = true;
                    break;
                }
                else
                {
                    sb.Append(c);
                }
            }

            if (invalidLiteral)
            {
                index = argsSpan.Length;
                return;
            }
        }

        private static void CapturePlaceholderGroup(StringBuilder sb, ref int index, ReadOnlySpan<char> argsSpan, Dictionary<string, string> placeholders)
        {
            index++; // skip $
            int start = index;
            while (index < argsSpan.Length)
            {
                if (char.IsWhiteSpace(argsSpan[index]))
                {
                    break;
                }
                index++;
            }

            string placeholder = argsSpan[start..index].ToString();

            if (!placeholders.TryGetValue(placeholder, out var value))
            {
                index = argsSpan.Length;
                return;
            }

            sb.Append(value);
        }

        private static void CaptureArgumentGroup(StringBuilder sb, ref int index, ReadOnlySpan<char> argsSpan)
        {
            int start = index;
            while (index < argsSpan.Length)
            {
                if (char.IsLetterOrDigit(argsSpan[index]))
                {
                    break;
                }
                index++;
            }

            ReadOnlySpan<char> argMarker = argsSpan[start..index];

            if (!IsKnownArgMarker(argMarker))
            {
                index = argsSpan.Length;
                return;
            }

            int startArg = index;
            while (index < argsSpan.Length)
            {
                if (char.IsWhiteSpace(argsSpan[index]))
                {
                    break;
                }
                index++;
            }

            ReadOnlySpan<char> argument = argsSpan[start..index];


            bool invalidArgument = false;

            for (int i = 0; i < argument.Length; i++)
            {
                char c = argument[i];

                if (i < startArg) // skip argument marker, was already checked previously.
                {
                    sb.Append(c);
                    continue;
                }

                if (!char.IsLetterOrDigit(c))
                {
                    invalidArgument = true;
                    break;
                }
                else
                {
                    sb.Append(c);
                }
            }

            if (invalidArgument)
            {
                index = argsSpan.Length;
                return;
            }
        }

        private static bool IsKnownArgMarker(ReadOnlySpan<char> argMarker)
        {
            if (argMarker.SequenceEqual("-"))
            {
                return true;
            }
            if (argMarker.SequenceEqual("--"))
            {
                return true;
            }
            if (argMarker.SequenceEqual("/"))
            {
                return true;
            }
            if (argMarker.SequenceEqual("+"))
            {
                return true;
            }

            return false;
        }
    }
}