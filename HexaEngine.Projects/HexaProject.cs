﻿namespace HexaEngine.Editor.Projects
{
    using System.Collections.Specialized;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Xml;
    using System.Xml.Serialization;

    [JsonSourceGenerationOptions(
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        GenerationMode = JsonSourceGenerationMode.Default,
        IgnoreReadOnlyFields = true,
        IgnoreReadOnlyProperties = true,
        IncludeFields = false,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        WriteIndented = true)]
    [JsonSerializable(typeof(HexaProject), GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(HexaFile), GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(HexaDirectory), GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(HexaItem), GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(HexaParent), GenerationMode = JsonSourceGenerationMode.Default)]
    internal partial class SourceGenerationContext : JsonSerializerContext
    {
    }

    [XmlRoot]
    public class HexaProject : HexaParent
    {
        private HexaItem? selectedItem;
        private bool isSelected;

        public HexaProject()
        {
        }

        public HexaProject(string projectFilePath)
        {
            Name = Path.GetFileNameWithoutExtension(projectFilePath);
            ProjectFilePath = projectFilePath;
        }

        public static HexaProject Create(string path)
        {
            HexaProject project = new(path);
            project.Save();
            return project;
        }

        public static HexaProject? Load(string path)
        {
            if (path is null) return null;
            var reader = File.OpenRead(path);
            HexaProject? result = (HexaProject?)JsonSerializer.Deserialize(reader, typeof(HexaProject), SourceGenerationContext.Default);
            if (result == null) return null;
            result.ProjectFilePath = Path.GetFullPath(path);
            reader.Close();
            reader.Dispose();
            result.BuildParentTree();
            return result;
        }

        [JsonIgnore]
        public string ProjectFilePath { get; private set; } = string.Empty;

        [JsonIgnore]
        public string ProjectFileDirectory => Path.GetDirectoryName(ProjectFilePath) ?? string.Empty;

        [JsonIgnore]
        public override IntPtr Icon { get; }

        public static XmlWriterSettings WriterSettings => new()
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\r\n",
            NewLineHandling = NewLineHandling.Replace
        };

        [JsonIgnore]
        public HexaItem? SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem != null)
                    selectedItem.IsSelected = false;
                selectedItem = value;
            }
        }

        public override event NotifyCollectionChangedEventHandler? CollectionChanged;

        public override T? FindRoot<T>() where T : class
        {
            if (this is T t)
                return t;
            else if (Parent != null)
                return Parent.FindRoot<T>();
            else
                return null;
        }

        public override string GetAbsolutePath(string path)
        {
            return Path.Combine(GetAbsolutePath(), path);
        }

        public override void Save()
        {
            var writer = File.Create(ProjectFilePath);
            JsonSerializer.Serialize(writer, this, typeof(HexaProject), SourceGenerationContext.Default);
            writer.Close();
            writer.Dispose();
        }

        public override void Import(string path)
        {
            string filename = Path.GetFileName(path);
            string newPath = GetAbsolutePath(filename);
            File.Copy(path, newPath, true);
            HexaFile file = new(filename, this);
            Items.Add(file);
        }

        public HexaDirectory CreateFolder(string name)
        {
            HexaDirectory directory = new(name, this);
            Items.Add(directory);
            return directory;
        }

        public override void Add(HexaItem item)
        {
            Items.Add(item);
            Save();
        }

        public override void Remove(HexaItem item)
        {
            Items.Remove(item);
            Save();
        }

        public override void Move(HexaItem item)
        {
            if (item.Parent == null) return;
            string oldPath = item.GetAbsolutePath();
            string newPath = GetAbsolutePath(item.Name);
            File.Move(oldPath, newPath, true);
            item.Parent.Items.Remove(item);
            item.Parent = this;
            Items.Add(item);
            Save();
        }

        internal override void BuildParentTree()
        {
            foreach (var child in Items)
            {
                child.Parent = this;
                if (child is HexaParent parent)
                {
                    parent.BuildParentTree();
                }
            }
        }

        public override string GetAbsolutePath()
        {
            return ProjectFileDirectory;
        }

        public override void Delete()
        {
            foreach (var child in Items)
            {
                child.Delete();
            }
            File.Delete(ProjectFilePath);
        }

        public override void Rename(string newName)
        {
            string oldPath = ProjectFilePath;
            string newPath = Path.Combine(ProjectFileDirectory, newName);
            Name = newName;
            File.Move(oldPath, newPath);
            ProjectFilePath = newPath;
            Save();
        }
    }
}