namespace HexaEngine.Editor.Packaging
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO;
    using HexaEngine.Editor.MaterialEditor.Generator.Structs;
    using HexaEngine.Mathematics;
    using Microsoft.CodeAnalysis;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Text;

    public struct PackageHeader
    {
        public static readonly byte[] MagicNumber = [0x54, 0x72, 0x61, 0x6E, 0x73, 0x50, 0x61, 0x63, 0x6B, 0x61, 0x67, 0x65, 0x00];
        public static readonly Version Version = new(1, 0, 0, 0);
        public static readonly Version MinVersion = new(1, 0, 0, 0);

        /// <summary>
        /// The endianness of binary data.
        /// </summary>
        public Endianness Endianness;

        /// <summary>
        /// The encoding used for writing strings.
        /// </summary>
        public Encoding Encoding;

        public long MetadataLength;

        /// <summary>
        /// Reads the header information from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the header from.</param>
        /// <exception cref="InvalidDataException">Thrown if the magic number or version doesn't match.</exception>
        public void Read(Stream stream)
        {
            if (!stream.Compare(MagicNumber))
            {
                throw new InvalidDataException("Magic number mismatch.");
            }

            Endianness = (Endianness)stream.ReadByte();

            if (!stream.CompareVersion(MinVersion, Version, Endianness, out ulong version))
            {
                throw new InvalidDataException($"Version mismatch, file: {version} min: {MinVersion} max: {Version}");
            }

            Encoding = Encoding.GetEncoding(stream.ReadInt32(Endianness));
            MetadataLength = stream.ReadInt64(Endianness);
        }

        public bool TryRead(Stream stream)
        {
            if (!stream.Compare(MagicNumber))
            {
                return false;
            }

            Endianness = (Endianness)stream.ReadByte();

            if (!stream.CompareVersion(MinVersion, Version, Endianness, out _))
            {
                return false;
            }

            Encoding = Encoding.GetEncoding(stream.ReadInt32(Endianness));
            MetadataLength = stream.ReadInt64(Endianness);
            return true;
        }

        /// <summary>
        /// Writes the header information to a stream.
        /// </summary>
        /// <param name="stream">The stream to write the header to.</param>
        public readonly void Write(Stream stream)
        {
            stream.Write(MagicNumber);
            stream.WriteByte((byte)Endianness);
            stream.WriteUInt64(Version, Endianness);
            stream.WriteInt32(Encoding.CodePage, Endianness);
            stream.WriteInt64(MetadataLength, Endianness);
        }
    }

    public class Package
    {
        public PackageMetadata Metadata { get; set; }

        public List<PackageData> Data { get; set; }

        public static PackageMetadata? ReadMetadata(string file)
        {
            PackageMetadata result;
            FileStream? fs = null;
            try
            {
                fs = File.OpenRead(file);
                result = ReadMetadata(fs);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to read metadata from package ({file})");
                Logger.Log(ex);
                return null;
            }
            finally
            {
                fs?.Close();
            }
            return result;
        }

        public static PackageMetadata? ReadMetadata(Stream stream)
        {
            PackageHeader header = new();
            header.Read(stream);

            PackageMetadata metadata;
            PackageMetadataReader reader = new(stream, true);

            try
            {
                metadata = reader.Parse();
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to read metadata from package.");
                Logger.Log(ex);
                return null;
            }
            finally
            {
                reader.Dispose();
            }

            return metadata;
        }

        public static void Extract(string packageFile, string outputPath)
        {
        }

        public static void Extract(Stream stream, string outputPath)
        {
            PackageHeader header = new();
            header.Read(stream);

            PackageMetadata metadata;
            PackageMetadataReader reader = new(stream, true);

            try
            {
                metadata = reader.Parse();
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to read metadata from package.");
                Logger.Log(ex);
                return;
            }
            finally
            {
                reader.Dispose();
            }

            Extract(stream, header, metadata, outputPath);
        }

        public static void Extract(Stream stream, PackageMetadata metadata, string outputPath)
        {
            PackageHeader header = default;
            header.Read(stream);

            Extract(stream, header, metadata, outputPath);
        }

        public static void Extract(Stream stream, PackageHeader header, PackageMetadata metadata, string outputPath)
        {
            Directory.CreateDirectory(outputPath);

            using (PackageMetadataWriter writer = new(Path.Combine(outputPath, $"{metadata.Id}.hexspec")))
            {
                writer.Write(metadata);
            }

            stream.Position = header.MetadataLength;

            ZipArchive archive = new(stream, ZipArchiveMode.Read);
            archive.ExtractToDirectory(outputPath);
        }
    }
}