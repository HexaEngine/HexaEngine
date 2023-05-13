namespace TestApp
{
    using System.Text;
    using System.Text.RegularExpressions;

    public static partial class SPIRVCommandsRewriter
    {
        [GeneratedRegex("private static readonly (.*) (.*) = LoadFunction<.*>\\(\\\"(.*)\\\"\\);")]
        private static partial Regex Scan();

        [GeneratedRegex(@"public static (.*) (.*)\((.*)\)\r\n\{\r\n.* (.*)\(.*\);")]
        private static partial Regex Scam();

        private static Regex scam = Scam();

        private static Regex scan = Scan();

        private struct Signature
        {
            public Signature(Match match)
            {
                Name = match.Groups[2].Value.Trim();
                Return = match.Groups[1].Value.Trim();
                Params = match.Groups[3].Value.Trim();
                Reference = match.Groups[4].Value.Trim();
            }

            public string Name;
            public string Return;
            public string Params;
            public string Reference;
        }

        private struct Entrypoint
        {
            public Entrypoint(Match match)
            {
                Name = match.Groups[3].Value.Trim();
                Delegate = match.Groups[1].Value.Trim();
                Field = match.Groups[2].Value.Trim();
            }

            public string Name;
            public string Delegate;
            public string Field;
        }

        private struct ExternMethod
        {
            public Entrypoint Entrypoint;
            public Signature Signature;

            public void Build(StringBuilder builder)
            {
                builder.AppendLine($"[DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = \"{Entrypoint.Name}\")]");
                builder.AppendLine($"public static extern {Signature.Return} {Signature.Name}({Signature.Params});");
                builder.AppendLine();
            }
        }

        public static void Rewrite()
        {
            string content = File.ReadAllText("SPIRV.Commands.cs");
            Entrypoint[] entries = scan.Matches(content).Select(x => new Entrypoint(x)).ToArray();
            Signature[] signatures = scam.Matches(content).Select(x => new Signature(x)).ToArray();
            ExternMethod[] methods = new ExternMethod[signatures.Length];

            for (int i = 0; i < signatures.Length; i++)
            {
                Signature signature = signatures[i];
                Entrypoint entrypoint = default;
                for (int j = 0; j < entries.Length; j++)
                {
                    var entry = entries[j];
                    if (entry.Field == signature.Reference)
                    {
                        entrypoint = entry;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(entrypoint.Name))
                {
                    throw new Exception("Entrypoint not found!");
                }

                methods[i] = new ExternMethod() { Entrypoint = entrypoint, Signature = signature };
            }

            StringBuilder builder = new();

            for (int i = 0; i < methods.Length; i++)
            {
                methods[i].Build(builder);
            }

            File.WriteAllText("SPIRV.Commands.cs", builder.ToString());
        }
    }
}