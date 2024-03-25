namespace HexaEngine.Editor
{
    using System.Collections.Generic;
    using System.Reflection;

    public interface IProxy
    {
        public string TypeName { get; }

        public Type? TargetType { get; }

        public Dictionary<string, object?> Data { get; }

        IReadOnlyList<PropertyInfo> Properties { get; }

        void Apply(object target);

        void UpdateType(object target);
    }
}