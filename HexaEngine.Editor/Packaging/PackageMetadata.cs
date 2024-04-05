namespace HexaEngine.Editor.Packaging
{
    using HexaEngine.Core.IO;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Xml;

    public class PackageMetadata
    {
        public const string PackageMetadataFormatVersion = "1.0.0.0";

        public string Id { get; set; }

        public string Name { get; set; }

        public Version Version { get; set; }

        public bool IsPreRelease { get; set; }

        public bool IsDeprecated { get; set; }

        public string? DeprecationReason { get; set; }

        public string? Description { get; set; }

        public string? Author { get; set; }

        public string? Copyright { get; set; }

        public string? LicenceUrl { get; set; }

        public string Licence { get; set; }

        public string? ReadmeUrl { get; set; }

        public DateTime DatePublished { get; set; }

        public string? ProjectUrl { get; set; }

        public List<string> Tags { get; } = [];

        public List<PackageDependency> Dependencies { get; } = [];

        public List<Version> SupportedVersions { get; } = [];
    }

    public class PackageDependency(string id, Version version)
    {
        public string Id { get; set; } = id;

        public Version Version { get; set; } = version;
    }

    public class PackageMetadataParser
    {
        public static PackageMetadata ParseFrom(string path)
        {
            return Parse(File.OpenText(path));
        }

        public static PackageMetadata Parse(string text)
        {
            return Parse(new StringReader(text));
        }

        public static PackageMetadata Parse(TextReader textReader)
        {
            return Parse(XmlReader.Create(textReader));
        }

        public static PackageMetadata Parse(XmlReader reader)
        {
            PackageMetadata metadata = new();

            reader.Read();

            string? version = null;

            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "version":
                        version = reader.Value;
                        break;
                }
            }

            switch (version)
            {
                case "1.0":
                    Parse1(metadata, reader);
                    break;

                default:
                    throw new NotSupportedException();
            }
            return metadata;
        }

        private static void Parse1(PackageMetadata metadata, XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }

                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "Id":
                            metadata.Id = reader.ReadElementContentAsString();
                            break;

                        case "Name":
                            metadata.Name = reader.ReadElementContentAsString();
                            break;

                        case "Version":
                            metadata.Version = Version.Parse(reader.ReadElementContentAsString());
                            break;

                        case "IsPreRelease":
                            metadata.IsPreRelease = reader.ReadElementContentAsBoolean();
                            break;

                        case "IsDeprecated":
                            metadata.IsDeprecated = reader.ReadElementContentAsBoolean();
                            break;

                        case "DeprecationReason":
                            metadata.DeprecationReason = reader.ReadElementContentAsString();
                            break;

                        case "Description":
                            metadata.Description = reader.ReadElementContentAsString();
                            break;

                        case "Author":
                            metadata.Author = reader.ReadElementContentAsString();
                            break;

                        case "Copyright":
                            metadata.Copyright = reader.ReadElementContentAsString();
                            break;

                        case "LicenceUrl":
                            metadata.LicenceUrl = reader.ReadElementContentAsString();
                            break;

                        case "Licence":
                            metadata.Licence = reader.ReadElementContentAsString();
                            break;

                        case "DatePublished":
                            metadata.DatePublished = DateTime.ParseExact(reader.ReadElementContentAsString(), "yyyy-MM-ddThh:mm:ss", CultureInfo.InvariantCulture);
                            break;

                        case "ProjectUrl":
                            metadata.ProjectUrl = reader.ReadElementContentAsString();
                            break;

                        case "Tags":
                            metadata.Tags.AddRange(reader.ReadElementContentAsString().Split(" "));
                            break;

                        case "Dependencies":
                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Dependency")
                                {
                                    string? id = null;
                                    Version? version = null;
                                    while (reader.MoveToNextAttribute())
                                    {
                                        switch (reader.Name)
                                        {
                                            case "packageId":
                                                id = reader.Value;
                                                break;

                                            case "version":
                                                version = Version.Parse(reader.Value);
                                                break;
                                        }
                                    }
                                    if (id == null)
                                    {
                                        throw new FormatException();
                                    }

                                    PackageDependency dependency = new(id, version ?? default);
                                    metadata.Dependencies.Add(dependency);
                                }

                                if (reader.NodeType == XmlNodeType.EndElement)
                                {
                                    break;
                                }
                            }
                            break;

                        case "SupportedVersions":
                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Version")
                                {
                                    metadata.SupportedVersions.Add(Version.Parse(reader.ReadElementContentAsString()));
                                }

                                if (reader.NodeType == XmlNodeType.EndElement)
                                {
                                    break;
                                }
                            }
                            break;
                    }
                }
            }
        }
    }
}