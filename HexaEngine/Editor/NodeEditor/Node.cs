namespace HexaEngine.Editor.NodeEditor
{
    using ImGuiNET;
    using ImNodesNET;
    using System.Collections.Generic;

    public class Node
    {
        public readonly int Id;
        public string Name;
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

        public Pin GetInput(int id)
        {
            return Pins.Find(x => x.Id == id && x.Kind == PinKind.Input) ?? throw new();
        }

        public Pin GetOuput(int id)
        {
            return Pins.Find(x => x.Id == id && x.Kind == PinKind.Output) ?? throw new();
        }

        public virtual Pin CreatePin(string name, PinKind kind, PinType type, PinShape shape)
        {
            Pin pin = new(Graph, this, name, shape, kind, type);
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
            ImGui.Text(Name);
            ImNodes.EndNodeTitleBar();

            for (int i = 0; i < Pins.Count; i++)
            {
                Pins[i].Draw();
            }

            DrawContent();

            ImNodes.EndNode();
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