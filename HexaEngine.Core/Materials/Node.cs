namespace HexaEngine.Materials
{
    using HexaEngine.Core.Collections;
    using HexaEngine.Materials.Nodes.Textures;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Numerics;

    public class Node : INode<Node>
    {
        protected NodeEditor? editor;
        private int id;

        private readonly List<Pin> pins = new();
        private readonly List<Link> links = new();
        private readonly List<Node> dependencies = new();

        public readonly bool Removable = true;
        public readonly bool IsStatic;
        public string Name;
        public string OriginalName;

        [JsonIgnore]
        public Vector4 TitleColor = 0x6930c3ff.ABGRToVec4();

        [JsonIgnore]
        public Vector4 TitleHoveredColor = 0x5e60ceff.ABGRToVec4();

        [JsonIgnore]
        public Vector4 TitleSelectedColor = 0x7400b8ff.ABGRToVec4();

        public bool isEditing;
        public Vector2 position;
        public Vector2 size;
        public bool wantsSetPosition;

        public Node(int id, string name, bool removable, bool isStatic)
        {
            this.id = id;
            Name = name;
            OriginalName = name;
            Removable = removable;
            IsStatic = isStatic;
        }

        public event EventHandler<Pin>? PinValueChanging;

        public event EventHandler<Pin>? PinValueChanged;

        public event EventHandler<Node>? NodeValueChanging;

        public event EventHandler<Node>? NodeValueChanged;

        public event EventHandler<Pin>? PinAdded;

        public event EventHandler<Pin>? PinRemoved;

        public event EventHandler<Link>? LinkAdded;

        public event EventHandler<Link>? LinkRemoved;

        public int Id => id;

        [JsonIgnore]
        public IReadOnlyList<Link> Links => links;

        [JsonIgnore]
        public IReadOnlyList<Pin> Pins => pins;

        [JsonIgnore]
        public Vector2 Position
        { get => position; set { position = value; wantsSetPosition = true; } }

        [JsonIgnore]
        public Vector2 Size => size;

        [JsonIgnore]
        public bool IsEditing
        {
            get => isEditing;
            set
            {
                isEditing = value;
            }
        }

        [JsonIgnore]
        public bool IsHovered { get; set; }

        [JsonIgnore]
        IEnumerable<Node> INode<Node>.Dependencies => dependencies;

        [JsonIgnore]
        public IReadOnlyList<Node> Dependencies => dependencies;

        public virtual void Initialize(NodeEditor editor)
        {
            this.editor = editor;
            if (id == 0)
            {
                id = editor.GetUniqueId();
            }

            for (int i = 0; i < pins.Count; i++)
            {
                pins[i].Initialize(editor, this);
                pins[i].ValueChanging += ValueChanging;
                pins[i].ValueChanged += ValueChanged;
            }
        }

        public void OnValueChanged()
        {
            NodeValueChanged?.Invoke(this, this);
        }

        public void OnValueChanging()
        {
            NodeValueChanging?.Invoke(this, this);
        }

        private void ValueChanged(object? sender, Pin e)
        {
            PinValueChanged?.Invoke(this, e);
        }

        private void ValueChanging(object? sender, Pin e)
        {
            PinValueChanging?.Invoke(this, e);
        }

        public Pin GetInput(int id)
        {
            Pin? pin = Find(id);
            if (pin == null || pin.Kind != PinKind.Input)
            {
                throw new();
            }
            return pin;
        }

        public Pin GetOutput(int id)
        {
            Pin? pin = Find(id);
            if (pin == null || pin.Kind != PinKind.Output)
            {
                throw new();
            }
            return pin;
        }

        public Pin? Find(int id)
        {
            for (int i = 0; i < pins.Count; i++)
            {
                var pin = pins[i];
                if (pin.Id == id)
                {
                    return pin;
                }
            }
            return null;
        }

        public Pin? Find(string name)
        {
            for (int i = 0; i < pins.Count; i++)
            {
                var pin = pins[i];
                if (pin.Name == name)
                {
                    return pin;
                }
            }
            return null;
        }

        public bool PinExists(string name)
        {
            for (int i = 0; i < pins.Count; i++)
            {
                var pin = pins[i];
                if (pin.Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public static Link? FindSourceLink(Pin pin, Node other)
        {
            for (int i = 0; i < pin.Links.Count; i++)
            {
                Link link = pin.Links[i];
                if (link.OutputNode == other)
                {
                    return link;
                }
            }
            return null;
        }

        public static Link? FindTargetLink(Pin pin, Node other)
        {
            for (int i = 0; i < pin.Links.Count; i++)
            {
                Link link = pin.Links[i];
                if (link.InputNode == other)
                {
                    return link;
                }
            }
            return null;
        }

        public virtual Pin CreatePin(NodeEditor editor, string name, PinKind kind, PinType type, PinShape shape, uint maxLinks = uint.MaxValue)
        {
            Pin pin = new(editor.GetUniqueId(), name, shape, kind, type, maxLinks);
            return AddPin(pin);
        }

        public virtual Pin CreateOrGetPin(NodeEditor editor, string name, PinKind kind, PinType type, PinShape shape, uint maxLinks = uint.MaxValue)
        {
            Pin pin = new(editor.GetUniqueId(), name, shape, kind, type, maxLinks);
            return AddOrGetPin(pin);
        }

        public virtual T AddPin<T>(T pin) where T : Pin
        {
            Pin? old = Find(pin.Name);

            if (old != null)
            {
                int index = pins.IndexOf(old);
                old.Destroy();
                if (editor != null)
                {
                    pin.Initialize(editor, this);
                }

                pins[index] = pin;
            }
            else
            {
                if (editor != null)
                {
                    pin.Initialize(editor, this);
                }

                pins.Add(pin);
                PinAdded?.Invoke(this, pin);
            }

            return pin;
        }

        public virtual T AddOrGetPin<T>(T pin) where T : Pin
        {
            Pin? old = Find(pin.Name);

            if (old != null)
            {
                return (T)old;
            }
            else
            {
                if (editor != null)
                {
                    pin.Initialize(editor, this);
                }

                pins.Add(pin);
                PinAdded?.Invoke(this, pin);
            }

            return pin;
        }

        public virtual void DestroyPin<T>(T pin) where T : Pin
        {
            pin.Destroy();
            pins.Remove(pin);
            PinRemoved?.Invoke(this, pin);
        }

        public virtual void AddLink(Link link)
        {
            links.Add(link);
            LinkAdded?.Invoke(this, link);
            if (link.InputNode == this && !dependencies.Contains(link.OutputNode))
            {
                dependencies.Add(link.OutputNode);
                dependencies.Sort(new DependencyComparer(this));
            }
        }

        public virtual void RemoveLink(Link link)
        {
            if (link.InputNode == this)
            {
                dependencies.Remove(link.OutputNode);
            }
            links.Remove(link);
            LinkRemoved?.Invoke(this, link);
        }

        public Link? FindLink(Node node)
        {
            foreach (Link link in links)
            {
                if (link.OutputNode == node || link.InputNode == node)
                {
                    return link;
                }
            }

            return null;
        }

        private readonly struct DependencyComparer : IComparer<Node>
        {
            private readonly Node parent;

            public DependencyComparer(Node parent)
            {
                this.parent = parent;
            }

            public int Compare(Node? x, Node? y)
            {
                if (x == null || y == null)
                {
                    return 0;
                }

                Link linkA = parent.FindLink(x)!;
                Link linkB = parent.FindLink(y)!;

                int idxA = parent.pins.IndexOf(linkA.Input);
                int idxB = parent.pins.IndexOf(linkB.Input);

                return idxA.CompareTo(idxB);
            }
        }

        public virtual void Destroy()
        {
            renderer?.Dispose();

            if (editor == null)
            {
                return;
            }

            for (int i = 0; i < pins.Count; i++)
            {
                pins[i].ValueChanged -= ValueChanged;
                pins[i].ValueChanging -= ValueChanging;
                pins[i].Destroy();
            }
            dependencies.Clear();
            links.Clear();
            pins.Clear();
            editor.RemoveNode(this);
            editor = null;
        }

        private INodeRenderer? renderer;

        public virtual void Draw()
        {
            renderer ??= NodeEditorRegistry.GetNodeRenderer(this);
            renderer.Draw(this);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}