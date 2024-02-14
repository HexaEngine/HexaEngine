namespace HexaEngine.Editor.Projects
{
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Serialization;

    public abstract class HexaItem : INotifyCollectionChanged
    {
        private bool isSelected;
        private bool isVisible;

        [XmlIgnore]
        public HexaParent? Parent { get; set; }

        [XmlAttribute]
        public string Name { get; set; } = string.Empty;

        [XmlAnyElement]
        public List<object> Properties { get; set; } = new();

        [XmlIgnore]
        public virtual nint Icon { get; }

        [XmlIgnore]
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                if (value)
                {
                    var root = FindRoot<HexaProject>();
                    if (root != null)
                    {
                        root.SelectedItem = this;
                    }
                }
            }
        }

        [XmlIgnore]
        public bool IsExpanded { get; set; }

        [XmlIgnore]
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (isVisible == value)
                {
                    return;
                }

                if (!value && this is HexaParent parent)
                {
                    for (int i = 0; i < parent.Items.Count; i++)
                    {
                        parent.Items[i].IsVisible = false;
                    }
                }

                isVisible = value;
            }
        }

        public abstract event NotifyCollectionChangedEventHandler? CollectionChanged;

        public abstract string GetAbsolutePath();

        public abstract T? FindRoot<T>() where T : HexaItem;

        public abstract void Save();

        public abstract void Delete();

        public abstract void Rename(string newName);

        public T? GetProperty<T>() where T : class
        {
            for (int i = 0; i < Properties.Count; i++)
            {
                var p = Properties[i];
                if (p is T t)
                    return t;
            }
            return default;
        }

        public bool TryGetProperty<T>([NotNullWhen(true)] out T? property) where T : class
        {
            property = GetProperty<T>();
            return property != null;
        }
    }

    [Serializable]
    public abstract class HexaParent : HexaItem
    {
        public ObservableCollection<HexaItem> Items { get; } = new();

        public abstract void ImportFile(string path);

        public abstract void ImportFolder(string path);

        public abstract void Remove(HexaItem item);

        public abstract void Add(HexaItem item);

        public abstract void Move(HexaItem item);

        public abstract string GetAbsolutePath(string path);

        internal abstract void BuildParentTree();
    }
}