namespace HexaEngine.Editor.Packaging
{
    using HexaEngine.Core.IO;
    using System.Collections.Generic;
    using System.Globalization;
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

        public static implicit operator PackageIdentifier(PackageDependency dependency)
        {
            return new(dependency.Id, dependency.Version);
        }
    }

    public class PackageMetadataReader : IDisposable
    {
        private readonly XmlReader xmlReader;
        private readonly bool leaveOpen;

        public PackageMetadataReader(string path, bool leaveOpen)
        {
            this.leaveOpen = leaveOpen;
            xmlReader = XmlReader.Create(File.OpenText(path));
        }

        public PackageMetadataReader(Stream stream, bool leaveOpen)
        {
            this.leaveOpen = leaveOpen;
            xmlReader = XmlReader.Create(stream);
        }

        public PackageMetadataReader(TextReader textReader, bool leaveOpen)
        {
            this.leaveOpen = leaveOpen;
            xmlReader = XmlReader.Create(textReader);
        }

        public void Close()
        {
            Dispose();
        }

        public PackageMetadata Parse()
        {
            PackageMetadata metadata = new();

            xmlReader.Read();

            string? version = null;

            while (xmlReader.MoveToNextAttribute())
            {
                switch (xmlReader.Name)
                {
                    case "version":
                        version = xmlReader.Value;
                        break;
                }
            }

            switch (version)
            {
                case "1.0":
                    Parse1(metadata, xmlReader);
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

        public void Dispose()
        {
            if (!leaveOpen)
            {
                ((IDisposable)xmlReader).Dispose();
            }
        }
    }

    public class PackageMetadataWriter : IDisposable
    {
        private readonly XmlWriter xmlWriter;
        private readonly bool leaveOpen;

        public PackageMetadataWriter(string path)
        {
            leaveOpen = false;
            xmlWriter = XmlWriter.Create(File.CreateText(path));
        }

        public PackageMetadataWriter(Stream stream, bool leaveOpen)
        {
            this.leaveOpen = leaveOpen;
            xmlWriter = XmlWriter.Create(stream);
        }

        public PackageMetadataWriter(TextWriter textWriter, bool leaveOpen)
        {
            this.leaveOpen = leaveOpen;
            xmlWriter = XmlWriter.Create(textWriter);
        }

        public void Write(PackageMetadata metadata)
        {
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("PackageMetadata");

            xmlWriter.WriteStartElement("Id");
            xmlWriter.WriteString(metadata.Id);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Name");
            xmlWriter.WriteString(metadata.Name);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Version");
            xmlWriter.WriteString(metadata.Version.ToString());
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("IsPreRelease");
            xmlWriter.WriteString(metadata.IsPreRelease.ToString());
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("IsDeprecated");
            xmlWriter.WriteString(metadata.IsDeprecated.ToString());
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("DeprecationReason");
            xmlWriter.WriteString(metadata.DeprecationReason);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Description");
            xmlWriter.WriteString(metadata.Description);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Author");
            xmlWriter.WriteString(metadata.Author);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Copyright");
            xmlWriter.WriteString(metadata.Copyright);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("LicenceUrl");
            xmlWriter.WriteString(metadata.LicenceUrl);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Licence");
            xmlWriter.WriteString(metadata.Licence);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("DatePublished");
            xmlWriter.WriteString(metadata.DatePublished.ToString("yyyy-MM-ddThh:mm:ss"));
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("ProjectUrl");
            xmlWriter.WriteString(metadata.ProjectUrl);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Tags");
            xmlWriter.WriteString(string.Join(" ", metadata.Tags));
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Dependencies");
            foreach (var dependency in metadata.Dependencies)
            {
                xmlWriter.WriteStartElement("Dependency");
                xmlWriter.WriteAttributeString("packageId", dependency.Id);
                xmlWriter.WriteAttributeString("version", dependency.Version.ToString());
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("SupportedVersions");
            foreach (var version in metadata.SupportedVersions)
            {
                xmlWriter.WriteStartElement("Version");
                xmlWriter.WriteString(version.ToString());
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            xmlWriter.Flush();
            if (!leaveOpen)
            {
                ((IDisposable)xmlWriter).Dispose();
            }
        }
    }
}