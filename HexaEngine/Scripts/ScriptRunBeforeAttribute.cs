namespace HexaEngine.Scripts
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class ScriptRunBeforeAttribute : Attribute
    {
        public ScriptRunBeforeAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ScriptRunBeforeAttribute<T> : ScriptRunBeforeAttribute where T : ScriptBehaviour
    {
        public ScriptRunBeforeAttribute() : base(typeof(T))
        {
        }
    }
}