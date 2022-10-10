namespace HexaEngine.Editor.Projects
{
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Text.Json.Serialization;
    using System.Xml.Serialization;

    public abstract class HexaItem : INotifyCollectionChanged
    {
        private bool isSelected;

        [JsonIgnore]
        public HexaParent? Parent { get; set; }

        [JsonInclude]
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public virtual IntPtr Icon { get; }

        [JsonIgnore]
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

        [JsonIgnore]
        public bool IsExpanded { get; set; }

        public abstract event NotifyCollectionChangedEventHandler? CollectionChanged;

        public abstract string GetAbsolutePath();

        public abstract T? FindRoot<T>() where T : HexaItem;

        public abstract void Save();

        public abstract void Delete();

        public abstract void Rename(string newName);
    }

    [Serializable]
    public abstract class HexaParent : HexaItem
    {
        public ObservableCollection<HexaItem> Items { get; } = new();

        public abstract void Import(string path);

        public abstract void Remove(HexaItem item);

        public abstract void Add(HexaItem item);

        public abstract void Move(HexaItem item);

        public abstract string GetAbsolutePath(string path);

        internal abstract void BuildParentTree();
    }
}