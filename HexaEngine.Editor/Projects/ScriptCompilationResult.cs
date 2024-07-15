namespace HexaEngine.Editor.Projects
{
    public class ScriptCompilationResult
    {
        public ScriptCompilationResult(bool success, List<MSBuildMessage> messages)
        {
            Success = success;
            Messages = messages;
            ErrorCount = messages.Count(x => x.Severity == MSBuildMessageSeverity.Error);
            WarningCount = messages.Count(x => x.Severity == MSBuildMessageSeverity.Warning);
            MessageCount = messages.Count(x => x.Severity == MSBuildMessageSeverity.Message);
            Codes = messages.Select(x => x.Code).Distinct().ToList();
            Projects = messages.Select(x => x.Project).Distinct().ToList();
            Files = messages.Select(x => x.File).Distinct().ToList();
        }

        public static ScriptCompilationResult FromMSBuildLog(string log)
        {
            var messages = MSBuildMessageParser.Parse(log);
            bool failed = log.Contains("FAILED");
            return new ScriptCompilationResult(!failed, messages);
        }

        public bool Success { get; init; }

        public List<MSBuildMessage> Messages { get; } = [];

        public List<string> Codes { get; }

        public List<string> Projects { get; }

        public List<string> Files { get; }

        public int ErrorCount { get; init; }

        public int WarningCount { get; init; }

        public int MessageCount { get; init; }
    }

    public enum MSBuildMessageSeverity
    {
        Error,
        Warning,
        Message,
    }

    public struct MSBuildMessage
    {
        public string Project;
        public string ProjectFile;
        public string File;
        public string FileName;
        public int Line;
        public int Column;
        public int LineEnd;
        public int ColumnEnd;
        public string Location;
        public MSBuildMessageSeverity Severity;
        public string Code;
        public string Description;
    }

    public static class MSBuildMessageParser
    {
        public static List<MSBuildMessage> Parse(string text)
        {
            List<MSBuildMessage> messages = [];
            string projectName = string.Empty;
            int index = 0;
            while (index < text.Length)
            {
                int eol = EndOfLine(text.AsSpan(index));
                var span = text.AsSpan(index, eol);
                index += eol + 1;

                if (span.StartsWith("------ Build started:"))
                {
                    int indexOfProject = span.IndexOf("Project: ");
                    if (indexOfProject == -1)
                    {
                        throw new Exception("Failed to parse project name");
                    }
                    indexOfProject += "Project: ".Length;

                    span = span[indexOfProject..];
                    int end = span.IndexOf(',');
                    projectName = span[..end].ToString();
                }
                else
                {
                    int indexOfMessage = span.IndexOf("):");
                    if (indexOfMessage == -1)
                    {
                        continue;
                    }
                    indexOfMessage++;

                    var fileText = span[..(indexOfMessage)];
                    int indexOfBracket = fileText.IndexOf('(');
                    var locationText = fileText[(indexOfBracket + 1)..][..^1];
                    MSBuildMessage message = new();
                    message.File = fileText[..(indexOfBracket)].ToString();
                    message.FileName = Path.GetFileName(message.File);
                    message.Project = projectName;

                    {
                        var local = locationText;
                        local = ParseInt(local, out message.Line);
                        local = ParseInt(local, out message.Column);
                        local = ParseInt(local, out message.LineEnd);
                        ParseInt(local, out message.ColumnEnd);
                    }
                    message.Location = $"({locationText})";

                    var scdText = span[(indexOfMessage + 1)..];
                    int indexOfDescription = scdText.IndexOf(':');

                    {
                        var scText = scdText[1..indexOfDescription];
                        int indexOfSpace = scText.IndexOf(' ');
                        message.Severity = Enum.Parse<MSBuildMessageSeverity>(scText[..(indexOfSpace)], true);
                        message.Code = scText[(indexOfSpace + 1)..].ToString();
                    }

                    {
                        var descText = scdText[(indexOfDescription + 1)..];
                        while (descText[^1] == '\n' || descText[^1] == '\r')
                        {
                            descText = descText[..^1];
                        }

                        descText = descText.Trim();

                        if (descText.EndsWith("]", StringComparison.InvariantCulture))
                        {
                            var lastIndexOfBracketStart = descText.LastIndexOf('[');
                            var projectFileText = descText[lastIndexOfBracketStart..];
                            var lastIndexOfBracketEnd = projectFileText.LastIndexOf(']');

                            message.ProjectFile = descText.Slice(lastIndexOfBracketStart + 1, lastIndexOfBracketEnd - 1).ToString();
                            message.Project = Path.GetFileNameWithoutExtension(message.ProjectFile);

                            descText = descText[..(lastIndexOfBracketStart - 1)];
                        }

                        message.Description = descText.Trim().ToString();
                    }

                    messages.Add(message);
                }
            }

            return messages;
        }

        private static ReadOnlySpan<char> ParseInt(ReadOnlySpan<char> local, out int result)
        {
            int indexLine = local.IndexOf(',');
            if (indexLine == -1)
            {
                indexLine = local.Length;
                if (!int.TryParse(local[..indexLine], out result))
                {
                    result = -1;
                }

                return [];
            }

            if (!int.TryParse(local[..indexLine], out result))
            {
                result = -1;
            }

            var local1 = local[(indexLine + 1)..];
            return local1;
        }

        public static int EndOfLine(ReadOnlySpan<char> text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    return i;
                }

                if (text[i] == '\r')
                {
                    if (i + 1 < text.Length && text[i + 1] == '\n')
                    {
                        return i + 1;
                    }

                    return i;
                }
            }
            return text.Length - 1;
        }
    }
}