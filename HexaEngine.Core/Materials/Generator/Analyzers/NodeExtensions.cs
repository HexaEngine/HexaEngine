namespace HexaEngine.Materials.Generator.Analyzers
{
    using HexaEngine.Materials.Nodes;

    public static class NodeExtensions
    {
        public static bool IsUnknown(this ITypedNode node)
        {
            return node.Type.IsUnknown;
        }

        public static bool IsUnknown(this ITypedPin pin)
        {
            return pin.Type.IsUnknown;
        }
    }
}