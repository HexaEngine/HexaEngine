namespace HexaEngine.Editor.NodeEditor
{
    using ImGuiNET;
    using ImNodesNET;
    using System.Collections.Generic;

    public class Pin
    {
        public readonly int Id;
        public readonly Node Parent;
        public string Name;
        public PinShape Shape;
        public PinKind Kind;
        public PinType Type;
        private object? _value;
        public readonly NodeEditor Graph;
        private readonly List<Link> links = new();

        public Pin(NodeEditor graph, Node parent, string name, PinShape shape, PinKind kind, PinType type)
        {
            Graph = graph;
            Id = graph.GetUniqueId();
            Parent = parent;
            Name = name;
            Shape = shape;
            Kind = kind;
            Type = type;
        }

        public event EventHandler? ValueChanged;

        public event EventHandler<Link>? LinkCreated;

        public event EventHandler<Link>? LinkRemoved;

        public IReadOnlyList<Link> Links => links;

        public void AddLink(Link link)
        {
            links.Add(link);
            LinkCreated?.Invoke(this, link);
        }

        public void RemoveLink(Link link)
        {
            links.Remove(link);
            LinkRemoved?.Invoke(this, link);
        }

        public object? Value
        {
            get => _value;
            set
            {
                _value = value;
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool CanCreateLink(Pin other)
        {
            if (Id == other.Id) return false;
            if (!IsType(other)) return false;
            if (Kind == other.Kind) return false;
            return true;
        }

        public bool IsType(Pin other)
        {
            if (other.Type == PinType.TextureAny)
            {
                return IsTexture(Type);
            }
            else if (Type == PinType.TextureAny)
            {
                return IsTexture(other.Type);
            }
            else
            {
                return other.Type == Type;
            }
        }

        public static bool IsTexture(PinType type)
        {
            return type switch
            {
                PinType.TextureAny => true,
                PinType.Texture1D => true,
                PinType.Texture1DArray => true,
                PinType.Texture2D => true,
                PinType.Texture2DMS => true,
                PinType.Texture2DArray => true,
                PinType.Texture2DMSArray => true,
                PinType.TextureCube => true,
                PinType.TextureCubeArray => true,
                PinType.Texture3D => true,
                _ => false
            };
        }

        public void Draw()
        {
            if (Kind == PinKind.Input)
            {
                ImNodes.BeginInputAttribute(Id, Shape);
                ImGui.Text(Name);
                ImNodes.EndInputAttribute();
            }
            if (Kind == PinKind.Output)
            {
                ImNodes.BeginOutputAttribute(Id, Shape);
                ImGui.Text(Name);
                ImNodes.EndOutputAttribute();
            }
            if (Kind == PinKind.Static)
            {
                ImNodes.BeginStaticAttribute(Id);
                ImGui.Text(Name);
                ImNodes.EndStaticAttribute();
            }
        }

        public virtual void Destroy()
        {
            var links = Links.ToArray();
            for (int i = 0; i < links.Length; i++)
            {
                links[i].Destroy();
            }

            Graph.RemovePin(this);
        }
    }
}