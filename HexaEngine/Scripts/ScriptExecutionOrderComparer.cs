namespace HexaEngine.Scripts
{
    public struct ScriptExecutionOrderComparer : IComparer<ScriptComponent>
    {
        public readonly int Compare(ScriptComponent? x, ScriptComponent? y)
        {
            return x == null || y == null ? 0 : x.ExecutionOrderIndex.CompareTo(y.ExecutionOrderIndex);
        }
    }
}