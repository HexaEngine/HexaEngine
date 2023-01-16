namespace HexaEngine.Scenes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects;
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    public class GameObject
    {
#nullable disable
        protected IGraphicsDevice Device;
#nullable enable
        private readonly List<GameObject> children = new();
        private readonly List<IComponent> components = new();
        private Scene? scene;
        private GameObject? parent;
        private bool initialized;

        public Transform Transform = new();
        private string name = string.Empty;
        private bool isSelected;
        private static GameObjectSelection selected = new();
        private GCHandle gcHandle;

        [JsonIgnore]
        public readonly IntPtr Pointer;

        public GameObject()
        {
            gcHandle = GCHandle.Alloc(this, GCHandleType.WeakTrackResurrection);
            Pointer = GCHandle.ToIntPtr(gcHandle);
        }

        ~GameObject()
        {
            gcHandle.Free();
        }

        public virtual string Name
        {
            get => name;
            set
            {
                if (initialized)
                    name = GetScene().GetAvailableName(this, value);
                else
                    name = value;
            }
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
        public static GameObjectSelection Selected => selected;

        [JsonIgnore]
        public bool IsSelected => isSelected;

        [JsonIgnore]
        public bool IsOpen { get; set; }

        [JsonIgnore]
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (isVisible == value) return;
                if (!value)
                {
                    for (int i = 0; i < Children.Count; i++)
                    {
                        Children[i].IsVisible = false;
                    }
                }
                isVisible = value;
            }
        }

        #region Sele

        public class GameObjectSelection : IEnumerable
        {
            private readonly List<GameObject> _objects = new();

            public GameObject this[int index] { get => ((IList<GameObject>)_objects)[index]; set => ((IList<GameObject>)_objects)[index] = value; }

            public int Count => ((ICollection<GameObject>)_objects).Count;

            public bool IsReadOnly => ((ICollection<GameObject>)_objects).IsReadOnly;

            public void PurgeSelection()
            {
                foreach (GameObject gameObject in _objects)
                {
                    gameObject.Parent?.RemoveChild(gameObject);
                }
                ClearSelection();
            }

            public void MoveSelection(GameObject parent)
            {
                foreach (GameObject gameObject in _objects)
                {
                    gameObject.Uninitialize();
                }
                foreach (GameObject gameObject in _objects)
                {
                    parent.AddChild(gameObject);
                }
            }

            public void AddSelection(GameObject gameObject)
            {
                gameObject.isSelected = true;
                _objects.Add(gameObject);
            }

            public void AddOverwriteSelection(GameObject gameObject)
            {
                ClearSelection();
                gameObject.isSelected = true;
                _objects.Add(gameObject);
            }

            public void AddMultipleSelection(IEnumerable<GameObject> gameObjects)
            {
                foreach (GameObject gameObject in gameObjects)
                {
                    AddSelection(gameObject);
                }
            }

            public void RemoveSelection(GameObject item)
            {
                item.isSelected = false;
                _objects.Remove(item);
            }

            public void RemoveMultipleSelection(IEnumerable<GameObject> gameObjects)
            {
                foreach (GameObject gameObject in gameObjects)
                {
                    RemoveSelection(gameObject);
                }
            }

            public void ClearSelection()
            {
                foreach (GameObject gameObject in _objects)
                {
                    gameObject.isSelected = false;
                }
                ((ICollection<GameObject>)_objects).Clear();
            }

            public GameObject? First() => _objects.Count == 0 ? null : _objects[0];

            public GameObject Last() => _objects[^1];

            public bool Contains(GameObject item)
            {
                return ((ICollection<GameObject>)_objects).Contains(item);
            }

            public void CopyTo(GameObject[] array, int arrayIndex)
            {
                ((ICollection<GameObject>)_objects).CopyTo(array, arrayIndex);
            }

            public IEnumerator<GameObject> GetEnumerator()
            {
                return ((IEnumerable<GameObject>)_objects).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_objects).GetEnumerator();
            }
        }

        #endregion Sele

        public virtual List<GameObject> Children => children;

        public virtual List<IComponent> Components => components;

        [JsonIgnore]
        public bool Initialized => initialized;

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
                        node.RemoveChild(child);
                    foreach (var comp in comps)
                        node.RemoveComponent(comp);
                }
                node.Parent?.RemoveChild(node);
                foreach (var child in childs)
                    AddChild(child);
                foreach (var comp in comps)
                    AddComponent(comp);
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
                component.Awake(Device, this);
        }

        public virtual void RemoveComponent(IComponent component)
        {
            if (initialized)
                component.Destory();
            components.Remove(component);
        }

        internal void Update()
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].Update();
            }
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Update();
            }
        }

        internal void FixedUpdate()
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].FixedUpdate();
            }
            for (int i = 0; i < children.Count; i++)
            {
                children[i].FixedUpdate();
            }
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
                return scene;
            return parent?.GetScene() ?? throw new("Node tree invalid");
        }

        public virtual void BuildTree()
        {
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                child.parent = this;
                child.BuildTree();
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
                    return t;
                else if (current?.parent is not null)
                    current = current.parent;
                else
                    return null;
            }
        }

        public virtual T GetOrCreateComponent<T>() where T : IComponent, new()
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is T t)
                    return t;
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
                    return t;
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
                    yield return t;
            }
        }

        private bool isVisible;
        private Type? type;

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
    }
}