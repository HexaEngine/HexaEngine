namespace HexaEngine.Editor.NodeEditor
{
    using HexaEngine.Core.Graphics.Reflection;
    using ImGuiNET;
    using ImNodesNET;
    using System.Collections.Generic;

    public class NodeEditor
    {
        private readonly List<Node> nodes = new();
        private readonly List<Pin> pins = new();
        private readonly List<Link> links = new();
        private int IdState;

        public bool isDragging;
        public Pin? Pin;
        private readonly nint context;

        public NodeEditor()
        {
            context = ImNodes.EditorContextCreate();
        }

        ~NodeEditor()
        {
            ImNodes.EditorContextFree(context);
        }

        public event EventHandler<Node>? NodeAdded;

        public event EventHandler<Node>? NodeRemoved;

        public event EventHandler<Pin>? PinAdded;

        public event EventHandler<Pin>? PinRemoved;

        public event EventHandler<Link>? LinkAdded;

        public event EventHandler<Link>? LinkRemoved;

        public IReadOnlyList<Node> Nodes => nodes;

        public IReadOnlyList<Pin> Pins => pins;

        public IReadOnlyList<Link> Links => links;

        public int GetUniqueId()
        {
            return IdState++;
        }

        public Node GetNode(int id)
        {
            return nodes.Find(x => x.Id == id) ?? throw new();
        }

        public Pin GetPin(int id)
        {
            return pins.Find(x => x.Id == id) ?? throw new();
        }

        public Link GetLink(int id)
        {
            return links.Find(x => x.Id == id) ?? throw new();
        }

        public Node CreateNode(string name, bool removable = true, bool isStatic = false)
        {
            Node node = new(this, name, removable, isStatic);
            nodes.Add(node);
            NodeAdded?.Invoke(this, node);
            return node;
        }

        public void AddNode(Node node)
        {
            nodes.Add(node);
            NodeAdded?.Invoke(this, node);
        }

        public void RemoveNode(Node node)
        {
            nodes.Remove(node);
            NodeRemoved?.Invoke(this, node);
        }

        public void AddPin(Pin pin)
        {
            pins.Add(pin);
            PinAdded?.Invoke(this, pin);
        }

        public void RemovePin(Pin pin)
        {
            pins.Remove(pin);
            PinRemoved?.Invoke(this, pin);
        }

        public void AddLink(Link link)
        {
            links.Add(link);
            LinkAdded?.Invoke(this, link);
        }

        public void RemoveLink(Link link)
        {
            links.Remove(link);
            LinkRemoved?.Invoke(this, link);
        }

        public Link CreateLink(Pin input, Pin output)
        {
            Link link = new(this, output.Parent, output, input.Parent, input);
            links.Add(link);
            output.Parent.AddLink(link);
            output.AddLink(link);
            input.Parent.AddLink(link);
            input.AddLink(link);
            LinkAdded?.Invoke(this, link);
            return link;
        }

        public static PinType GetPinType(ShaderInputBindDescription desc)
        {
            return desc.Type switch
            {
                ShaderInputType.SitCbuffer => PinType.Object,
                ShaderInputType.SitTbuffer => PinType.Object,
                ShaderInputType.SitTexture => GetPinType(desc.Dimension),
                ShaderInputType.SitSampler => PinType.Sampler,
                ShaderInputType.SitUavRwtyped => throw new NotImplementedException(),
                ShaderInputType.SitStructured => throw new NotImplementedException(),
                ShaderInputType.SitUavRwstructured => throw new NotImplementedException(),
                ShaderInputType.SitByteaddress => throw new NotImplementedException(),
                ShaderInputType.SitUavRwbyteaddress => throw new NotImplementedException(),
                ShaderInputType.SitUavAppendStructured => throw new NotImplementedException(),
                ShaderInputType.SitUavConsumeStructured => throw new NotImplementedException(),
                ShaderInputType.SitUavRwstructuredWithCounter => throw new NotImplementedException(),
                ShaderInputType.SitRtaccelerationstructure => throw new NotImplementedException(),
                ShaderInputType.SitUavFeedbacktexture => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
        }

        public static PinType GetPinType(SrvDimension dimension)
        {
            return dimension switch
            {
                SrvDimension.Unknown => PinType.Object,
                SrvDimension.Buffer => PinType.Buffer,
                SrvDimension.Texture1D => PinType.Texture1D,
                SrvDimension.Texture1Darray => PinType.Texture1DArray,
                SrvDimension.Texture2D => PinType.Texture2D,
                SrvDimension.Texture2Darray => PinType.Texture2DArray,
                SrvDimension.Texture2Dms => PinType.Texture2DMS,
                SrvDimension.Texture2Dmsarray => PinType.Texture2DMSArray,
                SrvDimension.Texture3D => PinType.Texture3D,
                SrvDimension.Texturecube => PinType.TextureCube,
                SrvDimension.Texturecubearray => PinType.TextureCubeArray,
                SrvDimension.Bufferex => PinType.Buffer,
                _ => throw new NotImplementedException(),
            };
        }

        private PinType GetPinType(SignatureParameterDescription output)
        {
            return output.SystemValueType switch
            {
                Name.Target => PinType.TextureAny,
                Name.RenderTargetArrayIndex => PinType.TextureAny,
                _ => throw new NotImplementedException(),
            };
        }

        /*
        public Node CreateFromPipeline<T>(T pipeline, string dbgName, bool removable = true, bool isStatic = false) where T : Pipeline
        {
            Node node = new(this, dbgName, removable, isStatic);
            nodes.Add(node);
            NodeAdded?.Invoke(this, node);

            uint register = 0;
            if (pipeline.output != null)
                for (int i = 0; i < pipeline.output.Length; i++)
                {
                    var output = pipeline.output[i];
                    if (output.Load != register || i == 0)
                    {
                        if (output.SystemValueType == DebugName.Undefined) continue;
                        node.CreatePin($"{output.SemanticName} : {output.Load}", PinKind.Output, GetPinType(output), PinShape.Circle);
                        register = output.Load;
                    }
                }
            for (int i = 0; i < pipeline.inputs.Count; i++)
            {
                var key = pipeline.inputs.Keys.ElementAt(i);
                for (int j = 0; j < pipeline.inputs[key].Length; j++)
                {
                    var input = pipeline.inputs[key][j];
                    if (input.Dimension == SrvDimension.Unknown) continue;
                    node.CreatePin(input.DebugName, PinKind.Input, GetPinType(input), PinShape.Circle);
                }
            }

            return node;
        }

        public Node CreateFromEffect<T>(T pipeline, string dbgName, bool removable = true, bool isStatic = false) where T : Effect
        {
            Node node = new(this, dbgName, removable, isStatic);
            nodes.Add(node);
            NodeAdded?.Invoke(this, node);

            uint register = 0;
            if (pipeline.output != null)
                for (int i = 0; i < pipeline.output.Length; i++)
                {
                    var output = pipeline.output[i];
                    if (output.Load != register || i == 0)
                    {
                        if (output.SystemValueType == DebugName.Undefined) continue;
                        node.CreatePin($"{output.SemanticName} : {output.Load}", PinKind.Output, GetPinType(output), PinShape.Circle);
                        register = output.Load;
                    }
                }
            if (pipeline.inputs != null)
                for (int i = 0; i < pipeline.inputs.Count; i++)
                {
                    var key = pipeline.inputs.Keys.ElementAt(i);
                    for (int j = 0; j < pipeline.inputs[key].Length; j++)
                    {
                        var input = pipeline.inputs[key][j];
                        if (input.Dimension == SrvDimension.Unknown) continue;
                        node.CreatePin(input.DebugName, PinKind.Input, GetPinType(input), PinShape.Circle);
                    }
                }

            return node;
        }
        */

        public void Draw()
        {
            ImNodes.EditorContextSet(context);
            ImNodes.BeginNodeEditor();
            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].Draw();
            }
            for (int i = 0; i < Links.Count; i++)
            {
                Links[i].Draw();
            }

            ImNodes.EndNodeEditor();

            int idNode1 = 0;
            int idNode2 = 0;
            int idpin1 = 0;
            int idpin2 = 0;
            if (ImNodes.IsLinkCreated(ref idNode1, ref idpin1, ref idNode2, ref idpin2))
            {
                var pino = GetNode(idNode1).GetOuput(idpin1);
                var pini = GetNode(idNode2).GetInput(idpin2);
                if (pino.CanCreateLink(pini))
                    CreateLink(pini, pino);
            }
            int idLink = 0;
            if (ImNodes.IsLinkDestroyed(ref idLink))
            {
                GetLink(idLink).Destroy();
            }
            if (ImGui.IsKeyPressed(ImGuiKey.Delete))
            {
                int numLinks = ImNodes.NumSelectedLinks();
                if (numLinks != 0)
                {
                    int[] links = new int[numLinks];
                    ImNodes.GetSelectedLinks(ref links[0]);
                    for (int i = 0; i < links.Length; i++)
                    {
                        GetLink(links[i]).Destroy();
                    }
                }
                int numNodes = ImNodes.NumSelectedNodes();
                if (numNodes != 0)
                {
                    int[] nodes = new int[numNodes];
                    ImNodes.GetSelectedNodes(ref nodes[0]);
                    for (int i = 0; i < nodes.Length; i++)
                    {
                        GetNode(nodes[i]).Destroy();
                    }
                }
            }
            int idpinStart = 0;
            if (ImNodes.IsLinkStarted(ref idpinStart))
            {
            }

            ImNodes.EditorContextSet((nint)null);
        }

        public void Destroy()
        {
            var nodes = this.nodes.ToArray();
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].Destroy();
            }
            this.nodes.Clear();
        }

        public static bool Validate(Pin startPin, Pin endPin)
        {
            Node node = startPin.Parent;
            Stack<(int, Node)> walkstack = new();
            walkstack.Push((0, node));
            while (walkstack.Count > 0)
            {
                (int i, node) = walkstack.Pop();
                if (i > node.Links.Count)
                    continue;
                Link link = node.Links[i];
                i++;
                walkstack.Push((i, node));
                if (link.SourceNode == node)
                {
                    if (link.Output == endPin)
                        return true;
                    else
                        walkstack.Push((0, link.TargetNode));
                }
            }

            return false;
        }

        public static Node[] TreeTraversal(Node root, bool includeStatic)
        {
            Stack<Node> stack1 = new();
            Stack<Node> stack2 = new();

            Node node = root;
            stack1.Push(node);
            while (stack1.Count != 0)
            {
                node = stack1.Pop();
                if (stack2.Contains(node))
                {
                    RemoveFromStack(stack2, node);
                }
                stack2.Push(node);

                for (int i = 0; i < node.Links.Count; i++)
                {
                    if (node.Links[i].TargetNode == node)
                    {
                        var src = node.Links[i].SourceNode;
                        if (includeStatic && src.IsStatic || !src.IsStatic)
                            stack1.Push(node.Links[i].SourceNode);
                    }
                }
            }

            return stack2.ToArray();
        }

        public static Node[][] TreeTraversal2(Node root, bool includeStatic)
        {
            Stack<(int, Node)> stack1 = new();
            Stack<(int, Node)> stack2 = new();

            int priority = 0;
            Node node = root;
            stack1.Push((priority, node));
            int groups = 0;
            while (stack1.Count != 0)
            {
                (priority, node) = stack1.Pop();
                var n = FindStack(stack2, x => x.Item2 == node);
                if (n.Item2 != null && n.Item1 < priority)
                {
                    RemoveFromStack(stack2, x => x.Item2 == node);
                    stack2.Push((priority, node));
                }
                else if (n.Item2 == null)
                {
                    stack2.Push((priority, node));
                }

                for (int i = 0; i < node.Links.Count; i++)
                {
                    if (node.Links[i].TargetNode == node)
                    {
                        var src = node.Links[i].SourceNode;
                        if (includeStatic && src.IsStatic || !src.IsStatic)
                            stack1.Push((priority + 1, node.Links[i].SourceNode));
                    }
                }

                if (groups < priority)
                    groups = priority;
            }
            groups++;
            Node[][] nodes = new Node[groups][];

            var pNodes = stack2.ToArray();

            for (int i = 0; i < groups; i++)
            {
                List<Node> group = new();
                for (int j = 0; j < pNodes.Length; j++)
                {
                    if (pNodes[j].Item1 == i)
                        group.Add(pNodes[j].Item2);
                }
                nodes[i] = group.ToArray();
            }

            return nodes;
        }

        public static void RemoveFromStack<T>(Stack<T> values, T value) where T : class
        {
            Stack<T> swap = new();
            while (values.Count > 0)
            {
                var val = values.Pop();
                if (val.Equals(value))
                    break;
                swap.Push(val);
            }
            while (swap.Count > 0)
            {
                values.Push(swap.Pop());
            }
        }

        public static void RemoveFromStack2<T>(Stack<T> values, T value) where T : IEquatable<T>
        {
            Stack<T> swap = new();
            while (values.Count > 0)
            {
                var val = values.Pop();
                if (val.Equals(value))
                    break;
                swap.Push(val);
            }
            while (swap.Count > 0)
            {
                values.Push(swap.Pop());
            }
        }

        public static void RemoveFromStack<T>(Stack<T> values, Func<T, bool> compare)
        {
            Stack<T> swap = new();
            while (values.Count > 0)
            {
                var val = values.Pop();
                if (compare(val))
                    break;
                swap.Push(val);
            }
            while (swap.Count > 0)
            {
                values.Push(swap.Pop());
            }
        }

        public static T FindStack<T>(Stack<T> values, Func<T, bool> compare)
        {
            for (int i = 0; i < values.Count; i++)
            {
                var value = values.ElementAt(i);
                if (compare(value))
                    return value;
            }
            return default;
        }
    }
}