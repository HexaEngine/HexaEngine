namespace HexaEngine.Scenes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Text.Json.Serialization;

    public class GameObject
    {
#nullable disable
        protected IGraphicsDevice Device;
#nullable enable
        private readonly List<GameObject> children = new();
        private readonly List<IComponent> components = new();
        private readonly List<int> meshes = new();
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

            public GameObject First() => _objects[0];

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

        [JsonInclude]
        public virtual IReadOnlyList<GameObject> Children => children;

        [JsonInclude]
        public virtual IReadOnlyList<IComponent> Components => components;

        [JsonInclude]
        public virtual IReadOnlyList<int> Meshes => meshes;

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
                var meshes = node.meshes.ToArray();
                if (node.initialized)
                {
                    foreach (var child in childs)
                        node.RemoveChild(child);
                    foreach (var comp in comps)
                        node.RemoveComponent(comp);
                    foreach (var mesh in meshes)
                        node.RemoveMesh(mesh);
                }
                node.Parent?.RemoveChild(node);
                foreach (var child in childs)
                    AddChild(child);
                foreach (var comp in comps)
                    AddComponent(comp);
                foreach (var mesh in meshes)
                    AddMesh(mesh);
            }
            else
            {
                children.AddRange(node.children);
                node.children.Clear();
                components.AddRange(node.components);
                node.components.Clear();
                meshes.AddRange(node.meshes);
                node.meshes.Clear();
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
            GetScene().CommandQueue.Enqueue(new SceneCommand(CommandType.Load, this));
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
            GetScene().CommandQueue.Enqueue(new SceneCommand(CommandType.Unload, this));
        }

        public virtual Scene GetScene()
        {
            if (scene != null)
                return scene;
            return parent?.GetScene() ?? throw new("Node tree invalid");
        }

        public virtual void GetDepth(ref int depth)
        {
            depth++;
            parent?.GetDepth(ref depth);
        }

        public virtual void AddMesh(int index)
        {
            meshes.Add(index);
            if (Initialized)
                GetScene().CommandQueue.Enqueue(new SceneCommand(CommandType.Update, this, ChildCommandType.Added, index));
        }

        public virtual void RemoveMesh(int index)
        {
            meshes.Remove(index);
            if (Initialized)
                GetScene().CommandQueue.Enqueue(new SceneCommand(CommandType.Update, this, ChildCommandType.Removed, index));
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

        public virtual T? GetComponent<T>() where T : IComponent
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is T t)
                    return t;
            }
            return default;
        }

        public virtual IEnumerable<T> GetComponents<T>() where T : IComponent
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is T t)
                    yield return t;
            }
        }

        private IPropertyEditor? editor;
        private bool isVisible;

        [JsonIgnore]
        public virtual IPropertyEditor Editor
        {
            get
            {
                editor ??= new PropertyEditor<GameObject>(this);
                return editor;
            }
            protected set
            {
                editor = value;
            }
        }

        protected void CreatePropertyEditor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>() where T : GameObject
        {
            editor = new PropertyEditor<T>((T)this);
        }
    }
}