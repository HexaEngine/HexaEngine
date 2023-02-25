namespace HexaEngine.Projects
{
    using System.Collections.Specialized;
    using System.IO;

    public class HexaFile : HexaItem
    {
        public HexaFile()
        {
        }

        public HexaFile(string name, HexaParent parent) : this()
        {
            Name = name;
            Parent = parent;
        }

#pragma warning disable CS0067 // The event 'HexaFile.CollectionChanged' is never used
        public override event NotifyCollectionChangedEventHandler? CollectionChanged;
#pragma warning restore CS0067 // The event 'HexaFile.CollectionChanged' is never used

        public override T? FindRoot<T>() where T : class
        {
            if (this is T t)
                return t;
            else if (Parent is not null)
                return Parent?.FindRoot<T>();
            return null;
        }

        public override void Save()
        {
            FindRoot<HexaProject>()?.Save();
        }

        public override string GetAbsolutePath()
        {
            return Parent?.GetAbsolutePath(Name) ?? string.Empty;
        }

        public override void Delete()
        {
            Parent?.Items.Remove(this);
            Save();
            File.Delete(GetAbsolutePath());
        }

        public override void Rename(string newName)
        {
            if (Parent is null) return;
            string oldPath = GetAbsolutePath();
            string newPath = Parent.GetAbsolutePath(newName);
            File.Move(oldPath, newPath);
            Name = newName;
        }

        public void Open()
        {
        }
    }
}