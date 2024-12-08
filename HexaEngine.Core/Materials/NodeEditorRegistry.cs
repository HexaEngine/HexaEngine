namespace HexaEngine.Materials
{
    public static class NodeEditorRegistry
    {
        private static INodeRenderer defaultNodeRenderer = null!;
        private static IPinRenderer defaultPinRenderer = null!;

        private static readonly List<NodeRendererFactory> nodeRendererFactories = [];
        private static readonly List<PinRendererFactory> pinRendererFactories = [];

        public static void SetDefaultRenderers(INodeRenderer nodeRenderer, IPinRenderer pinRenderer)
        {
            defaultNodeRenderer = nodeRenderer;
            defaultPinRenderer = pinRenderer;
        }

        public static void RegisterNodeSingleton<TNode, TRenderer>() where TNode : Node where TRenderer : INodeRenderer, new()
        {
            nodeRendererFactories.Add(new SingletonNodeRendererFactory<TNode, TRenderer>());
        }

        public static void RegisterNodeInstanced<TNode, TRenderer>() where TNode : Node where TRenderer : INodeRendererInstance<TNode>, new()
        {
            nodeRendererFactories.Add(new InstancedNodeRendererFactory<TNode, TRenderer>());
        }

        public static void RegisterPinSingleton<TPin, TRenderer>() where TPin : Pin where TRenderer : IPinRenderer, new()
        {
            pinRendererFactories.Add(new SingletonPinRendererFactory<TPin, TRenderer>());
        }

        public static void RegisterPinInstanced<TPin, TRenderer>() where TPin : Pin where TRenderer : IPinRendererInstance<TPin>, new()
        {
            pinRendererFactories.Add(new InstancedPinRendererFactory<TPin, TRenderer>());
        }

        public static INodeRenderer GetNodeRenderer(Node node)
        {
            for (int i = 0; i < nodeRendererFactories.Count; i++)
            {
                if (nodeRendererFactories[i].TryCreate(node, out var renderer))
                {
                    return renderer;
                }
            }

            return defaultNodeRenderer;
        }

        public static IPinRenderer GetPinRenderer(Pin pin)
        {
            for (int i = 0; i < pinRendererFactories.Count; i++)
            {
                if (pinRendererFactories[i].TryCreate(pin, out var renderer))
                {
                    return renderer;
                }
            }

            return defaultPinRenderer;
        }
    }
}