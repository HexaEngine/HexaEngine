namespace HexaEngine.Scripts
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class ScriptTypeRegistry
    {
        private readonly Dictionary<Type, ScriptNode> typeToNode = [];
        private readonly Dictionary<ScriptNode, Type> nodeToType = [];
        private readonly object _lock = new();

        public void Clear()
        {
            lock (_lock)
            {
                typeToNode.Clear();
                nodeToType.Clear();
            }
        }

        public bool TryGetNodeBy(Type type, [MaybeNullWhen(false)] out ScriptNode? node)
        {
            lock (_lock)
            {
                return typeToNode.TryGetValue(type, out node);
            }
        }

        public bool TryGetTypeBy(ScriptNode node, [MaybeNullWhen(false)] out Type? type)
        {
            lock (_lock)
            {
                return nodeToType.TryGetValue(node, out type);
            }
        }

        public ScriptNode? GetNodeBy(Type type)
        {
            lock (_lock)
            {
                typeToNode.TryGetValue(type, out var node);
                return node;
            }
        }

        public Type? GetTypeBy(ScriptNode node)
        {
            lock (_lock)
            {
                nodeToType.TryGetValue(node, out var type);
                return type;
            }
        }

        public void Add(Type? script, ScriptNode node)
        {
            if (script == null)
            {
                return;
            }
            lock (_lock)
            {
                typeToNode.Add(script, node);
                nodeToType.Add(node, script);
            }
        }

        public void Add(ScriptComponent script, ScriptNode node)
        {
            Add(script.ScriptType, node);
        }

        public bool Remove(Type? type)
        {
            if (type == null)
                return false;

            lock (_lock)
            {
                if (typeToNode.TryGetValue(type, out var node))
                {
                    typeToNode.Remove(type);
                    nodeToType.Remove(node);
                    return true;
                }
            }

            return false;
        }

        public bool Remove(ScriptComponent script)
        {
            return Remove(script.ScriptType);
        }

        public bool Remove(ScriptNode node)
        {
            lock (_lock)
            {
                if (nodeToType.TryGetValue(node, out var type))
                {
                    typeToNode.Remove(type);
                    nodeToType.Remove(node);
                    return true;
                }
            }

            return false;
        }
    }
}