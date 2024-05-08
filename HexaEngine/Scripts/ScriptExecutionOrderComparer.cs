namespace HexaEngine.Scripts
{
    using HexaEngine.Scenes;

    public struct ScriptExecutionOrderComparer : IComparer<IScriptComponent>
    {
        public readonly int Compare(IScriptComponent? x, IScriptComponent? y)
        {
            return x == null || y == null ? 0 : x.ExecutionOrderIndex.CompareTo(y.ExecutionOrderIndex);
        }
    }
}