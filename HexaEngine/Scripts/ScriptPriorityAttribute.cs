namespace HexaEngine.Scripts
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ScriptPriorityAttribute : Attribute
    {
        public ScriptPriorityAttribute(int index)
        {
            Index = index;
        }

        public int Index { get; }
    }
}