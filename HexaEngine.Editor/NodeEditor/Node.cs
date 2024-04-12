namespace HexaEngine.Editor.NodeEditor
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImNodes;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Numerics;

    public class Node
    {
        private NodeEditor? editor;
        private int id;

        private bool isEditing;
        private readonly List<Pin> pins = new();
        private readonly List<Link> links = new();

        public readonly bool Removable = true;
        public readonly bool IsStatic;
        public string Name;
        public string OriginalName;

        public uint TitleColor = 0x6930c3ff;

        public uint TitleHoveredColor = 0x5e60ceff;

        public uint TitleSelectedColor = 0x7400b8ff;
        private Vector2 position;
        private Vector2 size;

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
        public List<Link> Links => links;

        [JsonIgnore]
        public List<Pin> Pins => pins;

        [JsonIgnore]
        public Vector2 Position { get => position; set => ImNodes.SetNodeEditorSpacePos(id, value); }

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

        protected void OnValueChanged()
        {
            NodeValueChanged?.Invoke(this, this);
        }

        protected void OnValueChanging()
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

        public virtual Pin CreatePin(NodeEditor editor, string name, PinKind kind, PinType type, ImNodesPinShape shape, uint maxLinks = uint.MaxValue)
        {
            Pin pin = new(editor.GetUniqueId(), name, shape, kind, type, maxLinks);
            return AddPin(pin);
        }

        public virtual Pin CreateOrGetPin(NodeEditor editor, string name, PinKind kind, PinType type, ImNodesPinShape shape, uint maxLinks = uint.MaxValue)
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
        }

        public virtual void RemoveLink(Link link)
        {
            links.Remove(link);
            LinkRemoved?.Invoke(this, link);
        }

        public virtual void Destroy()
        {
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
            editor.RemoveNode(this);
            editor = null;
        }

        public virtual void Draw()
        {
            ImNodes.PushColorStyle(ImNodesCol.TitleBar, TitleColor);
            ImNodes.PushColorStyle(ImNodesCol.TitleBarHovered, TitleHoveredColor);
            ImNodes.PushColorStyle(ImNodesCol.TitleBarSelected, TitleSelectedColor);
            ImNodes.BeginNode(id);
            ImNodes.BeginNodeTitleBar();
            if (isEditing)
            {
                string name = Name;
                ImGui.PushItemWidth(100);
                if (ImGui.InputText("Name", ref name, 256, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    Name = name;
                    isEditing = false;
                }
                ImGui.PopItemWidth();
            }
            else
            {
                ImGui.Text(Name);
                ImGui.SameLine();
                if (ImGui.SmallButton($"Edit##{id}")) // TODO: Replace with icon
                {
                    isEditing = true;
                }
                if (ImGui.BeginItemTooltip())
                {
                    ImGui.Text(OriginalName);
                    ImGui.EndTooltip();
                }
            }

            ImNodes.EndNodeTitleBar();

            DrawContentBeforePins();

            for (int i = 0; i < pins.Count; i++)
            {
                pins[i].Draw();
            }

            DrawContent();

            ImNodes.EndNode();
            ImNodes.PopColorStyle();

            position = ImNodes.GetNodeEditorSpacePos(id);
            size = ImNodes.GetNodeDimensions(id);
        }

        protected virtual void DrawContentBeforePins()
        {
        }

        protected virtual void DrawContent()
        {
        }

        public override string ToString()
        {
            return Name;
        }
    }
}