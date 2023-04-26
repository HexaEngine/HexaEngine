namespace HexaEngine.Core.Scenes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    public partial class GameObject : EntityNotifyBase
    {
        private static readonly GameObjectSelection selected = new();

#nullable disable
        protected IGraphicsDevice Device;
#nullable enable
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
        private string name = string.Empty;
        private bool isEditorSelected;

        private GCHandle gcHandle;
        private Guid guid = Guid.NewGuid();

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

        public Guid Guid
        {
            get => guid;
            set => guid = value;
        }

        public string Name
        {
            get => name;
            set
            {
                if (initialized)
                {
                    name = GetScene().GetAvailableName(value);
                }
                else
                {
                    name = value;
                }
            }
        }

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
                parent = value;
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

        public event Action<GameObject>? Transformed;

        protected void OverwriteTransform(Transform transform)
        {
            Transform.Updated -= TransformUpdated;
            transform.Updated += TransformUpdated;
        }

        protected virtual void TransformUpdated(object? sender, EventArgs e)
        {
            Transformed?.Invoke(this);
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
        }

        public void Merge(GameObject node)
        {
            if (Initialized)
            {
                var childs = node.children.ToArray();
                var comps = node.components.ToArray();
                if (node.initialized)
                {
                    foreach (var child in childs)
                    {
                        node.RemoveChild(child);
                    }

                    foreach (var comp in comps)
                    {
                        node.RemoveComponent(comp);
                    }
                }
                node.Parent?.RemoveChild(node);
                foreach (var child in childs)
                {
                    AddChild(child);
                }

                foreach (var comp in comps)
                {
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
                node.Initialize(Device);
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
                component.Awake(Device, this);
            }
        }

        public virtual void RemoveComponent(IComponent component)
        {
            if (initialized)
            {
                component.Destory();
            }

            components.Remove(component);
        }

        public virtual void Initialize(IGraphicsDevice device)
        {
            Transform.Parent = parent?.Transform;
            scene = GetScene();
            scene.RegisterChild(this);

            Device = device;
            initialized = true;
            for (int i = 0; i < components.Count; i++)
            {
                components[i].Awake(device, this);
            }
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Initialize(device);
            }
        }

        public virtual void Uninitialize()
        {
            Device = null;
            for (int i = 0; i < components.Count; i++)
            {
                components[i].Destory();
            }
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Uninitialize();
            }
            scene?.UnregisterChild(this);
            initialized = false;
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

        public virtual bool TryGetComponent<T>([NotNullWhen(true)] out T? value) where T : class, IComponent
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

        public Type Type
        {
            get
            {
                if (type == null)
                {
                    type = GetType();
                }
                return type;
            }
        }

        public static void RemoveComponent(GameObject gameObject, IComponent component)
        {
            gameObject.RemoveComponent(component);
        }

        public static void RemoveComponent(ValueTuple<GameObject, IComponent> values)
        {
            values.Item1.RemoveComponent(values.Item2);
        }
    }
}