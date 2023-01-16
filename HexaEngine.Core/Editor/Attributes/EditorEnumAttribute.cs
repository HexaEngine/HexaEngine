namespace HexaEngine.Core.Editor.Attributes
{
    using System;
    using System.Linq;

    [AttributeUsage(AttributeTargets.Property)]
    public class EditorEnumAttribute : Attribute
    {
        public EditorEnumAttribute(Type type, object[] values)
        {
            Type = type;
            Values = values;
            ValueNames = values.Select(x => x?.ToString() ?? throw new()).ToArray();
        }

        public Type Type { get; }

        public object[] Values { get; }

        public string[] ValueNames { get; }
    }

    public class EditorEnumAttribute<T> : EditorEnumAttribute where T : struct, Enum
    {
        public EditorEnumAttribute() : base(typeof(T), Enum.GetValues<T>().Select(x => (object)x).ToArray())
        {
        }
    }
}