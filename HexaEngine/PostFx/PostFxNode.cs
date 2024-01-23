namespace HexaEngine.PostFx
{
    using HexaEngine.Collections;
    using HexaEngine.Graphics.Graph;
    using System.Collections.Generic;

    public class PostFxNode : INode
    {
        public PostFxNode(IPostFx postFx, PostFxNameRegistry nameRegistry)
        {
            Name = postFx.Name;
            PostFx = postFx;
            Builder = new(this, nameRegistry);
        }

        public string Name { get; }

        public IPostFx PostFx { get; set; }

        public bool Enabled => PostFx.Enabled;

        public List<PostFxNode> Dependencies { get; } = [];

        public List<ResourceBinding> GlobalDependencies { get; } = [];

        public List<PostFxNode> Dependents { get; } = [];

        public PostFxDependencyBuilder Builder { get; }

        List<INode> INode.Dependencies => Dependencies.Cast<INode>().ToList();

        public void Clear()
        {
            Dependencies.Clear();
            GlobalDependencies.Clear();
            Dependents.Clear();
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
                return node.Name == Name;
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