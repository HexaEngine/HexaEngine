namespace FontTool
{
    using CommandLine;

    internal class Program
    {
        private class Options
        {
            [Option('p', "path", Required = true, HelpText = "path to font")]
            public string Path { get; set; }

            [Option('t', "texture", Required = true, HelpText = "path to texture")]
            public string Texture { get; set; }
        }

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                LegacyFontFile legacyFont = new(o.Path, o.Texture);
                legacyFont.Save("output.ff");
            });
        }
    }
}