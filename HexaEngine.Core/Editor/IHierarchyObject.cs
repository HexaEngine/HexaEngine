namespace HexaEngine.Core.Editor
{
    using System.ComponentModel;

    public interface IHierarchyObject : IEditorSelectable, INotifyPropertyChanged, INotifyPropertyChanging
    {
        public IHierarchyObject? Parent { get; }

        public void AddChild(IHierarchyObject selectable);

        public void RemoveChild(IHierarchyObject selectable);

        public void Initialize();

        public void Uninitialize();
    }
}