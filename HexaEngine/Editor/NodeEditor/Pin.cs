namespace HexaEngine.Editor.NodeEditor
{
    using HexaEngine.ImGuiNET;
    using HexaEngine.ImNodesNET;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class Pin
    {
        private NodeEditor? editor;
        private Node parent;
        private int id;

        public readonly string Name;
        public ImNodesPinShape Shape;
        public PinKind Kind;
        public PinType Type;
        public uint MaxLinks;
        public bool IsActive;

        private readonly List<Link> links = new();

#pragma warning disable CS8618 // Non-nullable field 'parent' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.

        public Pin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue)
#pragma warning restore CS8618 // Non-nullable field 'parent' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        {
            this.id = id;
            Name = name;
            Shape = shape;
            Kind = kind;
            Type = type;
            MaxLinks = maxLinks;
        }

        public event EventHandler<Pin>? ValueChanging;

        public event EventHandler<Pin>? ValueChanged;

        public event EventHandler<Link>? LinkCreated;

        public event EventHandler<Link>? LinkRemoved;

        public int Id => id;

        [JsonIgnore]
        public Node Parent => parent;

        [JsonIgnore]
        public List<Link> Links => links;

        protected void OnValueChanging()
        {
            ValueChanging?.Invoke(this, this);
        }

        protected void OnValueChanged()
        {
            ValueChanged?.Invoke(this, this);
        }

        public PinId ToId()
        {
            return new(id, parent.Id);
        }

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

        public virtual bool CanCreateLink(Pin other)
        {
            if (id == other.id) return false;
            if (Links.Count == MaxLinks) return false;
            if (Type == PinType.DontCare) return true;
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
            else if (other.Type == PinType.AnyFloat)
            {
                return IsFloatType(Type);
            }
            else if (Type == PinType.AnyFloat)
            {
                return IsFloatType(other.Type);
            }
            else if (Type == PinType.Float2OrFloat)
            {
                return other.Type == PinType.Float || other.Type == PinType.Float2 || other.Type == PinType.Float2OrFloat;
            }
            else if (Type == PinType.Float3OrFloat)
            {
                return other.Type == PinType.Float || other.Type == PinType.Float3 || other.Type == PinType.Float3OrFloat;
            }
            else if (Type == PinType.Float4OrFloat)
            {
                return other.Type == PinType.Float || other.Type == PinType.Float4 || other.Type == PinType.Float4OrFloat;
            }
            else if (other.Type == PinType.Float2OrFloat)
            {
                return Type == PinType.Float || Type == PinType.Float2 || Type == PinType.Float2OrFloat;
            }
            else if (other.Type == PinType.Float3OrFloat)
            {
                return Type == PinType.Float || Type == PinType.Float3 || Type == PinType.Float3OrFloat;
            }
            else if (other.Type == PinType.Float4OrFloat)
            {
                return Type == PinType.Float || Type == PinType.Float4 || Type == PinType.Float4OrFloat;
            }
            else
            {
                return other.Type == Type;
            }
        }

        public static bool IsType(Pin a, Pin b)
        {
            if (a.Type == PinType.TextureAny)
            {
                return IsTexture(b.Type);
            }
            else if (a.Type == PinType.AnyFloat)
            {
                return IsFloatType(b.Type);
            }
            else if (a.Type == PinType.Float2OrFloat)
            {
                return b.Type == PinType.Float || b.Type == PinType.Float2 || b.Type == PinType.Float2OrFloat;
            }
            else if (a.Type == PinType.Float3OrFloat)
            {
                return b.Type == PinType.Float || b.Type == PinType.Float3 || b.Type == PinType.Float3OrFloat;
            }
            else if (a.Type == PinType.Float4OrFloat)
            {
                return b.Type == PinType.Float || b.Type == PinType.Float4 || b.Type == PinType.Float4OrFloat;
            }
            else
            {
                return a.Type == b.Type;
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

        public static bool IsFloatType(PinType type)
        {
            return type switch
            {
                PinType.AnyFloat => true,
                PinType.Float => true,
                PinType.Float2 => true,
                PinType.Float3 => true,
                PinType.Float4 => true,
                _ => false
            };
        }

        public void Draw()
        {
            if (Kind == PinKind.Input)
            {
                ImNodes.BeginInputAttribute(id, Shape);
                DrawContent();
                ImNodes.EndInputAttribute();
            }
            if (Kind == PinKind.Output)
            {
                ImNodes.BeginOutputAttribute(id, Shape);
                DrawContent();
                ImNodes.EndOutputAttribute();
            }
            if (Kind == PinKind.Static)
            {
                ImNodes.BeginStaticAttribute(id);
                DrawContent();
                ImNodes.EndStaticAttribute();
            }
        }

        protected virtual void DrawContent()
        {
            ImGui.Text(Name);
        }

        public virtual void Initialize(NodeEditor editor, Node parent)
        {
            this.editor = editor;
            this.parent = parent;
            if (id == 0)
                id = editor.GetUniqueId();
        }

        public virtual void Destroy()
        {
            if (editor == null) return;
            var links = Links.ToArray();
            for (int i = 0; i < links.Length; i++)
            {
                links[i].Destroy();
            }

            editor = null;
        }
    }
}