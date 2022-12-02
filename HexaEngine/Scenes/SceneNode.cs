namespace HexaEngine.Scenes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Text.Json.Serialization;

    public class SceneNode
    {
#nullable disable
        protected IGraphicsDevice Device;
#nullable enable
        private readonly List<SceneNode> children = new();
        private readonly List<IComponent> components = new();
        private readonly List<int> meshes = new();
        private Scene? scene;
        private SceneNode? parent;
        private bool initialized;

        public Transform Transform = new();
        private string name = string.Empty;
        private bool isSelected;
        private static SceneNode? selectedNode;
        private GCHandle gcHandle;

        [JsonIgnore]
        public readonly IntPtr Pointer;

        public SceneNode()
        {
            gcHandle = GCHandle.Alloc(this, GCHandleType.WeakTrackResurrection);
            Pointer = GCHandle.ToIntPtr(gcHandle);
        }

        ~SceneNode()
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
        public virtual SceneNode? Parent
        {
            get => parent;
            set
            {
                parent = value;
            }
        }

        [JsonIgnore]
        public static SceneNode? SelectedNode
        {
            get => selectedNode;
        }

        [JsonIgnore]
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (value)
                {
                    if (selectedNode != null)
                        selectedNode.isSelected = false;
                    selectedNode = this;
                }
                else if (selectedNode == this)
                {
                    selectedNode = null;
                }

                isSelected = value;
            }
        }

        [JsonInclude]
        public virtual IReadOnlyList<SceneNode> Children => children;

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

        public void Merge(SceneNode node)
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

        public virtual void AddChild(SceneNode node)
        {
            node.parent?.RemoveChild(node);
            node.parent = this;
            children.Add(node);
            if (initialized)
            {
                node.Initialize(Device);
            }
        }

        public virtual void RemoveChild(SceneNode node)
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
                component.Initialize(Device, this);
        }

        public virtual void RemoveComponent(IComponent component)
        {
            if (initialized)
                component.Uninitialize();
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
                components[i].Initialize(device, this);
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
                components[i].Uninitialize();
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

        public virtual T? GetParentNodeOf<T>() where T : SceneNode
        {
            SceneNode? current = parent;
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

        [JsonIgnore]
        public virtual IPropertyEditor Editor
        {
            get
            {
                editor ??= new PropertyEditor<SceneNode>(this);
                return editor;
            }
            protected set
            {
                editor = value;
            }
        }

        protected void CreatePropertyEditor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>() where T : SceneNode
        {
            editor = new PropertyEditor<T>((T)this);
        }
    }
}