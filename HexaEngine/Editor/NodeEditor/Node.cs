namespace HexaEngine.Editor.NodeEditor
{
    using ImGuiNET;
    using ImNodesNET;
    using System.Collections.Generic;

    public class Node
    {
        public readonly int Id;
        public string Name;
        private bool isEditing;
        public readonly NodeEditor Graph;
        private readonly List<Pin> Pins = new();
        private readonly List<Link> links = new();
        public readonly bool Removable = true;
        public readonly bool IsStatic;

        public Node(NodeEditor graph, string name, bool removable, bool isStatic)
        {
            Graph = graph;
            Id = graph.GetUniqueId();
            Name = name;
            Removable = removable;
            IsStatic = isStatic;
        }

        public event EventHandler<Pin>? PinAdded;

        public event EventHandler<Pin>? PinRemoved;

        public event EventHandler<Link>? LinkAdded;

        public event EventHandler<Link>? LinkRemoved;

        public IReadOnlyList<Link> Links => links;

        public bool IsEditing
        {
            get => isEditing;
            set
            {
                isEditing = value;
            }
        }

        public Pin GetInput(int id)
        {
            return Pins.Find(x => x.Id == id && x.Kind == PinKind.Input) ?? throw new();
        }

        public Pin GetOuput(int id)
        {
            return Pins.Find(x => x.Id == id && x.Kind == PinKind.Output) ?? throw new();
        }

        public static Link? FindSourceLink(Pin pin, Node other)
        {
            for (int i = 0; i < pin.Links.Count; i++)
            {
                Link link = pin.Links[i];
                if (link.SourceNode == other)
                    return link;
            }
            return null;
        }

        public static Link? FindTargetLink(Pin pin, Node other)
        {
            for (int i = 0; i < pin.Links.Count; i++)
            {
                Link link = pin.Links[i];
                if (link.TargetNode == other)
                    return link;
            }
            return null;
        }

        public virtual Pin CreatePin(string name, PinKind kind, PinType type, PinShape shape, uint maxLinks = uint.MaxValue)
        {
            Pin pin = new(Graph, this, name, shape, kind, type, maxLinks);
            Pins.Add(pin);
            Graph.AddPin(pin);
            PinAdded?.Invoke(this, pin);
            return pin;
        }

        public virtual void DestroyPin(Pin pin)
        {
            pin.Destroy();
            Pins.Remove(pin);
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
            for (int i = 0; i < Pins.Count; i++)
            {
                var pin = Pins[i];
                var links = pin.Links.ToArray();
                for (int j = 0; j < links.Length; j++)
                {
                    links[j].Destroy();
                }
                Pins.Remove(pin);
                Graph.RemovePin(pin);
            }
            Graph.RemoveNode(this);
        }

        public virtual void Draw()
        {
            ImNodes.BeginNode(Id);
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
                if (ImGui.SmallButton("Edit")) // TODO: Replace with icon
                {
                    isEditing = true;
                }
            }
            //if (ImGui.InputText("Name"))

            ImNodes.EndNodeTitleBar();

            DrawContentBeforePins();

            for (int i = 0; i < Pins.Count; i++)
            {
                Pins[i].Draw();
            }

            DrawContent();

            ImNodes.EndNode();
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