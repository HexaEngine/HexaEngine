namespace HexaEngine.Materials.Generator
{
    using HexaEngine.Materials.Generator.Analyzers;
    using System.Collections.Generic;

    public static class NodeAnalyzerRegistry
    {
        private static readonly List<INodeAnalyzer> analyzers = [];

        static NodeAnalyzerRegistry()
        {
            AddAnalyzer<InputNodeAnalyzer>();
            AddAnalyzer<SwizzleVectorNodeAnalyzer>();
            AddAnalyzer<ConstantNodeAnalyzer>();
            AddAnalyzer<PropertyNodeAnalyzer>();
            AddAnalyzer<FlipUVNodeAnalyzer>();
            AddAnalyzer<FuncCallDeclarationNodeAnalyzer>();
            AddAnalyzer<FuncCallNodeAnalyzer>();
            AddAnalyzer<FuncCallVoidNodeAnalyzer>();
            AddAnalyzer<FuncOperatorNodeAnalyzer>();
            AddAnalyzer<PackNodeAnalyzer>();
            AddAnalyzer<SplitNodeAnalyzer>();
            AddAnalyzer<TextureNodeAnalyzer>();
        }

        public static IReadOnlyList<INodeAnalyzer> Analyzers => analyzers;

        public static void AddAnalyzer(INodeAnalyzer analyzer)
        {
            analyzers.Add(analyzer);
        }

        public static void AddAnalyzer<T>() where T : INodeAnalyzer, new()
        {
            AddAnalyzer(new T());
        }

        public static void RemoveAnalyzer(INodeAnalyzer analyzer)
        {
            analyzers.Remove(analyzer);
        }
    }
}