namespace HexaEngine.Materials.Generator
{
    using HexaEngine.Materials;
    using System.Text;

    public interface INodeAnalyzer
    {
        bool CanAnalyze(Node node);

        void Analyze(Node node, GenerationContext context, StringBuilder builder);

        bool TryAnalyze(Node node, GenerationContext context, StringBuilder builder)
        {
            if (CanAnalyze(node))
            {
                Analyze(node, context, builder);
                return true;
            }
            return false;
        }
    }

    public abstract class NodeAnalyzer<T> : INodeAnalyzer
    {
        public void Analyze(Node node, GenerationContext context, StringBuilder builder)
        {
            if (node is T t)
            {
                Analyze(t, context, builder);
            }
        }

        public abstract void Analyze(T node, GenerationContext context, StringBuilder builder);

        public bool CanAnalyze(Node node)
        {
            if (node is T t)
            {
                return CanAnalyze(t);
            }
            return false;
        }

        public virtual bool CanAnalyze(T node)
        {
            return true;
        }
    }
}