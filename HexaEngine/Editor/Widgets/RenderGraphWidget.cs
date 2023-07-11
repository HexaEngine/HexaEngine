using HexaEngine.Rendering.Graph;

namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.ImGuiNET;
    using HexaEngine.ImNodesNET;
    using HexaEngine.Rendering.Renderers;
    using System.Collections.Generic;
    using System.Numerics;

    public class RenderGraphWidget : EditorWindow
    {
        private ImNodesEditorContextPtr nodeContext;
        private int currId;
        private readonly Dictionary<RenderPassOld, ImNode> nodes = new();
        private readonly Dictionary<(RenderPassOld, RenderPassOld), ImLink> links = new();
        private readonly Dictionary<string, ImAttribute> attributes = new();
        private readonly Dictionary<ResourceBinding, ImAttribute> resourceBindings = new();
        private ViewMode viewMode = ViewMode.NodeTree;

        private string viewSettings;

        private enum ViewMode
        {
            NodeTree,
            Waterfall,
            DependencyTree,
        }

        private struct ImNode
        {
            public int Id;
            public int Input;
            public int Output;

            public List<ImAttribute> Attributes = new();

            public ImNode(int id, int input, int output)
            {
                Id = id;
                Input = input;
                Output = output;
            }
        }

        private struct ImAttribute
        {
            public int Id;

            public ImAttribute(int id)
            {
                Id = id;
            }
        }

        private struct ImLink
        {
            public int Id;

            public ImLink(int id)
            {
                Id = id;
            }
        }

        public RenderGraphWidget()
        {
            nodeContext = ImNodes.EditorContextCreate();
            Flags = ImGuiWindowFlags.MenuBar;
        }

        protected override string Name { get; } = "Render Graph";

        private ImNode GetOrAddNode(RenderPassOld graphNode)
        {
            if (nodes.TryGetValue(graphNode, out ImNode node))
            {
                return node;
            }

            node = new(currId++, currId++, currId++);
            nodes.Add(graphNode, node);
            return node;
        }

        private ImLink GetOrAddLink(RenderPassOld a, RenderPassOld b)
        {
            if (links.TryGetValue((a, b), out ImLink link))
            {
                return link;
            }
            link = new(currId++);
            links.Add((a, b), link);
            return link;
        }

        private ImAttribute GetOrAddAttribute(ref ImNode imNode, ResourceBinding resourceBinding)
        {
            if (resourceBindings.TryGetValue((resourceBinding), out ImAttribute attribute))
            {
                return attribute;
            }
            attribute = new(currId++);
            imNode.Attributes.Add(attribute);
            return attribute;
        }

        private ImAttribute GetOrAddAttributeOutput(ref ImNode imNode, int index, string name)
        {
            if (imNode.Attributes.Count > index)
            {
                return imNode.Attributes[index];
            }
            ImAttribute attribute = new(currId++);
            imNode.Attributes.Add(attribute);
            attributes.Add(name, attribute);
            return attribute;
        }

        private ImAttribute GetByName(string name)
        {
            if (attributes.TryGetValue(name, out ImAttribute attribute))
            {
                return attribute;
            }
            return new(-1);
        }

        private ImLink GetLink(int v)
        {
            foreach (var a in links)
            {
                if (a.Value.Id == v)
                {
                    return a.Value;
                }
            }

            return default;
        }

        private void DestroyLink(int v)
        {
            foreach (var a in links)
            {
                if (a.Value.Id == v)
                {
                    a.Key.Item1.Dependencies.Remove(a.Key.Item2);
                    break;
                }
            }

            return;
        }

        public override void DrawContent(IGraphicsContext context)
        {
            var renderer = SceneRenderer.Current;
            if (renderer == null)
                return;

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("View"))
                {
                    if (ImGui.MenuItem("Node Tree"))
                    {
                        if (viewSettings != null)
                        {
                            ImNodes.LoadEditorStateFromIniString(nodeContext, viewSettings, (nuint)viewSettings.Length);
                        }
                        viewMode = ViewMode.NodeTree;
                        viewSettings = ImNodes.SaveEditorStateToIniStringS(nodeContext);
                    }
                    if (ImGui.MenuItem("Waterfall"))
                    {
                        viewMode = ViewMode.Waterfall;
                    }
                    if (ImGui.MenuItem("Dependency Tree"))
                    {
                        viewMode = ViewMode.DependencyTree;
                    }
                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            var renderGraph = renderer.RenderGraph;
            ImNodes.EditorContextSet(nodeContext);
            ImNodes.BeginNodeEditor();

            if (viewMode == ViewMode.NodeTree)
            {
                /*
                for (int i = 0; i < renderGraph.Sorted.Count; i++)
                {
                    var node = renderGraph.Sorted[i];
                    ImGui.BulletText(node.Name);
                }

                for (int i = 0; i < renderGraph.Nodes.Count; i++)
                {
                    var node = renderGraph.Nodes[i];
                    var imNode = GetOrAddNode(node);

                    ImNodes.BeginNode(imNode.Id);

                    ImNodes.BeginNodeTitleBar();
                    ImGui.Text(node.Name);
                    ImNodes.EndNodeTitleBar();

                    ImNodes.BeginInputAttribute(imNode.Input, ImNodesPinShape.CircleFilled);
                    ImNodes.EndInputAttribute();
                    ImGui.SameLine();
                    ImNodes.BeginOutputAttribute(imNode.Output, ImNodesPinShape.CircleFilled);
                    ImNodes.EndOutputAttribute();

                    for (int j = 0; j < node.Dependencies.Count; j++)
                    {
                        var dep = node.Dependencies[j];
                        var depNode = GetOrAddNode(dep);
                        var link = GetOrAddLink(node, dep);
                        ImNodes.Link(link.Id, imNode.Input, depNode.Output);
                    }

                    for (int j = 0; j < node.WriteDependencies.Count; j++)
                    {
                        var dep = node.WriteDependencies[j];
                        var atrri = GetOrAddAttribute(ref imNode, dep);
                        ImNodes.BeginOutputAttribute(atrri.Id, ImNodesPinShape.CircleFilled);
                        ImGui.Text(dep.Name);
                        ImNodes.EndOutputAttribute();
                    }

                    for (int j = 0; j < node.ReadDependencies.Count; j++)
                    {
                        var dep = node.ReadDependencies[j];
                        var atrri = GetOrAddAttribute(ref imNode, dep);
                        ImNodes.BeginInputAttribute(atrri.Id, ImNodesPinShape.CircleFilled);
                        ImGui.Text(dep.Name);
                        ImNodes.EndInputAttribute();
                    }

                    ImNodes.EndNode();
                }

                int idNode1 = 0;
                int idNode2 = 0;
                int idpin1 = 0;
                int idpin2 = 0;
                if (ImNodes.IsLinkCreated(ref idNode1, ref idpin1, ref idNode2, ref idpin2))
                {
                    var passo = GetRenderPassById(idNode1);
                    var passi = GetRenderPassById(idNode2);
                    passo.Dependencies.Add(passi);
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
                            DestroyLink(links[i]);
                        }
                    }

                    int numNodes = ImNodes.NumSelectedNodes();
                    if (numNodes != 0)
                    {
                        int[] nodes = new int[numNodes];
                        ImNodes.GetSelectedNodes(ref nodes[0]);
                        for (int i = 0; i < nodes.Length; i++)
                        {
                            var node = Get(nodes[i]);
                            if (node.Removable)
                            {
                                node.Destroy();
                            }
                        }
                    }
                }
            }

            if (viewMode == ViewMode.Waterfall)
            {
                int linkId = currId;
                var pos = Vector2.Zero;
                var prevId = -1;
                for (int i = 0; i < renderGraph.Sorted.Count; i++)
                {
                    var node = renderGraph.Sorted[i];
                    var imNode = GetOrAddNode(node);

                    ImNodes.BeginNode(imNode.Id);
                    ImNodes.SetNodeGridSpacePos(imNode.Id, pos);
                    ImNodes.BeginNodeTitleBar();
                    ImGui.Text(node.Name);
                    ImNodes.EndNodeTitleBar();

                    ImNodes.BeginInputAttribute(imNode.Input, ImNodesPinShape.CircleFilled);
                    ImNodes.EndInputAttribute();
                    ImGui.SameLine();
                    ImNodes.BeginOutputAttribute(imNode.Output, ImNodesPinShape.CircleFilled);
                    ImNodes.EndOutputAttribute();

                    if (prevId != -1)
                    {
                        ImNodes.Link(linkId++, imNode.Input, prevId);
                    }
                    prevId = imNode.Output;

                    ImNodes.EndNode();
                    pos += new Vector2(ImNodes.GetNodeDimensions(imNode.Id).X, 0) + new Vector2(20, 0);
                }
            }

            if (viewMode == ViewMode.DependencyTree)
            {
                var pos = Vector2.Zero;
                var posPrev = Vector2.Zero;
                var sizePrev = Vector2.Zero;
                RenderPass prev = null;
                int stackHeight = 0;
                for (int i = 0; i < renderGraph.Sorted.Count; i++)
                {
                    bool isDepended = true;
                    var node = renderGraph.Sorted[i];
                    var imNode = GetOrAddNode(node);

                    ImNodes.BeginNode(imNode.Id);

                    if (prev != null && !node.Dependencies.Contains(prev))
                    {
                        ImNodes.SetNodeGridSpacePos(imNode.Id, posPrev + new Vector2(0, sizePrev.Y + 20 + sizePrev.Y * stackHeight + 20 * stackHeight));
                        isDepended = false;
                        stackHeight++;
                    }
                    else
                    {
                        if (stackHeight > 0)
                        {
                            pos.X += sizePrev.X;
                        }
                        ImNodes.SetNodeGridSpacePos(imNode.Id, new(pos.X, 0));
                        stackHeight = 0;
                    }

                    ImNodes.BeginNodeTitleBar();
                    ImGui.Text(node.Name);
                    ImNodes.EndNodeTitleBar();

                    ImNodes.BeginInputAttribute(imNode.Input, ImNodesPinShape.CircleFilled);
                    ImNodes.EndInputAttribute();
                    ImGui.SameLine();
                    ImNodes.BeginOutputAttribute(imNode.Output, ImNodesPinShape.CircleFilled);
                    ImNodes.EndOutputAttribute();

                    for (int j = 0; j < node.Dependencies.Count; j++)
                    {
                        var dep = node.Dependencies[j];
                        var depNode = GetOrAddNode(dep);
                        var link = GetOrAddLink(node, dep);
                        ImNodes.Link(link.Id, imNode.Input, depNode.Output);
                    }

                    for (int j = 0; j < node.WriteDependencies.Count; j++)
                    {
                        var dep = node.WriteDependencies[j];
                        var atrri = GetOrAddAttribute(ref imNode, dep);
                        ImNodes.BeginOutputAttribute(atrri.Id, ImNodesPinShape.CircleFilled);
                        ImGui.Text(dep.Name);
                        ImNodes.EndOutputAttribute();
                    }

                    for (int j = 0; j < node.ReadDependencies.Count; j++)
                    {
                        var dep = node.ReadDependencies[j];
                        var atrri = GetOrAddAttribute(ref imNode, dep);
                        ImNodes.BeginInputAttribute(atrri.Id, ImNodesPinShape.CircleFilled);
                        ImGui.Text(dep.Name);
                        ImNodes.EndInputAttribute();
                    }

                    ImNodes.EndNode();
                    var size = ImNodes.GetNodeDimensions(imNode.Id);
                    if (isDepended)
                    {
                        posPrev = pos;
                        pos += new Vector2(size.X + 20, 0);
                        sizePrev = new(size.X, size.Y);
                    }
                    else
                    {
                        sizePrev = new(MathF.Max(size.X, sizePrev.X), size.Y);
                    }

                    prev = node;
                }*/
            }

            ImNodes.MiniMap(ImNodesMiniMapLocation.TopRight);
            ImNodes.EndNodeEditor();
        }

        private ImNode GetNode(int id)
        {
            foreach (var node in nodes)
            {
                if (node.Value.Id == id)
                {
                    return node.Value;
                }
            }

            return default;
        }

        private RenderPassOld GetRenderPassById(int id)
        {
            foreach (var node in nodes)
            {
                if (node.Value.Id == id)
                {
                    return node.Key;
                }
            }
            return default;
        }
    }
}