namespace HexaEngine.Materials
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    [Flags]
    public enum PinShape
    {
        Circle = 0,
        CircleFilled = 1,
        Triangle = 2,
        TriangleFilled = 3,
        Quad = 4,
        QuadFilled = 5
    }

    public class Pin
    {
        private NodeEditor? editor;
        private Node parent = null!;
        private int id;

        public readonly string Name;
        public PinShape Shape;
        public PinKind Kind;
        private PinType type;
        [JsonIgnore] public PinFlags Flags;
        public uint MaxLinks;
        public bool IsActive;

        private readonly List<Link> links = [];

        public Pin(int id, string name, PinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None)
        {
            this.id = id;
            Name = name;
            Shape = shape;
            Kind = kind;
            Type = type;
            MaxLinks = maxLinks;
            Flags = flags;
        }

        public event EventHandler<Pin>? ValueChanging;

        public event EventHandler<Pin>? ValueChanged;

        public event EventHandler<Link>? LinkCreated;

        public event EventHandler<Link>? LinkRemoved;

        public int Id { get => id; set => id = value; }

        [JsonIgnore]
        public Node Parent => parent;

        [JsonIgnore]
        public List<Link> Links => links;

        public virtual PinType Type { get => type; set => type = value; }

        public void OnValueChanging()
        {
            ValueChanging?.Invoke(this, this);
        }

        public void OnValueChanged()
        {
            ValueChanged?.Invoke(this, this);
        }

        protected void SetAndNotifyValueChanged<T>(ref T target, T value)
        {
            OnValueChanging();
            target = value;
            OnValueChanged();
        }

        public PinId ToId()
        {
            return new(id, parent.Id);
        }

        public void AddLink(Link link)
        {
            if (links.Contains(link))
            {
                return;
            }

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
            if (id == other.id)
            {
                return false;
            }

            if (Links.Count == MaxLinks)
            {
                return false;
            }

            if (Type == PinType.DontCare)
            {
                return parent.CanCreateLink(this, other.parent, other);
            }

            if (!IsType(other))
            {
                return false;
            }

            if (Kind == other.Kind)
            {
                return false;
            }

            return parent.CanCreateLink(this, other.parent, other);
        }

        public bool IsType(Pin other)
        {
            return IsType(this, other);
        }

        public static bool IsTypeOfVecOrScalar(PinType a, PinType b, PinType baseType, out bool result)
        {
            /*
            Float,          // 0
            Float2,         // 1
            Float3,         // 2
            Float4,         // 3
            Float2OrFloat,  // 4
            Float3OrFloat,  // 5
            Float4OrFloat,  // 6
            AnyFloat        // 7
             */

            // early exit.
            if (a < baseType || a > baseType + 6 || b < baseType || b > baseType + 6)
            {
                result = false;
                return false;
            }

            if (a == baseType + 4)
            {
                result = b == baseType || b == baseType + 1 || b == baseType + 4;
                return true;
            }

            if (a == baseType + 5)
            {
                result = b == baseType || b == baseType + 2 || b == baseType + 5;
                return true;
            }

            if (a == baseType + 6)
            {
                result = b == baseType || b == baseType + 3 || b == baseType + 6;
                return true;
            }

            if (b == baseType + 4)
            {
                result = a == baseType || a == baseType + 1 || a == baseType + 4;
                return true;
            }

            if (b == baseType + 5)
            {
                result = a == baseType || a == baseType + 2 || a == baseType + 5;
                return true;
            }

            if (b == baseType + 6)
            {
                result = a == baseType || a == baseType + 3 || a == baseType + 6;
                return true;
            }

            result = false;
            return false;
        }

        public static bool IsType(Pin a, Pin b)
        {
            if (b.Type == PinType.DontCare)
            {
                return true;
            }
            else if (IsTypeOfAny(a.Type, b.Type, out bool result) || IsTypeOfAny(b.Type, a.Type, out result))
            {
                return result;
            }
            else if (IsTypeOfVecOrScalar(a.Type, b.Type, PinType.Half, out result))
            {
                return result;
            }
            else if (IsTypeOfVecOrScalar(a.Type, b.Type, PinType.Float, out result))
            {
                return result;
            }
            else if (IsTypeOfVecOrScalar(a.Type, b.Type, PinType.Double, out result))
            {
                return result;
            }
            else if (IsTypeOfVecOrScalar(a.Type, b.Type, PinType.Bool, out result))
            {
                return result;
            }
            else if (IsTypeOfVecOrScalar(a.Type, b.Type, PinType.Int, out result))
            {
                return result;
            }
            else if (IsTypeOfVecOrScalar(a.Type, b.Type, PinType.UInt, out result))
            {
                return result;
            }
            else
            {
                return a.Type == b.Type;
            }
        }

        public static bool IsTexture(PinType type)
        {
            return type >= PinType.Texture1D && type <= PinType.TextureAny;
        }

        public static bool IsTypeOfAny(PinType a, PinType b, out bool result)
        {
            switch (a)
            {
                case PinType.AnyHalf:
                    result = b >= PinType.Half && b <= PinType.AnyHalf && (b < PinType.Half2OrHalf || b > PinType.Half4OrHalf);
                    return true;

                case PinType.AnyFloat:
                    result = b >= PinType.Float && b <= PinType.AnyFloat && (b < PinType.Float2OrFloat || b > PinType.Float4OrFloat);
                    return true;

                case PinType.AnyDouble:
                    result = b >= PinType.Double && b <= PinType.AnyDouble && (b < PinType.Double2OrDouble || b > PinType.Double4OrDouble);
                    return true;

                case PinType.AnyBool:
                    result = b >= PinType.Bool && b <= PinType.AnyBool && (b < PinType.Bool2OrBool || b > PinType.Bool4OrBool);
                    return true;

                case PinType.AnyInt:
                    result = b >= PinType.Int && b <= PinType.AnyInt && (b < PinType.Int2OrInt || b > PinType.Int4OrInt);
                    return true;

                case PinType.AnyUInt:
                    result = b >= PinType.UInt && b <= PinType.AnyUInt && (b < PinType.UInt2OrUInt || b > PinType.UInt4OrUInt);
                    return true;

                case PinType.TextureAny:
                    result = IsTexture(b);
                    return true;

                default:
                    result = false;
                    return false;
            }
        }

        private IPinRenderer? renderer;

        public void Draw()
        {
            renderer ??= NodeEditorRegistry.GetPinRenderer(this);
            renderer.Draw(this);
        }

        public virtual void Initialize(NodeEditor editor, Node parent)
        {
            this.editor = editor;
            this.parent = parent;
            if (id == 0)
            {
                id = editor.GetUniqueId();
            }
        }

        public virtual void Destroy()
        {
            renderer?.Dispose();

            if (editor == null)
            {
                return;
            }

            var links = Links.ToArray();
            for (int i = 0; i < links.Length; i++)
            {
                links[i].Destroy();
            }

            editor = null;
        }

        public Pin? FindLink<TNode>(PinKind searchFor) where TNode : Node
        {
            return FindNode<TNode>(this, searchFor);
        }

        public static Pin? FindNode<TNode>(Pin startPin, PinKind searchFor) where TNode : Node
        {
            var visited = new HashSet<Pin>();
            var stack = new Stack<Pin>();
            stack.Push(startPin);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                visited.Add(current);

                if (current.Parent is TNode)
                {
                    return current;
                }

                // Check pins based on the direction of search
                foreach (var pin in current.Parent.Pins)
                {
                    if (pin.Kind != searchFor)
                    {
                        foreach (var link in pin.Links)
                        {
                            var adjacentPin = pin.Kind == PinKind.Input ? link.Output : link.Input;
                            if (!visited.Contains(adjacentPin))
                            {
                                stack.Push(adjacentPin);
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}