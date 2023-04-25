namespace TestApp
{
    using System.Text;
    using System.Text.RegularExpressions;

    public static partial class SPIRVEnumerationsRewriter
    {
        [GeneratedRegex("enum (.*)")]
        private static partial Regex Scan();

        [GeneratedRegex("\\{(.*)\\}", RegexOptions.Singleline)]
        private static partial Regex Scam();

        public static readonly Regex regex = Scan();
        public static readonly Regex regex1 = Scam();

        public static void Resave()
        {
            var content = File.ReadAllText("SPIRV.Enumerations.cs");
            var names = regex.Matches(content).Select(x => x.Groups[1].Value.Trim()).ToArray();
            var code = regex1.Match(content).Groups[1].Value;

            bool capture = false;
            int idx = 0;
            int start = 0;
            StringBuilder sb = new();
            sb.Append("namespace HexaEngine.SPIRVCross\r\n{\r\n");
            for (int i = 0; i < code.Length; i++)
            {
                char c = code[i];
                if (c == '{')
                {
                    start = i + 1;
                    capture = true;
                }
                if (c == '}' && capture)
                {
                    var name = names[idx];
                    int la2 = 0;
                    int la = 0;
                    for (int j = 0; j < name.Length; j++)
                    {
                        if (char.IsUpper(name[j]))
                        {
                            la2 = la;
                            la = j;
                        }
                    }
                    var nameWithoutSecLastPart = name[..la2];
                    var nameWithoutLastPart = name[..la];
                    var newDef = code[start..i].Replace(name, string.Empty, StringComparison.InvariantCultureIgnoreCase).Replace(nameWithoutLastPart, string.Empty, StringComparison.InvariantCultureIgnoreCase);
                    if (!string.IsNullOrEmpty(nameWithoutSecLastPart))
                        newDef = newDef.Replace(nameWithoutSecLastPart, string.Empty, StringComparison.InvariantCultureIgnoreCase);
                    sb.Append($"    public enum {name}\r\n");
                    sb.Append("    {");
                    sb.Append(newDef);
                    sb.Append("}\r\n");
                    idx++;
                    if (idx != names.Length)
                    {
                        sb.Append("\r\n");
                    }
                    start = 0;
                    capture = false;
                }
            }
            sb.Append('}');

            File.WriteAllText("SPIRV.Enumerations.cs", sb.ToString());
        }
    }
}