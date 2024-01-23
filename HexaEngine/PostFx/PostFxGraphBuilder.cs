namespace HexaEngine.PostFx
{
    using HexaEngine.Collections;
    using HexaEngine.Graphics.Graph;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class PostFxGraphBuilder
    {
        private readonly List<PostFxNode> nodes = new();
        private readonly TopologicalSorter<PostFxNode> topologicalSorter = new();

        public PostFxNode this[int index]
        {
            get => nodes[index];
            set => nodes[index] = value;
        }

        public void AddNode(PostFxNode node)
        {
            nodes.Add(node);
        }

        public bool RemoveNode(PostFxNode node)
        {
            return nodes.Remove(node);
        }

        public void RemoveNodeAt(int index)
        {
            nodes.RemoveAt(index);
        }

        public void Clear()
        {
            nodes.Clear();
        }

        public bool Contains(PostFxNode node)
        {
            return nodes.Contains(node);
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