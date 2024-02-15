namespace HexaEngine.PostFx
{
    using HexaEngine.Collections;
    using HexaEngine.Graphics.Graph;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class PostFxGraph
    {
        private readonly List<PostFxNode> nodes = new();
        private readonly Dictionary<IPostFx, PostFxNode> effectToNode = new();
        private readonly List<PostFxNode> composeNodes = new();
        private readonly PostFxNameRegistry nameRegistry = new();
        private readonly TopologicalSorter<PostFxNode> topologicalSorter = new();

        public PostFxNode this[int index]
        {
            get => nodes[index];
            set => nodes[index] = value;
        }

        public PostFxNode Add(IPostFx fx)
        {
            nameRegistry.Add(fx);
            var node = new PostFxNode(this, fx, nameRegistry);
            nodes.Add(node);
            effectToNode.Add(fx, node);
            return node;
        }

        public void AddComposeNode(PostFxNode node)
        {
            composeNodes.Add(node);
        }

        public void RemoveComposeNode(PostFxNode node)
        {
            composeNodes.Remove(node);
            effectToNode.Remove(node.PostFx);
        }

        public void Remove(IPostFx fx)
        {
            nameRegistry.Remove(fx);

            if (effectToNode.TryGetValue(fx, out var node))
            {
                nodes.Remove(node);
                effectToNode.Remove(fx);
            }
        }

        public PostFxNode GetNode(IPostFx effect)
        {
            return effectToNode[effect];
        }

        public bool TryGetNode(IPostFx effect, [NotNullWhen(true)] out PostFxNode? node)
        {
            return effectToNode.TryGetValue(effect, out node);
        }

        public void Clear()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Clear(true);
            }
            nodes.Clear();
            effectToNode.Clear();
            nameRegistry.Clear();
        }

        public bool Contains(PostFxNode node)
        {
            return nodes.Contains(node);
        }

        private void RemoveComposeNodes()
        {
            composeNodes.Clear();
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node.IsComposeTarget)
                {
                    nodes.RemoveAt(i);
                    i--;
                }
            }
        }

        private void CheckOptional()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node.PostFx.Flags.HasFlag(PostFxFlags.Optional))
                {
                    node.PostFx.Enabled = node.Dependents.Count > 0;
                }
            }
        }

        public void Build(IList<IPostFx> effectsSorted)
        {
            RemoveComposeNodes();

            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Clear();
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                node.PostFx.SetupDependencies(node.Builder);

                if (node.Enabled)
                {
                    for (int j = 0; j < nodes.Count; j++)
                    {
                        var other = nodes[j];
                        if (node == other)
                        {
                            continue;
                        }

                        if (node.Builder.Overrides(other))
                        {
                            other.PostFx.Enabled = false;
                        }
                    }
                }
            }

            nodes.AddRange(composeNodes);

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node.Enabled || node.PostFx.Flags.HasFlag(PostFxFlags.Optional))
                {
                    node.Builder.Build(nodes, new List<ResourceBinding>());
                }
            }

            CheckOptional();

            var sorted = topologicalSorter.TopologicalSort(nodes);

            effectsSorted.Clear();
            for (int i = 0; i < sorted.Count; i++)
            {
                if (sorted[i].Enabled)
                {
                    effectsSorted.Add(sorted[i].PostFx);
                }
            }
        }
    }
}