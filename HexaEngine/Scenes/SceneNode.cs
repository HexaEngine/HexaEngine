namespace HexaEngine.Scenes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public abstract class SceneNode
    {
#nullable disable
        protected IGraphicsDevice Device;
#nullable enable
        private readonly List<SceneNode> children = new();
        private readonly List<IComponent> components = new();
        private Scene? scene;
        private SceneNode? parent;
        private bool initialized;

        public Transform Transform = new();
        public string Name = string.Empty;
        private bool isSelected;
        private static SceneNode? selectedNode;

        [JsonIgnore]
        public virtual SceneNode? Parent
        {
            get => parent;
            set
            {
                parent = value;
            }
        }

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
                if (selectedNode != null)
                    selectedNode.isSelected = false;
                selectedNode = this;
                isSelected = value;
            }
        }

        public virtual IReadOnlyList<SceneNode> Children => children;

        public virtual IReadOnlyList<IComponent> Components => components;

        [JsonIgnore]
        public bool Initialized => initialized;

        public virtual void AddChild(SceneNode node)
        {
            node.parent?.RemoveChild(node);
            node.parent = this;
            children.Add(node);
            if (initialized)
                node.Initialize(Device);
        }

        public virtual void RemoveChild(SceneNode node)
        {
            if (initialized)
                node.Uninitialize();
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
            scene = GetScene();
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
            initialized = false;
        }

        public virtual Scene GetScene()
        {
            if (scene != null)
                return scene;
            return parent?.GetScene() ?? throw new("Node tree invalid");
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
    }
}