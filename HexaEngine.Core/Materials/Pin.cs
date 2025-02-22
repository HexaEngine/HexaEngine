﻿namespace HexaEngine.Materials
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

        public Pin(int id, string name, PinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue)
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
            if (links.Contains(link)) return;
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
            if (other.Type == PinType.DontCare)
            {
                return true;
            }
            else if (other.Type == PinType.TextureAny)
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