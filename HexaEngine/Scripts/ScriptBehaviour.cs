namespace HexaEngine.Scripts
{
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    public class ScriptBehaviour : IScriptBehaviour
    {
        private GameObject gameObject = null!;

        [JsonIgnore]
        public GameObject GameObject { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => gameObject; [MethodImpl(MethodImplOptions.AggressiveInlining)] set => gameObject = value; }

        [JsonIgnore]
        public Scene Scene => gameObject.GetScene();

        [JsonIgnore]
        public Transform Transform => gameObject.Transform;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddChild(GameObject child)
        {
            gameObject.AddChild(child);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveChild(GameObject child)
        {
            return gameObject.RemoveChild(child);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponent(IComponent component)
        {
            gameObject.AddComponent(component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponent<T>() where T : IComponent, new()
        {
            gameObject.AddComponent<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponentSingleton<T>() where T : IComponent, new()
        {
            gameObject.AddComponentSingleton<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponent(IComponent component)
        {
            gameObject.RemoveComponent(component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetDepth(ref int depth)
        {
            gameObject.GetDepth(ref depth);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? FindParent<T>() where T : GameObject
        {
            return gameObject.FindParent<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? FindChild<T>() where T : GameObject
        {
            return gameObject.FindChild<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetComponentFromParent<T>() where T : IComponent
        {
            return gameObject.GetComponentFromParent<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetComponentsFromParents<T>() where T : IComponent
        {
            return gameObject.GetComponentsFromParents<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetComponentFromChild<T>() where T : IComponent
        {
            return gameObject.GetComponentFromChild<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetComponentsFromChilds<T>() where T : IComponent
        {
            return gameObject.GetComponentsFromChilds<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetOrCreateComponent<T>() where T : class, IComponent, new()
        {
            return gameObject.GetOrCreateComponent<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetComponent<T>() where T : IComponent
        {
            return gameObject.GetComponent<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetComponent<T>([NotNullWhen(true)] out T? value) where T : IComponent
        {
            return gameObject.TryGetComponent(out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetComponents<T>() where T : IComponent
        {
            return gameObject.GetComponents<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IComponent> GetComponents(Func<IComponent, bool> selector)
        {
            return gameObject.GetComponents(selector);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetComponents<T>(Func<T, bool> selector) where T : IComponent
        {
            return gameObject.GetComponents(selector);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent<T>() where T : IComponent
        {
            return gameObject.HasComponent<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetComponentsFromTree<T>() where T : IComponent
        {
            return gameObject.GetComponentsFromTree<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> DiscoverComponents<T, TStop>() where T : IComponent where TStop : IComponent
        {
            return gameObject.DiscoverComponents<T, TStop>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetChild<T>() where T : GameObject
        {
            return gameObject.GetChild<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetChild<T>([NotNullWhen(true)] out T? child) where T : GameObject
        {
            return gameObject.TryGetChild(out child);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetChildren<T>() where T : GameObject
        {
            return gameObject.GetChildren<T>();
        }
    }
}