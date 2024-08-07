﻿namespace AssetsBundler
{
    using CommandLine;
    using HexaEngine;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Archives;
    using HexaEngine.Windows;
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.IO.Hashing;
    using System.Security.Cryptography;
    using System.Text;

    internal class Program
    {
        private class Options
        {
            [Option('m', "mode", Required = true, HelpText = "Mode: create or list")]
            public Mode Mode { get; set; }

            [Option('p', "path", Required = true, HelpText = "path to dir")]
            public string Path { get; set; }

            [Option('o', "output", Required = false, HelpText = "out dir")]
            public string Output { get; set; }

            [Option('c', "Compression", Required = false, HelpText = "Compression 0 = none, 1 = deflate")]
            public int Compress { get; set; } = 0;

            [Option('l', "Compression level", Required = false, HelpText = "Compression level")]
            public int CompressionLevel { get; set; } = 0;
        }

        private enum Mode
        {
            create,
            list,
            gen,
            extract
        }

        public struct ProgressDummy : IProgress<float>
        {
            public void Report(float value)
            {
            }
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
                            //AssetArchive.CreateFrom(o.Path, (Compression)o.Compress, (CompressionLevel)o.CompressionLevel);
                        }
                        break;

                    case Mode.list:
                        if (File.Exists(o.Path))
                        {
                            AssetArchive bundle = new(o.Path);
                            Crc32 crc = new();

                            Console.WriteLine($"Archive Entries: {bundle.Assets.Count}, BaseOffset: {bundle.BaseOffset}, Flags: {bundle.Flags}, CRC32: {bundle.CRC32:X8}");
                            foreach (AssetArchiveEntry asset in bundle.Assets)
                            {
                                crc.Append(asset.GetData());
                                Console.WriteLine($"[{asset.Type}] CRC32:{crc.GetCurrentHashAsUInt32():X8}, [{asset.Start}..{asset.Length}] {asset.Name}##{asset.Guid}, {asset.PathInArchive}");
                            }
                        }
                        break;

                    case Mode.gen:
                        Window window = new();
                        Application.Boot(HexaEngine.Core.Graphics.GraphicsBackend.D3D11, HexaEngine.Core.Audio.AudioBackend.Disabled);
                        Platform.Init(window, true);
                        Application.Init();

                        SourceAssetsDatabase.Init(o.Path, new ProgressDummy()).Wait();

                        AssetArchive archive = new();

                        foreach (var artifact in ArtifactDatabase.Artifacts)
                        {
                            archive.AddArtifact(artifact, Path.GetRelativePath(o.Path, artifact.GetSourceMetadata().GetFullPath()));
                        }

                        string name = Path.GetFileName(o.Path);
                        RSA rsa = RSA.Create(4096);
                        archive.Save(Path.Combine(o.Path, $"{name}.bundle"), Compression.LZ4, CompressionLevel.SmallestSize, rsa);

                        Application.Shutdown();
                        break;

                    case Mode.extract:
                        if (File.Exists(o.Path))
                        {
                            AssetArchive bundle = new(o.Path);
                            if (Directory.Exists("assets/"))
                            {
                                Directory.Delete("assets/", true);
                            }

                            Directory.CreateDirectory("assets/");
                            bundle.Extract("assets/");
                        }
                        break;

                    default:
                        throw new InvalidOperationException($"'{o.Mode}' is not supported.");
                }
            });
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new(ba.Length * 2);
            foreach (byte b in ba)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }
    }
}