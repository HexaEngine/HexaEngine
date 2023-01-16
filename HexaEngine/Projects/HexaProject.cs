namespace HexaEngine.Editor.Projects
{
    using System.Collections.Specialized;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    [XmlRoot]
    [XmlInclude(typeof(HexaProject))]
    [XmlInclude(typeof(HexaItem))]
    [XmlInclude(typeof(HexaFile))]
    [XmlInclude(typeof(HexaDirectory))]
    public class HexaProject : HexaParent
    {
        private static XmlSerializer serializer = new(typeof(HexaProject));
        private HexaItem? selectedItem;

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
            HexaProject? result = (HexaProject?)serializer.Deserialize(reader);
            if (result == null) return null;
            result.ProjectFilePath = Path.GetFullPath(path);
            reader.Close();
            reader.Dispose();
            result.BuildParentTree();
            return result;
        }

        [XmlIgnore]
        public string ProjectFilePath { get; private set; } = string.Empty;

        [XmlIgnore]
        public string ProjectFileDirectory => Path.GetDirectoryName(ProjectFilePath) ?? string.Empty;

        [XmlIgnore]
        public override IntPtr Icon { get; }

        public static XmlWriterSettings WriterSettings => new()
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\r\n",
            NewLineHandling = NewLineHandling.Replace
        };

        [XmlIgnore]
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
            serializer.Serialize(writer, this);
            writer.Close();
            writer.Dispose();
        }

        public override void ImportFile(string path)
        {
            string filename = Path.GetFileName(path);
            string newPath = GetAbsolutePath(filename);
            File.Copy(path, newPath, true);
            HexaFile file = new(filename, this);
            Items.Add(file);
        }

        public override void ImportFolder(string path)
        {
            Stack<string> folders = new();
            folders.Push(path);

            while (folders.Count > 0)
            {
                var folder = folders.Pop();
                var hexFolder = CreateFolder(path);
                foreach (string subFolder in Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly))
                {
                    folders.Push(subFolder);
                }

                foreach (string file in Directory.GetFiles(folder, "*", SearchOption.TopDirectoryOnly))
                {
                    hexFolder.ImportFile(file);
                }
            }
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