﻿namespace HexaEngine.Dotnet
{
    using System.Diagnostics;
    using System.Xml;
    using System.Xml.Serialization;

    public static class Dotnet
    {
        private static string Execute(string parameters)
        {
            ProcessStartInfo psi = new("dotnet", parameters);
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            Process? p = Process.Start(psi);
            string? output = p?.StandardOutput.ReadToEnd();
            p?.WaitForExit();
            string result = "dotnet " + parameters + Environment.NewLine + output;
            Debug.WriteLine(result);
            return result;
        }

        public static string New(DotnetTemplate template, string outputPath)
        {
            string parameter = $"new {template.ToString().ToLower()} -o \"{outputPath}\"";
            return Execute(parameter);
        }

        public static string New(DotnetTemplate template, string outputPath, string name)
        {
            string parameter = $"new {template.ToString().ToLower()} -o \"{outputPath}\" -n \"{name}\"";
            return Execute(parameter);
        }

        public static string Sln(SlnCommand command, string solutionPath, string projectPath)
        {
            string parameter = $"sln \"{solutionPath}\" {command.ToString().ToLower()} \"{projectPath}\"";
            return Execute(parameter);
        }

        public static string AddDlls(string projectPath, IEnumerable<string> dllsPaths)
        {
            XmlDocument document = new();
            document.LoadXml(File.ReadAllText(projectPath));
            XmlNode? root = document?.DocumentElement;

            ItemGroup group = new();
            foreach (string dll in dllsPaths)
            {
                Reference reference = new();
                reference.Include = Path.GetFileNameWithoutExtension(dll);
                reference.HintPath = dll;
                group.Reference.Add(reference);
            }

            XmlSerializerNamespaces ns = new();
            XmlDocument xmlDocument = new();
            XmlSerializer serializer = new(typeof(ItemGroup));
            XmlWriter? writer = xmlDocument?.CreateNavigator()?.AppendChild();
            if (writer == null) return string.Empty;
            serializer.Serialize(writer, group, ns);
            writer.Flush();
            writer.Close();
            xmlDocument?.DocumentElement?.Attributes.RemoveAll();
            if (xmlDocument == null && xmlDocument?.DocumentElement == null) return string.Empty;
#nullable disable
            XmlNode importedNode = root?.OwnerDocument?.ImportNode(xmlDocument.DocumentElement, true);
#nullable enable
            if (importedNode == null) return string.Empty;
            root?.AppendChild(importedNode);

            document?.Save(projectPath);
            return string.Empty;
        }

        public static string Build(string path)
        {
            return Execute($"build \"{path}\"");
        }

        public static string Build(string path, string outputDirectory)
        {
            return Execute($"build \"{path}\" -o \"{outputDirectory}\" -a AMD64");
        }

        public static string Clean(string path)
        {
            return Execute($"clean \"{path}\"");
        }
    }

    public enum SlnCommand
    {
        Add,
        Remove
    }

    public enum DotnetTemplate
    {
        Sln,
        Classlib
    }
}