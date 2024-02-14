namespace HexaEngine.Editor.Projects
{
    using System.Collections.Specialized;
    using System.IO;
    using System.Xml.Serialization;

    public class HexaDirectory : HexaParent
    {
#pragma warning disable CS0169 // The field 'HexaDirectory.isSelected' is never used
        private bool isSelected;
#pragma warning restore CS0169 // The field 'HexaDirectory.isSelected' is never used

        public HexaDirectory()
        {
        }

        public HexaDirectory(string name, HexaParent parent)
        {
            Name = name;
            Parent = parent;
            Directory.CreateDirectory(GetAbsolutePath());
        }

        [XmlIgnore]
        public override nint Icon => nint.Zero;

#pragma warning disable CS0067 // The event 'HexaDirectory.CollectionChanged' is never used
        public override event NotifyCollectionChangedEventHandler? CollectionChanged;
#pragma warning restore CS0067 // The event 'HexaDirectory.CollectionChanged' is never used

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
            if (item.Parent == null)
            {
                return;
            }

            string oldPath = item.GetAbsolutePath();
            string newPath = GetAbsolutePath(item.Name);
            File.Move(oldPath, newPath, true);
            item.Parent.Items.Remove(item);
            item.Parent = this;
            Items.Add(item);
            Save();
        }

        public override void Delete()
        {
            Parent?.Items.Remove(this);
            Save();
            Directory.Delete(GetAbsolutePath(), true);
        }

        public override void Rename(string newName)
        {
            string oldPath = GetAbsolutePath();
            string? newPath = Parent?.GetAbsolutePath(newName);
            if (newPath == null)
            {
                return;
            }

            Directory.Move(oldPath, newPath);
            Name = newName;
            Save();
        }

        public override string GetAbsolutePath(string path)
        {
            return Path.Combine(GetAbsolutePath(), path);
        }

        public override string GetAbsolutePath()
        {
            return Parent?.GetAbsolutePath(Name) ?? string.Empty;
        }

        public override void Save()
        {
            FindRoot<HexaProject>()?.Save();
        }

        internal override void BuildParentTree()
        {
            foreach (var item in Items)
            {
                item.Parent = this;
                if (item is HexaParent parent)
                {
                    parent.BuildParentTree();
                }
            }
        }

        public override T? FindRoot<T>() where T : class
        {
            if (this is T t)
            {
                return t;
            }
            else if (Parent is not null)
            {
                return Parent.FindRoot<T>();
            }

            return null;
        }
    }
}