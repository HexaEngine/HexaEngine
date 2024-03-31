namespace HexaEngine.Editor.NodeEditor
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImNodes;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class NodeEditor
    {
        private bool initialized = false;
        private string? state;
        private ImNodesEditorContextPtr context;

        private readonly List<Node> nodes = new();
        private readonly List<Link> links = new();
        private int currentId;

        public NodeEditor()
        {
        }

        public event EventHandler<Pin>? NodePinValueChanging;

        public event EventHandler<Pin>? NodePinValueChanged;

        public event EventHandler<Node>? NodeValueChanging;

        public event EventHandler<Node>? NodeValueChanged;

        public event EventHandler<NodeEditor>? ValueChanging;

        public event EventHandler<NodeEditor>? ValueChanged;

        public event EventHandler<Node>? NodeAdded;

        public event EventHandler<Node>? NodeRemoved;

        public event EventHandler<Link>? LinkAdded;

        public event EventHandler<Link>? LinkRemoved;

        public List<Node> Nodes => nodes;

        [JsonIgnore]
        public List<Link> Links => links;

        public int CurrentId { get => currentId; set => currentId = value; }

        public bool Minimap { get; set; }

        public ImNodesMiniMapLocation Location { get; set; }

        public class NodeEditorSerializationContainer
        {
            public NodeEditorSerializationContainer(NodeEditor editor)
            {
                for (int i = 0; i < editor.Nodes.Count; i++)
                {
                    var node = editor.Nodes[i];
                    Nodes.Add(node);
                    var list = PinMap[i] = new();
                    for (int j = 0; j < node.Pins.Count; j++)
                    {
                        var pin = node.Pins[j];
                        list.Add(pin);
                    }
                }
                for (int i = 0; i < editor.Links.Count; i++)
                {
                    Links.Add(editor.Links[i].GetId());
                }
                State = editor.SaveState();
                CurrentId = editor.CurrentId;
            }

            [JsonConstructor]
            public NodeEditorSerializationContainer()
            {
            }

            public List<Node> Nodes { get; } = new();

            public Dictionary<int, List<Pin>> PinMap { get; } = new();

            public List<LinkId> Links { get; } = new();

            public string State { get; set; }

            public int CurrentId { get; set; }

            public void Build(NodeEditor editor)
            {
                for (int i = 0; i < Nodes.Count; i++)
                {
                    var node = Nodes[i];
                    editor.Nodes.Add(node);
                    var list = PinMap[i];
                    for (int j = 0; j < list.Count; j++)
                    {
                        node.AddPin(list[j]);
                    }
                }
                editor.Initialize(false);
                for (int i = 0; i < Links.Count; i++)
                {
                    editor.CreateLinkFromId(Links[i]);
                }
                editor.RestoreState(State);
                editor.CurrentId = CurrentId;
            }
        }

        private void OnPinValueChanged(object? sender, Pin e)
        {
            NodePinValueChanged?.Invoke(this, e);
            OnValueChanged(e);
        }

        private void OnPinValueChanging(object? sender, Pin e)
        {
            NodePinValueChanging?.Invoke(this, e);
            OnValueChanging(e);
        }

        private void OnNodeValueChanged(object? sender, Node e)
        {
            NodeValueChanged?.Invoke(this, e);
            OnValueChanged(e);
        }

        private void OnNodeValueChanging(object? sender, Node e)
        {
            NodeValueChanging?.Invoke(this, e);
            OnValueChanging(e);
        }

        private void OnValueChanged(object? sender)
        {
            ValueChanged?.Invoke(sender, this);
        }

        private void OnValueChanging(object? sender)
        {
            ValueChanging?.Invoke(sender, this);
        }

        public string Serialize()
        {
            NodeEditorSerializationContainer container = new(this);
            JsonSerializerSettings settings = new()
            {
                Formatting = Formatting.Indented,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                CheckAdditionalContent = true,
                MetadataPropertyHandling = MetadataPropertyHandling.Default,
                NullValueHandling = NullValueHandling.Include,
                MaxDepth = int.MaxValue
            };

            return JsonConvert.SerializeObject(container, settings);
        }

        public static NodeEditor Deserialize(string json)
        {
            JsonSerializerSettings settings = new()
            {
                Formatting = Formatting.Indented,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                CheckAdditionalContent = true,
                MetadataPropertyHandling = MetadataPropertyHandling.Default,
                NullValueHandling = NullValueHandling.Include,
                MaxDepth = int.MaxValue
            };

            var container = JsonConvert.DeserializeObject<NodeEditorSerializationContainer>(json, settings);
            NodeEditor editor = new();
            container.Build(editor);
            return editor;
        }

        public virtual void Initialize(bool createContext = true)
        {
            if (context.IsNull)
            {
                if (createContext)
                    context = ImNodes.EditorContextCreate();

                if (!initialized)
                {
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        nodes[i].Initialize(this);
                        nodes[i].PinValueChanging += OnPinValueChanging;
                        nodes[i].PinValueChanged += OnPinValueChanged;
                        nodes[i].NodeValueChanging += OnNodeValueChanging;
                        nodes[i].NodeValueChanged += OnNodeValueChanged;
                    }
                    for (int i = 0; i < links.Count; i++)
                    {
                        links[i].Initialize(this);
                    }
                    initialized = true;
                }
            }
        }

        public int GetUniqueId()
        {
            return currentId++;
        }

        public Node GetNode(int id)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];
                if (node.Id == id)
                    return node;
            }
            throw new();
        }

        public T GetNode<T>() where T : Node
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node is T t)
                    return t;
            }
            throw new KeyNotFoundException();
        }

        public IEnumerable<T> GetNodes<T>() where T : Node
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node is T t)
                    yield return t;
            }
        }

        public Link GetLink(int id)
        {
            for (int i = 0; i < links.Count; i++)
            {
                var link = links[i];
                if (link.Id == id)
                    return link;
            }

            throw new KeyNotFoundException();
        }

        public Node CreateNode(string name, bool removable = true, bool isStatic = false)
        {
            Node node = new(GetUniqueId(), name, removable, isStatic);
            AddNode(node);
            return node;
        }

        public void AddNode(Node node)
        {
            if (initialized)
            {
                node.Initialize(this);
                node.PinValueChanging += OnPinValueChanging;
                node.PinValueChanged += OnPinValueChanged;
                node.NodeValueChanging += OnNodeValueChanging;
                node.NodeValueChanged += OnNodeValueChanged;
            }

            nodes.Add(node);
            NodeAdded?.Invoke(this, node);
        }

        public void RemoveNode(Node node)
        {
            nodes.Remove(node);
            NodeRemoved?.Invoke(this, node);
        }

        public void AddLink(Link link)
        {
            if (initialized)
            {
                link.Initialize(this);
            }

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
            Link link = new(GetUniqueId(), output.Parent, output, input.Parent, input);
            AddLink(link);
            return link;
        }

        private Link CreateLinkFromId(LinkId id)
        {
            var output = GetNode(id.IdOutputNode).GetOutput(id.IdOutput);
            var input = GetNode(id.IdInputNode).GetInput(id.IdInput);
            Link link = new(id.Id, output.Parent, output, input.Parent, input);
            AddLink(link);
            return link;
        }

        public unsafe string SaveState()
        {
            return ImNodes.SaveEditorStateToIniStringS(context, null);
        }

        public void RestoreState(string state)
        {
            if (context.IsNull)
            {
                this.state = state;
                return;
            }
            ImNodes.LoadEditorStateFromIniString(context, state, (uint)state.Length);
        }

        public void BeginModify()
        {
            ImNodes.EditorContextSet(context);
        }

        public void EndModify()
        {
            ImNodes.EditorContextSet(null);
        }

        public void Draw()
        {
            if (context.IsNull)
                Initialize();
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
            if (Minimap)
                ImNodes.MiniMap(1, Location, null, default);
            ImNodes.EndNodeEditor();

            int idNode1 = 0;
            int idNode2 = 0;
            int idpin1 = 0;
            int idpin2 = 0;
            bool createdFromSpan = false;
            if (ImNodes.IsLinkCreatedIntPtr(ref idNode1, ref idpin1, ref idNode2, ref idpin2, ref createdFromSpan))
            {
                var pino = GetNode(idNode1).GetOutput(idpin1);
                var pini = GetNode(idNode2).GetInput(idpin2);
                if (pini.CanCreateLink(pino) && pino.CanCreateLink(pini))
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
                        var node = GetNode(nodes[i]);
                        if (node.Removable)
                        {
                            node.Destroy();
                        }
                    }
                }
            }
            int idpinStart = 0;
            if (ImNodes.IsLinkStarted(ref idpinStart))
            {
            }

            for (int i = 0; i < Nodes.Count; i++)
            {
                var id = Nodes[i].Id;
                Nodes[i].IsHovered = ImNodes.IsNodeHovered(ref id);
            }

            ImNodes.EditorContextSet(null);

            if (state != null)
            {
                RestoreState(state);
                state = null;
            }
        }

        public void Destroy()
        {
            var nodes = this.nodes.ToArray();
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].PinValueChanged -= OnPinValueChanged;
                nodes[i].PinValueChanging -= OnPinValueChanging;
                nodes[i].NodeValueChanging -= OnNodeValueChanging;
                nodes[i].NodeValueChanged -= OnNodeValueChanged;
                nodes[i].Destroy();
            }
            this.nodes.Clear();
            if (!context.IsNull)
                ImNodes.EditorContextFree(context);
            context = null;
            initialized = false;
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
                if (link.OutputNode == node)
                {
                    if (link.Output == endPin)
                        return true;
                    else
                        walkstack.Push((0, link.InputNode));
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
                    if (node.Links[i].InputNode == node)
                    {
                        var src = node.Links[i].OutputNode;
                        if (includeStatic && src.IsStatic || !src.IsStatic)
                            stack1.Push(node.Links[i].OutputNode);
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
                    if (node.Links[i].InputNode == node)
                    {
                        var src = node.Links[i].OutputNode;
                        if (includeStatic && src.IsStatic || !src.IsStatic)
                            stack1.Push((priority + 1, node.Links[i].OutputNode));
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
#pragma warning disable CS8603 // Possible null reference return.
            return default;
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}