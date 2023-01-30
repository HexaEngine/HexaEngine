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
        public uint MaxLinks;
        private object? _value;
        public readonly NodeEditor Graph;
        private readonly List<Link> links = new();

        public Pin(NodeEditor graph, Node parent, string name, PinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue)
        {
            Graph = graph;
            Id = graph.GetUniqueId();
            Parent = parent;
            Name = name;
            Shape = shape;
            Kind = kind;
            Type = type;
            MaxLinks = maxLinks;
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

        public virtual bool CanCreateLink(Pin input, Pin output)
        {
            if (Id == input.Id) return false;
            if (Links.Count == MaxLinks) return false;
            if (Type == PinType.DontCare) return true;
            if (!IsType(input)) return false;
            if (Kind == input.Kind) return false;

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
            else if (other.Type == PinType.VectorAny)
            {
                return IsVector(Type);
            }
            else if (Type == PinType.VectorAny)
            {
                return IsVector(other.Type);
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

        public static bool IsVector(PinType type)
        {
            return type switch
            {
                PinType.VectorAny => true,
                PinType.Float => true,
                PinType.Vector2 => true,
                PinType.Vector3 => true,
                PinType.Vector4 => true,
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