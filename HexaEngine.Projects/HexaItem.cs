namespace HexaEngine.Editor.Projects
{
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Xml.Serialization;

    public abstract class HexaItem : INotifyCollectionChanged
    {
        private bool isSelected;

        [XmlIgnore]
        public HexaParent? Parent { get; set; }

        [XmlAttribute]
        public string Name { get; set; } = string.Empty;

        [XmlIgnore]
        public virtual IntPtr Icon { get; }

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
                        root.SelectedItem = this;
                }
            }
        }

        [XmlIgnore]
        public bool IsExpanded { get; set; }

        public abstract event NotifyCollectionChangedEventHandler? CollectionChanged;

        public abstract string GetAbsolutePath();

        public abstract T? FindRoot<T>() where T : HexaItem;

        public abstract void Save();

        public abstract void Delete();

        public abstract void Rename(string newName);
    }

    [Serializable]
    [XmlInclude(typeof(HexaDirectory))]
    [XmlInclude(typeof(HexaFile))]
    public abstract class HexaParent : HexaItem
    {
        [XmlArrayItem(ElementName = "File", IsNullable = true, Type = typeof(HexaFile))]
        [XmlArrayItem(ElementName = "Folder", IsNullable = true, Type = typeof(HexaDirectory))]
        [XmlArray("Items")]
        public ObservableCollection<HexaItem> Items { get; } = new();

        public abstract void Import(string path);

        public abstract void Remove(HexaItem item);

        public abstract void Add(HexaItem item);

        public abstract void Move(HexaItem item);

        public abstract string GetAbsolutePath(string path);

        internal abstract void BuildParentTree();
    }
}