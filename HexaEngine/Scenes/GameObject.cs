namespace HexaEngine.Core.Scenes
{
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    public partial class GameObject : EntityNotifyBase
    {
        private static readonly GameObjectSelection selected = new();

        private readonly List<GameObject> children = new();
        private readonly List<IComponent> components = new();
        private Scene? scene;
        private GameObject? parent;
        private bool initialized;
        private bool isEditorVisible;
        private Type? type;
        private bool isEnabled = true;
        private bool isHidden = false;

        public Transform Transform = new();
        private Guid guid = Guid.NewGuid();
        private string name = string.Empty;
        private string? fullName;
        private bool isEditorSelected;

        private GCHandle gcHandle;
        private object? tag;

        [JsonIgnore]
        public readonly IntPtr Pointer;

        public GameObject()
        {
            gcHandle = GCHandle.Alloc(this, GCHandleType.WeakTrackResurrection);
            Pointer = GCHandle.ToIntPtr(gcHandle);
            Transform.Updated += TransformUpdated;
        }

        ~GameObject()
        {
            gcHandle.Free();
        }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid
        {
            get => guid;
            set
            {
                if (SetAndNotifyWithEqualsTest(ref guid, value))
                {
                    fullName = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get => name;
            set
            {
                if (SetAndNotifyWithEqualsTest(ref name, value))
                {
                    fullName = null;
                    OnNameChanged?.Invoke(this, value);
                }
            }
        }

        /// <summary>
        /// Gets the full name of the GameObject, which is a unique combination of its name and Guid.
        /// The full name is lazily generated when accessed for the first time and will be
        /// reinitialized if either the name or Guid property is modified.
        /// </summary>
        /// <value>
        /// The full name of the GameObject.
        /// </value>
        [JsonIgnore]
        public string FullName
        {
            get
            {
                return fullName ??= $"{name}##{guid}";
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled == value)
                {
                    return;
                }

                SetAndNotify(ref isEnabled, value);
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].IsEnabled = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is hidden.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is hidden; otherwise, <c>false</c>.
        /// </value>
        public bool IsHidden
        {
            get => isHidden;
            set => SetAndNotify(ref isHidden, value);
        }

        [JsonIgnore]
        public virtual GameObject? Parent
        {
            get => parent;
            set
            {
                if (SetAndNotifyWithRefEqualsTest(ref parent, value))
                {
                    OnParentChanged?.Invoke(this, value);
                }
            }
        }

        [JsonIgnore]
        public bool Initialized => initialized;

        [JsonIgnore]
        public static GameObjectSelection Selected => selected;

        [JsonIgnore]
        public bool IsEditorSelected => isEditorSelected;

        [JsonIgnore]
        public bool IsEditorOpen { get; set; }

        [JsonIgnore]
        public bool IsEditorVisible
        {
            get => isEditorVisible;
            set
            {
                if (isEditorVisible == value)
                {
                    return;
                }

                isEditorVisible = value;
                if (!value)
                {
                    for (int i = 0; i < Children.Count; i++)
                    {
                        Children[i].IsEditorVisible = false;
                    }
                }
            }
        }

        public virtual List<GameObject> Children => children;

        public virtual List<IComponent> Components => components;

        [JsonIgnore]
        public object? Tag
        {
            get => tag;
            set
            {
                if (SetAndNotifyWithRefEqualsTest(ref tag, value))
                {
                    OnTagChanged?.Invoke(this, value);
                }
            }
        }

        public Type Type => type ??= GetType();

        public event Action<GameObject, string>? OnNameChanged;

        public event Action<GameObject, object?>? OnTagChanged;

        public event Action<GameObject, GameObject?>? OnParentChanged;

        public event Action<GameObject>? OnTransformed;

        public event Action<GameObject, IComponent>? OnComponentAdded;

        public event Action<GameObject, IComponent>? OnComponentRemoved;

        protected void OverwriteTransform(Transform transform)
        {
            Transform.Updated -= TransformUpdated;
            transform.Updated += TransformUpdated;
        }

        protected virtual void TransformUpdated(object? sender, EventArgs e)
        {
            OnTransformed?.Invoke(this);
        }

        public void SendUpdateTransformed()
        {
            OnTransformed?.Invoke(this);
        }

        public void SaveState()
        {
            Transform.SaveState();
            for (int i = 0; i < children.Count; i++)
            {
                children[i].SaveState();
            }
        }

        public void RestoreState()
        {
            Transform.RestoreState();
            for (int i = 0; i < children.Count; i++)
            {
                children[i].RestoreState();
            }
            tag = null;
        }

        public void Merge(GameObject node)
        {
            if (Initialized)
            {
                var childs = node.children.ToArray();
                var comps = node.components.ToArray();
                if (node.initialized)
                {
                    for (int i = 0; i < childs.Length; i++)
                    {
                        GameObject? child = childs[i];
                        node.RemoveChild(child);
                    }

                    for (int i = 0; i < comps.Length; i++)
                    {
                        IComponent? comp = comps[i];
                        node.RemoveComponent(comp);
                    }
                }
                node.Parent?.RemoveChild(node);
                for (int i = 0; i < childs.Length; i++)
                {
                    GameObject? child = childs[i];
                    AddChild(child);
                }

                for (int i = 0; i < comps.Length; i++)
                {
                    IComponent? comp = comps[i];
                    AddComponent(comp);
                }
            }
            else
            {
                children.AddRange(node.children);
                node.children.Clear();
                components.AddRange(node.components);
                node.components.Clear();
            }
        }

        public virtual void AddChild(GameObject node)
        {
            node.parent?.RemoveChild(node);
            node.parent = this;
            children.Add(node);
            if (initialized)
            {
                node.Initialize();
            }
        }

        public virtual void RemoveChild(GameObject node)
        {
            if (initialized)
            {
                node.Uninitialize();
            }
            children.Remove(node);
            node.parent = null;
        }

        public virtual void AddComponent(IComponent component)
        {
            components.Add(component);
            if (initialized)
            {
                component.GameObject = this;
                component.Awake();
            }
            OnComponentAdded?.Invoke(this, component);
        }

        public virtual void RemoveComponent(IComponent component)
        {
            if (initialized)
            {
                component.Destroy();
            }

            components.Remove(component);
            OnComponentRemoved?.Invoke(this, component);
        }

        public virtual void Initialize()
        {
            Transform.Parent = parent?.Transform;
            scene = GetScene();
            scene.RegisterChild(this);

            initialized = true;
            for (int i = 0; i < components.Count; i++)
            {
                var component = components[i];
                component.GameObject = this;
                component.Awake();
            }
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Initialize();
            }
        }

        public virtual void Uninitialize()
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].Destroy();
            }
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Uninitialize();
            }
            scene?.UnregisterChild(this);
            initialized = false;
            scene = null;
            tag = null;
        }

        public virtual Scene GetScene()
        {
            if (scene != null)
            {
                return scene;
            }

            return parent?.GetScene() ?? throw new("Node tree invalid");
        }

        public virtual void BuildReferences()
        {
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                child.parent = this;
                child.BuildReferences();
            }
        }

        public virtual void GetDepth(ref int depth)
        {
            depth++;
            parent?.GetDepth(ref depth);
        }

        public virtual T? GetParentNodeOf<T>() where T : GameObject
        {
            GameObject? current = parent;
            while (true)
            {
                if (current is T t)
                {
                    return t;
                }
                else if (current?.parent is not null)
                {
                    current = current.parent;
                }
                else
                {
                    return null;
                }
            }
        }

        public virtual T GetOrCreateComponent<T>() where T : class, IComponent, new()
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is T t)
                {
                    return t;
                }
            }
            T t1 = new();
            components.Add(t1);
            return t1;
        }

        public virtual T? GetComponent<T>() where T : IComponent
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is T t)
                {
                    return t;
                }
            }
            return default;
        }

        public virtual bool TryGetComponent<T>([NotNullWhen(true)] out T? value) where T : IComponent
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is T t)
                {
                    value = t;
                    return true;
                }
            }
            value = default;
            return false;
        }

        public virtual IEnumerable<T> GetComponents<T>() where T : IComponent
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is T t)
                {
                    yield return t;
                }
            }
        }

        public virtual IEnumerable<IComponent> GetComponents(Func<IComponent, bool> selector)
        {
            for (int i = 0; i < components.Count; i++)
            {
                var component = components[i];
                if (selector(component))
                {
                    yield return component;
                }
            }
        }

        public virtual IEnumerable<T> GetComponents<T>(Func<T, bool> selector) where T : IComponent
        {
            for (int i = 0; i < components.Count; i++)
            {
                var component = components[i];
                if (component is T t && selector(t))
                {
                    yield return t;
                }
            }
        }

        public bool HasComponent<T>() where T : IComponent
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is T)
                    return true;
            }
            return false;
        }

        public virtual IEnumerable<T> GetComponentsFromChilds<T>() where T : IComponent
        {
            List<T> components = new();
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                components.AddRange(child.GetComponents<T>());
                components.AddRange(child.GetComponentsFromChilds<T>());
            }
            return components;
        }

        public static void RemoveComponent(GameObject gameObject, IComponent component)
        {
            gameObject.RemoveComponent(component);
        }

        public static void RemoveComponent(ValueTuple<GameObject, IComponent> values)
        {
            values.Item1.RemoveComponent(values.Item2);
        }

        public virtual T? GetChild<T>() where T : GameObject
        {
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (child is T t)
                {
                    return t;
                }
            }
            return null;
        }

        public virtual bool TryGetChild<T>([NotNullWhen(true)] out T? child) where T : GameObject
        {
            for (int i = 0; i < children.Count; i++)
            {
                var item = children[i];
                if (item is T t)
                {
                    child = t;
                    return true;
                }
            }
            child = default;
            return false;
        }

        public virtual IEnumerable<T> GetChildren<T>() where T : GameObject
        {
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (child is T t)
                {
                    yield return t;
                }
            }
        }
    }
}