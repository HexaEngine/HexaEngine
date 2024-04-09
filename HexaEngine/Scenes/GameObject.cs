namespace HexaEngine.Scenes
{
    using HexaEngine.Core;
    using HexaEngine.Core.Editor;
    using HexaEngine.Mathematics;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public delegate void GameObjectOnEnabledChanged(GameObject gameObject, bool enabled);

    public delegate void GameObjectOnNameChanged(GameObject gameObject, string name);

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public partial class GameObject : EntityNotifyBase, IHierarchyObject, IEditorSelectable
    {
        private readonly List<GameObject> children = [];
        private readonly List<IComponent> components = [];
        private Scene? scene;
        private GameObject? parent;
        private InternalGameObjectFlags gameObjectFlags = InternalGameObjectFlags.Default;
        private InternalEditorFlags editorFlags = InternalEditorFlags.None;
        private Type? type;

        private Transform transform = new();
        private Guid guid = Guid.NewGuid();
        private string name = string.Empty;
        private string? fullName;

        private GCHandle gcHandle;
        private object? tag;

        private enum InternalGameObjectFlags : byte
        {
            None = 0,

            /// <summary>
            /// The GameObject is enabled.
            /// </summary>
            Enabled = 1 << 0,

            /// <summary>
            /// The GameObject is visible in editor mode.
            /// </summary>
            EditorVisible = 1 << 1,

            /// <summary>
            /// The GameObject is hidden.
            /// </summary>
            Hidden = 1 << 2,

            /// <summary>
            /// The GameObject is initialized.
            /// </summary>
            Initialized = 1 << 3,

            Default = Enabled | EditorVisible
        }

        private enum InternalEditorFlags : byte
        {
            None = 0,

            /// <summary>
            /// Is selected in the scene hierarchy widget.
            /// </summary>
            Selected = 1 << 0,

            /// <summary>
            /// Is open in the scene hierarchy widget.
            /// </summary>
            Open = 1 << 1,

            /// <summary>
            /// Is displayed in the scene hierarchy widget.
            /// </summary>
            Displayed = 1 << 2,
        }

        [JsonIgnore]
        public readonly nint Pointer;

        public GameObject()
        {
            gcHandle = GCHandle.Alloc(this, GCHandleType.WeakTrackResurrection);
            Pointer = GCHandle.ToIntPtr(gcHandle);
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
        /// Gets the full name of the <see cref="GameObject"/>, which is a unique combination of its name and Guid.
        /// The full name is lazily generated when accessed for the first time and will be
        /// reinitialized if either the name or Guid property is modified.
        /// </summary>
        /// <value>
        /// The full name of the <see cref="GameObject"/>.
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
            get => (gameObjectFlags & InternalGameObjectFlags.Enabled) != 0;
            set
            {
                if (IsEnabled == value)
                {
                    return;
                }

                SetFlagAndNotify(InternalGameObjectFlags.Enabled, value);

                OnEnabledChanged?.Invoke(this, value);

                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].IsEnabled = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is visible in edit mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is visible in edit mode; otherwise, <c>false</c>.
        /// </value>
        public bool IsEditorVisible
        {
            get => (gameObjectFlags & InternalGameObjectFlags.EditorVisible) != 0;
            set
            {
                if (IsEditorVisible == value)
                {
                    return;
                }

                SetFlagAndNotify(InternalGameObjectFlags.EditorVisible, value);

                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].IsEditorVisible = value;
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
            get => (gameObjectFlags & InternalGameObjectFlags.Hidden) != 0;
            set => SetFlagAndNotify(InternalGameObjectFlags.Hidden, value);
        }

        public Transform Transform { get => transform; set => OverwriteTransform(transform); }

        [JsonIgnore]
        public bool IsVisible => Application.InEditMode ? IsEditorVisible && IsEnabled : IsEnabled;

        private void SetFlagAndNotify(InternalGameObjectFlags flag, bool value, [CallerMemberName] string propertyName = "")
        {
            OnPropertyChanging(propertyName);

            if (value)
            {
                gameObjectFlags |= flag;
            }
            else
            {
                gameObjectFlags &= ~flag;
            }

            OnPropertyChanged(propertyName);
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
        public bool Initialized
        {
            get => (gameObjectFlags & InternalGameObjectFlags.Initialized) != 0;
            internal set => SetFlagAndNotify(InternalGameObjectFlags.Initialized, value);
        }

        [JsonIgnore]
        public bool IsEditorSelected => (editorFlags & InternalEditorFlags.Selected) != 0;

        [JsonIgnore]
        public bool IsEditorOpen
        {
            get => (editorFlags & InternalEditorFlags.Open) != 0;
            set
            {
                if (value)
                {
                    editorFlags |= InternalEditorFlags.Open;
                }
                else
                {
                    editorFlags &= ~InternalEditorFlags.Open;
                }
            }
        }

        [JsonIgnore]
        public bool IsEditorDisplayed
        {
            get => (editorFlags & InternalEditorFlags.Displayed) != 0;
            set
            {
                if (IsEditorDisplayed == value)
                {
                    return;
                }

                if (value)
                {
                    editorFlags |= InternalEditorFlags.Displayed;
                }
                else
                {
                    editorFlags &= ~InternalEditorFlags.Displayed;
                }

                if (!value)
                {
                    for (int i = 0; i < Children.Count; i++)
                    {
                        Children[i].IsEditorDisplayed = false;
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

        IHierarchyObject? IHierarchyObject.Parent => parent;

        bool IEditorSelectable.IsEditorSelected
        {
            get => (editorFlags & InternalEditorFlags.Selected) != 0;
            set
            {
                if (value)
                {
                    editorFlags |= InternalEditorFlags.Selected;
                }
                else
                {
                    editorFlags &= ~InternalEditorFlags.Selected;
                }
            }
        }

        public event GameObjectOnEnabledChanged? OnEnabledChanged;

        public event GameObjectOnNameChanged? OnNameChanged;

        public event Action<GameObject, object?>? OnTagChanged;

        public event Action<GameObject, GameObject?>? OnParentChanged;

        public event Action<GameObject>? OnTransformed;

        public event Action<GameObject, IComponent>? OnComponentAdded;

        public event Action<GameObject, IComponent>? OnComponentRemoved;

        protected void OverwriteTransform(Transform transform)
        {
            this.transform.Updated -= TransformUpdated;
            this.transform = transform;
            transform.Updated += TransformUpdated;
        }

        protected virtual void TransformUpdated(Transform transform)
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
                if (node.Initialized)
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
            if (Initialized)
            {
                node.Initialize();
            }
        }

        public virtual void RemoveChild(GameObject node)
        {
            if (Initialized)
            {
                node.Uninitialize();
            }
            children.Remove(node);
            node.parent = null;
        }

        public virtual void AddComponent(IComponent component)
        {
            components.Add(component);
            if (Initialized)
            {
                component.GameObject = this;
                component.Awake();
            }
            OnComponentAdded?.Invoke(this, component);
        }

        public virtual void AddComponent<T>() where T : IComponent, new()
        {
            T component = new();
            components.Add(component);
            if (Initialized)
            {
                component.GameObject = this;
                component.Awake();
            }
            OnComponentAdded?.Invoke(this, component);
        }

        public virtual void AddComponentSingleton<T>() where T : IComponent, new()
        {
            if (GetComponent<T>() != null)
                return;
            T component = new();
            components.Add(component);
            if (Initialized)
            {
                component.GameObject = this;
                component.Awake();
            }
            OnComponentAdded?.Invoke(this, component);
        }

        public virtual void RemoveComponent(IComponent component)
        {
            if (Initialized)
            {
                component.Destroy();
            }

            components.Remove(component);
            OnComponentRemoved?.Invoke(this, component);
        }

        public virtual void Initialize()
        {
            Transform.Updated += TransformUpdated;
            Transform.Parent = parent?.Transform;
            scene = GetScene();
            scene.RegisterChild(this);

            Initialized = true;
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
            Transform.Updated -= TransformUpdated;
            for (int i = 0; i < components.Count; i++)
            {
                components[i].Destroy();
            }
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Uninitialize();
            }
            scene?.UnregisterChild(this);
            Initialized = false;
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

        public virtual T? FindParent<T>() where T : GameObject
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

        public virtual T? FindChild<T>() where T : GameObject
        {
            for (int i = 0; i < children?.Count; i++)
            {
                var child = children[i];
                if (child is T t)
                {
                    return t;
                }

                var result = child.FindChild<T>();
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public virtual T? GetComponentFromParent<T>() where T : IComponent
        {
            var current = parent;

            while (current != null)
            {
                var component = current.GetComponent<T>();

                if (component != null)
                {
                    return component;
                }

                current = current.parent;
            }

            return default;
        }

        public virtual IEnumerable<T> GetComponentsFromParents<T>() where T : IComponent
        {
            var current = parent;

            while (current != null)
            {
                var components = current.GetComponents<T>();

                foreach (var component in components)
                {
                    yield return component;
                }

                current = current.parent;
            }
        }

        public virtual T? GetComponentFromChild<T>() where T : IComponent
        {
            for (int i = 0; i < children?.Count; i++)
            {
                var child = children[i];
                var component = child.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }

                var result = child.GetComponentFromChild<T>();
                if (result != null)
                {
                    return result;
                }
            }
            return default;
        }

        public virtual IEnumerable<T> GetComponentsFromChilds<T>() where T : IComponent
        {
            for (int i = 0; i < children?.Count; i++)
            {
                var child = children[i];
                var components = child.GetComponents<T>();
                foreach (var component in components)
                {
                    yield return component;
                }

                foreach (var component in child.GetComponentsFromChilds<T>())
                {
                    yield return component;
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

        public virtual IEnumerable<T> GetComponentsFromTree<T>() where T : IComponent
        {
            foreach (var component in GetComponents<T>())
            {
                yield return component;
            }

            foreach (var child in Children)
            {
                foreach (var component in child.GetComponentsFromTree<T>())
                {
                    yield return component;
                }
            }
        }

        public virtual IEnumerable<T> DiscoverComponents<T, TStop>() where T : IComponent where TStop : IComponent
        {
            foreach (var component in GetComponents<T>())
            {
                yield return component;
            }

            foreach (var child in Children)
            {
                if (child.HasComponent<TStop>())
                {
                    continue;
                }

                foreach (var component in child.DiscoverComponents<T, TStop>())
                {
                    yield return component;
                }
            }
        }

        public static void RemoveComponent(ValueTuple<GameObject, IComponent> values)
        {
            values.Item1.RemoveComponent(values.Item2);
        }

        public static void AddComponent(ValueTuple<GameObject, IComponent> values)
        {
            values.Item1.AddComponent(values.Item2);
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

        void IHierarchyObject.AddChild(IHierarchyObject selectable)
        {
            if (selectable is GameObject gameObject)
            {
                AddChild(gameObject);
            }
        }

        void IHierarchyObject.RemoveChild(IHierarchyObject selectable)
        {
            if (selectable is GameObject gameObject)
            {
                RemoveChild(gameObject);
            }
        }

        private string GetDebuggerDisplay()
        {
            return FullName;
        }
    }
}