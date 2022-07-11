namespace AssetsBundler
{
    using CommandLine;
    using System;
    using System.IO;

    internal class Program
    {
        private class Options
        {
            [Option('m', "mode", Required = true, HelpText = "Mode: create or list")]
            public Mode Mode { get; set; }

            [Option('p', "path", Required = true, HelpText = "path to dir")]
            public string Path { get; set; }

            public bool SkipImages { get; set; }
        }

        private enum Mode
        {
            create,
            list,
            gen,
            extract
        }

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                switch (o.Mode)
                {
                    case Mode.create:
                        if (Directory.Exists(o.Path))
                        {
                            AssetBundle.CreateFrom(o.Path);
                        }
                        break;

                    case Mode.list:
                        if (File.Exists(o.Path))
                        {
                            AssetBundle bundle = new(o.Path);
                            foreach (Asset asset in bundle.Assets)
                            {
                                Console.WriteLine(asset.Path);
                            }
                        }
                        break;

                    case Mode.gen:
                        if (Directory.Exists(o.Path))
                        {
                            AssetBundle.GenerateFrom(o.Path);
                        }
                        break;

                    case Mode.extract:
                        if (File.Exists(o.Path))
                        {
                            AssetBundle bundle = new(o.Path);
                            if (Directory.Exists("assets/"))
                                Directory.Delete("assets/", true);
                            Directory.CreateDirectory("assets/");
                            bundle.Extract("assets/");
                        }
                        break;
                }
            });
        }
    }
}