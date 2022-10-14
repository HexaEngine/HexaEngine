namespace HexaEngine.Editor.Nodes
{
    using ImGuiNET;
    using imnodesNET;
    using System.Collections.Generic;

    public enum PinType
    {
        Flow,
        Bool,
        Int,
        Float,
        String,
        Object,
        Function,
        Delegate,
        Texture1D,
        Texture1DArray,
        Texture2D,
        Texture2DArray,
        TextureCube,
        Texture3D,
        Vertices,
    }

    public enum PinKind
    {
        Input,
        Output,
        Static
    }

    public class Pin
    {
        public readonly int Id;
        public readonly Node Parent;
        public string Name;
        public PinShape Shape;
        public PinKind Kind;
        public PinType Type;
        public readonly Graph Graph;
        public readonly List<Link> Links = new();

        public Pin(int id, Node parent, string name, PinShape shape, PinKind kind, PinType type, Graph graph)
        {
            Id = id;
            Parent = parent;
            Name = name;
            Shape = shape;
            Kind = kind;
            Type = type;
            Graph = graph;
        }

        public bool CanCreateLink(Pin other)
        {
            if (Id == other.Id) return false;
            if (Type != other.Type) return false;
            if (Kind == other.Kind) return false;
            return true;
        }

        public void Draw()
        {
            if (Kind == PinKind.Input)
            {
                imnodes.BeginInputAttribute(Id, Shape);
                ImGui.Text(Name);
                imnodes.EndInputAttribute();
            }
            if (Kind == PinKind.Output)
            {
                imnodes.BeginOutputAttribute(Id, Shape);
                ImGui.Text(Name);
                imnodes.EndOutputAttribute();
            }
            if (Kind == PinKind.Static)
            {
                imnodes.BeginStaticAttribute(Id);
                ImGui.Text(Name);
                imnodes.EndStaticAttribute();
            }
        }
    }

    public class Link
    {
        public readonly int Id;
        public readonly Node SourceNode;
        public readonly int SourceNodeId;
        public readonly Pin Output;
        public readonly int OutputId;
        public readonly Node TargetNode;
        public readonly int TargetNodeId;
        public readonly Pin Input;
        public readonly int InputId;

        public Link(int id, Node sourceNode, Pin output, Node targetNode, Pin input)
        {
            Id = id;
            SourceNode = sourceNode;
            SourceNodeId = sourceNode.Id;
            Output = output;
            OutputId = output.Id;
            TargetNode = targetNode;
            TargetNodeId = targetNode.Id;
            Input = input;
            InputId = input.Id;
        }

        public void Draw()
        {
            imnodes.Link(Id, OutputId, InputId);
        }
    }

    public class Node
    {
        public readonly int Id;
        public string Name;
        public readonly Graph Graph;
        public readonly List<Pin> Pins = new();
        public readonly List<Link> Links = new();

        public Node(int id, string name, Graph graph)
        {
            Id = id;
            Name = name;
            Graph = graph;
        }

        public Pin GetInput(int id)
        {
            return Pins.Find(x => x.Id == id && x.Kind == PinKind.Input) ?? throw new();
        }

        public Pin GetOuput(int id)
        {
            return Pins.Find(x => x.Id == id && x.Kind == PinKind.Output) ?? throw new();
        }

        public Pin CreatePin(string name, PinKind kind, PinType type, PinShape shape)
        {
            Pin pin = new(Graph.GetUniqueId(), this, name, shape, kind, type, Graph);
            Pins.Add(pin);
            Graph.Pins.Add(pin);
            return pin;
        }

        public void DestroyPin(Pin pin)
        {
            var links = pin.Links.ToArray();
            for (int i = 0; i < links.Length; i++)
            {
                Graph.DestroyLink(links[i]);
            }
            Pins.Remove(pin);
            Graph.Pins.Remove(pin);
        }

        public virtual void Draw()
        {
            imnodes.BeginNode(Id);
            imnodes.BeginNodeTitleBar();
            ImGui.Text(Name);
            imnodes.EndNodeTitleBar();

            for (int i = 0; i < Pins.Count; i++)
            {
                Pins[i].Draw();
            }

            imnodes.EndNode();
        }
    }

    public class Graph
    {
        public readonly List<Node> Nodes = new();
        public readonly List<Pin> Pins = new();
        public readonly List<Link> Links = new();
        private int IdState;

        public bool isDragging;
        public Pin? Pin;

        public int GetUniqueId()
        {
            return IdState++;
        }

        public Node GetNode(int id)
        {
            return Nodes.Find(x => x.Id == id) ?? throw new();
        }

        public Pin GetPin(int id)
        {
            return Pins.Find(x => x.Id == id) ?? throw new();
        }

        public Link GetLink(int id)
        {
            return Links.Find(x => x.Id == id) ?? throw new();
        }

        public Node CreateNode(string name)
        {
            Node node = new(GetUniqueId(), name, this);
            Nodes.Add(node);
            return node;
        }

        public Link CreateLink(Pin input, Pin output)
        {
            Link link = new(GetUniqueId(), output.Parent, output, input.Parent, input);
            Links.Add(link);
            output.Parent.Links.Add(link);
            output.Links.Add(link);
            input.Parent.Links.Add(link);
            input.Links.Add(link);
            return link;
        }

        public void DestroyLink(Link link)
        {
            Links.Remove(link);
            link.SourceNode.Links.Remove(link);
            link.Output.Links.Remove(link);
            link.TargetNode.Links.Remove(link);
            link.Input.Links.Remove(link);
        }

        public void Draw()
        {
            imnodes.BeginNodeEditor();
            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].Draw();
            }
            for (int i = 0; i < Links.Count; i++)
            {
                Links[i].Draw();
            }

            imnodes.EndNodeEditor();

            int idNode1 = 0;
            int idNode2 = 0;
            int idpin1 = 0;
            int idpin2 = 0;
            if (imnodes.IsLinkCreated(ref idNode1, ref idpin1, ref idNode2, ref idpin2))
            {
                var pino = GetNode(idNode1).GetOuput(idpin1);
                var pini = GetNode(idNode2).GetInput(idpin2);
                if (pino.CanCreateLink(pini))
                    CreateLink(pini, pino);
            }
            int idLink = 0;
            if (imnodes.IsLinkDestroyed(ref idLink))
            {
                DestroyLink(GetLink(idLink));
            }
            if (imnodes.IsLinkHovered(ref idLink))
            {
                if (ImGui.IsKeyPressed(ImGuiKey.Delete))
                    DestroyLink(GetLink(idLink));
            }
            int idpinStart = 0;
            if (imnodes.IsLinkStarted(ref idpinStart))
            {
            }
        }
    }
}