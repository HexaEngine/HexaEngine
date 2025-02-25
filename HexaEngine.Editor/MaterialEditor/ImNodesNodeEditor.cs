﻿namespace HexaEngine.Editor.MaterialEditor
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImNodes;
    using HexaEngine.Core.Materials.Nodes.Functions;
    using HexaEngine.Core.Utilities;
    using HexaEngine.Editor.MaterialEditor.Pins;
    using HexaEngine.Materials;
    using HexaEngine.Materials.Pins;
    using System.Numerics;

    public struct LinkStartedEventArgs
    {
        public Pin Pin;
        public Node Node;

        public LinkStartedEventArgs(Pin pin, Node node)
        {
            Pin = pin;
            Node = node;
        }
    }

    public struct LinkDroppedEventArgs
    {
        public Pin Pin;
        public Node Node;

        public LinkDroppedEventArgs(Pin pin, Node node)
        {
            Pin = pin;
            Node = node;
        }
    }

    public delegate void DrawContextMenuCallback(ImNodesNodeEditor editor, Pin? droppedPin, Vector2 mousePos);

    public class ImNodesNodeEditor : NodeEditor
    {
        private ImNodesEditorContextPtr context;
        private string? state;
        private DrawContextMenuCallback? drawContextMenuCallback;
        private Pin? droppedPin;

        public ImNodesNodeEditor()
        {
            NodeEditorRegistry.SetDefaultRenderers(new BaseNodeRenderer(), new BasePinRenderer());
            NodeEditorRegistry.RegisterPinSingleton<BoolPin, BoolPinRenderer>();
            NodeEditorRegistry.RegisterPinSingleton<DoublePin, DoublePinRenderer>();
            NodeEditorRegistry.RegisterPinSingleton<FloatPin, FloatPinRenderer>();
            NodeEditorRegistry.RegisterPinSingleton<HalfPin, HalfPinRenderer>();
            NodeEditorRegistry.RegisterPinSingleton<IntPin, IntPinRenderer>();
            NodeEditorRegistry.RegisterPinSingleton<UIntPin, UIntPinRenderer>();
            NodeEditorRegistry.RegisterPinSingleton<UniversalPin, UniversalPinRenderer>();
        }

        public event EventHandler<LinkStartedEventArgs>? LinkStarted;

        public event EventHandler<LinkDroppedEventArgs>? LinkDropped;

        public DrawContextMenuCallback? DrawContextMenuCallback { get => drawContextMenuCallback; set => drawContextMenuCallback = value; }

        public ImNodesMiniMapLocation Location { get; set; }

        public bool Minimap { get; set; }

        public override void BeginModify()
        {
            ImNodes.EditorContextSet(context);
        }

        public override void EndModify()
        {
            ImNodes.EditorContextSet(null);
        }

        public new static ImNodesNodeEditor Deserialize(string json)
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
                MissingMemberHandling = MissingMemberHandling.Ignore,
                MaxDepth = int.MaxValue
            };

            var container = JsonConvert.DeserializeObject<NodeContainer>(json, settings)!;
            ImNodesNodeEditor editor = new();
            container.Build(editor);
            return editor;
        }

        public override unsafe string SaveState()
        {
            return ImNodes.SaveEditorStateToIniStringS(context, null);
        }

        public override void RestoreState(string? state)
        {
            if (state == null)
            {
                return;
            }

            if (context.IsNull)
            {
                this.state = state;
                return;
            }
            ImNodes.LoadEditorStateFromIniString(context, state, (uint)state.Length);
        }

        public override void Initialize(bool createContext = true)
        {
            if (context.IsNull)
            {
                if (createContext)
                {
                    context = ImNodes.EditorContextCreate();
                }

                base.Initialize(createContext);
            }
        }

        private Vector2 contextMenuMousePos;
        private bool contextMenuOpen = false;

        public override void Draw()
        {
            if (context.IsNull)
            {
                Initialize();
            }

            ImNodes.EditorContextSet(context);
            ImNodes.BeginNodeEditor();

            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4f);

            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Draw();
            }

            ImGui.PopStyleVar();

            for (int i = 0; i < links.Count; i++)
            {
                var link = links[i];
                ImNodes.Link(link.Id, link.OutputId, link.InputId);
            }
            if (Minimap)
            {
                ImNodes.MiniMap(0.2f, Location, null, default);
            }

            if (droppedPin != null && !contextMenuOpen)
            {
                ImGui.OpenPopup("NodeEditorContextPopup"u8);
            }

            if (ImGui.BeginPopupContextWindow("NodeEditorContextPopup"u8))
            {
                if (!contextMenuOpen)
                {
                    contextMenuMousePos = ImGui.GetMousePos();
                }

                contextMenuOpen = true;
                drawContextMenuCallback?.Invoke(this, droppedPin, contextMenuMousePos);
                ImGui.EndPopup();
            }
            else
            {
                contextMenuOpen = false;
                droppedPin = null;
            }

            ImNodes.EndNodeEditor();

            int pinIdHovered = 0;
            if (ImNodes.IsPinHovered(ref pinIdHovered))
            {
                var pin = GetPin(pinIdHovered)!;
                ImGui.SetTooltip(EnumHelper<PinType>.GetName(pin.Type));
            }

            int nodeIdHovered = 0;
            if (ImNodes.IsNodeHovered(ref nodeIdHovered))
            {
                if (ImGuiP.IsMouseClicked(ImGuiMouseButton.Right))
                {
                    var node = GetNode(nodeIdHovered)!;
                }
            }

            int idNode1 = 0;
            int idNode2 = 0;
            int idpin1 = 0;
            int idpin2 = 0;
            bool createdFromSpan = false;
            if (ImNodes.IsLinkCreated(ref idNode1, ref idpin1, ref idNode2, ref idpin2, ref createdFromSpan))
            {
                var pino = GetNode(idNode1)!.GetOutput(idpin1);
                var pini = GetNode(idNode2)!.GetInput(idpin2);
                if (pini.CanCreateLink(pino) && pino.CanCreateLink(pini))
                {
                    CreateLink(pini, pino);
                }
            }
            int idLink = 0;
            if (ImNodes.IsLinkDestroyed(ref idLink))
            {
                GetLink(idLink)?.Destroy();
            }
            if (ImGuiP.IsKeyPressed(ImGuiKey.Delete))
            {
                int numLinks = ImNodes.NumSelectedLinks();
                if (numLinks != 0)
                {
                    int[] links = new int[numLinks];
                    ImNodes.GetSelectedLinks(ref links[0]);
                    for (int i = 0; i < links.Length; i++)
                    {
                        GetLink(links[i])?.Destroy();
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
                        if (node != null && node.Removable)
                        {
                            node.Destroy();
                        }
                    }
                }
            }

            int idpinStart = 0;
            if (ImNodes.IsLinkStarted(ref idpinStart))
            {
                var pin = GetPin(idpinStart)!;
                LinkStarted?.Invoke(this, new LinkStartedEventArgs(pin, pin.Parent));
            }

            int startedAt = 0;
            if (ImNodes.IsLinkDropped(ref startedAt))
            {
                var pin = GetPin(startedAt)!;
                droppedPin = pin;
                LinkDropped?.Invoke(this, new LinkDroppedEventArgs(pin, pin.Parent));
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                var id = nodes[i].Id;
                nodes[i].IsHovered = ImNodes.IsNodeHovered(ref id);
            }

            ImNodes.EditorContextSet(null);

            if (state != null)
            {
                RestoreState(state);
                state = null;
            }
        }

        public override void Destroy()
        {
            base.Destroy();

            if (!context.IsNull)
            {
                ImNodes.EditorContextFree(context);
            }

            context = null;
        }
    }
}