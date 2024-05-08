namespace HexaEngine.Scripts
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class ScriptDependencyBuilder
    {
        private readonly ScriptNode node;

        public ScriptDependencyBuilder(ScriptNode node, ScriptTypeRegistry registry)
        {
            this.node = node;
        }

        public ScriptNode Node => node;

        public List<Type> Dependencies { get; } = [];

        public void Clear()
        {
            Dependencies.Clear();
        }

        private static ScriptNode? ResolveNode(Type scriptType, IReadOnlyList<ScriptNode> others)
        {
            for (int i = 0; i < others.Count; i++)
            {
                var other = others[i];
                if (other.ScriptType == scriptType)
                {
                    return other;
                }
            }
            return null;
        }

        public void Build(IReadOnlyList<ScriptNode> others)
        {
            foreach (var after in node.ScriptType.GetCustomAttributes<ScriptRunAfterAttribute>())
            {
                var runAfter = after.Type;
                var afterNode = ResolveNode(runAfter, others);
                if (afterNode is null)
                {
                    continue;
                }
                if (!node.Dependencies.Contains(afterNode))
                {
                    node.Dependencies.Add(afterNode);
                    afterNode.Dependants.Add(node);
                }
            }

            foreach (var before in node.ScriptType.GetCustomAttributes<ScriptRunBeforeAttribute>())
            {
                var beforeNode = ResolveNode(before.Type, others);
                if (beforeNode is null)
                {
                    continue;
                }
                if (!beforeNode.Dependencies.Contains(node))
                {
                    beforeNode.Dependencies.Add(node);
                    node.Dependants.Add(beforeNode);
                }
            }
        }
    }
}