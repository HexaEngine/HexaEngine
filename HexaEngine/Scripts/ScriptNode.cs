namespace HexaEngine.Scripts
{
    using HexaEngine.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class ScriptNode : INode
    {
        private readonly List<ScriptNode> dependencies = [];
        private readonly List<ScriptNode> dependants = [];
        private readonly ScriptDependencyBuilder builder;
        private readonly Type scriptType;

        public ScriptNode(Type scriptType, ScriptTypeRegistry registry)
        {
            builder = new(this, registry);
            this.scriptType = scriptType;
            RunInParallel = scriptType.GetCustomAttribute<ScriptRunInParallelAttribute>() != null;
        }

        public Type ScriptType => scriptType;

        List<INode> INode.Dependencies => dependencies.Cast<INode>().ToList();

        public List<ScriptNode> Dependencies => dependencies;

        public List<ScriptNode> Dependants => dependants;

        public int Depth { get; internal set; }

        public bool RunInParallel { get; }

        public ScriptDependencyBuilder Builder => builder;

        public void Reset()
        {
            dependencies.Clear();
            dependants.Clear();
            builder.Clear();
        }

        public static bool operator ==(ScriptNode left, ScriptNode right)
        {
            return left.ScriptType == right.ScriptType;
        }

        public static bool operator !=(ScriptNode left, ScriptNode right)
        {
            return left.ScriptType != right.ScriptType;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ScriptNode node)
            {
                return node.ScriptType == ScriptType;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ScriptType.GetHashCode();
        }

        public override string ToString()
        {
            return ScriptType.Name;
        }
    }
}