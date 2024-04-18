namespace HexaEngine.Scripts
{
    using HexaEngine.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class ScriptNode : INode
    {
        private readonly List<ScriptNode> dependencies = [];
        private readonly List<ScriptNode> dependants = [];
        private readonly ScriptDependencyBuilder builder;
        private readonly ScriptComponent? script;
        private readonly ScriptFlags flags;
        private readonly Type scriptType;

        public ScriptNode(ScriptComponent script, ScriptTypeRegistry registry)
        {
            this.script = script;
            flags = script.Flags;

            builder = new(this, registry);
        }

        public ScriptNode(Type scriptType, ScriptFlags flags, ScriptTypeRegistry registry)
        {
            builder = new(this, registry);
            this.scriptType = scriptType;
            this.flags = flags;
        }

        public ScriptComponent? Script => script;

        public Type ScriptType => script?.ScriptType ?? scriptType;

        public ScriptFlags Flags => flags;

        List<INode> INode.Dependencies => dependencies.Cast<INode>().ToList();

        public List<ScriptNode> Dependencies => dependencies;

        public List<ScriptNode> Dependants => dependants;

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