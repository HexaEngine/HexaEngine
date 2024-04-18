namespace HexaEngine.Scripts
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ScriptRunAfterAttribute : Attribute
    {
        public ScriptRunAfterAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ScriptRunAfterAttribute<T> : ScriptRunAfterAttribute where T : ScriptBehaviour
    {
        public ScriptRunAfterAttribute() : base(typeof(T))
        {
        }
    }
}