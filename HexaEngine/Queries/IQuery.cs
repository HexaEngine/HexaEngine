namespace HexaEngine.Queries
{
    using HexaEngine.Collections;
    using HexaEngine.Scenes;

    public interface IQuery : IHasFlags<QueryFlags>, IDisposable
    {
        QuerySystem QuerySystem { get; internal set; }

        void OnGameObjectAdded(GameObject gameObject);

        void OnGameObjectRemoved(GameObject gameObject);

        void OnGameObjectComponentAdded(GameObject gameObject, IComponent component);

        void OnGameObjectComponentRemoved(GameObject gameObject, IComponent component);

        void OnGameObjectPropertyChanged(GameObject gameObject, string propertyName);

        void OnGameObjectTransformed(GameObject gameObject);

        void OnGameObjectTagChanged(GameObject gameObject, object? tag);

        void OnGameObjectParentChanged(GameObject gameObject, GameObject? parent);

        void OnGameObjectNameChanged(GameObject gameObject, string name);

        void FlushQuery(IList<GameObject> objects);
    }
}