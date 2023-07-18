namespace HexaEngine.PostFx
{
    using HexaEngine.Collections;
    using HexaEngine.Rendering.Graph;
    using System.Collections.Generic;

    public class PostFxNode : INode
    {
        public PostFxNode(IPostFx postFx)
        {
            Name = postFx.Name;
            PostFx = postFx;
            Builder = new(this);
        }

        public string Name { get; }

        public IPostFx PostFx { get; set; }

        public bool Enabled => PostFx.Enabled;

        public List<PostFxNode> Dependencies { get; } = new();

        public List<ResourceBinding> GlobalDependencies { get; } = new();

        public PostFxDependencyBuilder Builder { get; }

        List<INode> INode.Dependencies => Dependencies.Cast<INode>().ToList();

        public void Clear()
        {
            Dependencies.Clear();
            GlobalDependencies.Clear();
            Builder.Clear();
        }

        public static bool operator ==(PostFxNode left, PostFxNode right)
        {
            return left.Name == right.Name;
        }

        public static bool operator !=(PostFxNode left, PostFxNode right)
        {
            return left.Name != right.Name;
        }

        public override bool Equals(object? obj)
        {
            if (obj is PostFxNode node)
            {
                return node.Name == this.Name;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}