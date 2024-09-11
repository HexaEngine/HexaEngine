using System.Diagnostics.CodeAnalysis;

namespace HexaEngine.Materials
{
    public abstract class NodeRendererFactory
    {
        public abstract bool CanCreate(Node node);

        public abstract INodeRenderer Create(Node node);

        public bool TryCreate(Node node, [NotNullWhen(true)] out INodeRenderer? renderer)
        {
            if (CanCreate(node))
            {
                renderer = Create(node);
                return true;
            }
            renderer = null;
            return false;
        }
    }
}