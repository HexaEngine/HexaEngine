namespace HexaEngine.Editor.Attributes
{
    using System;
    using System.Linq;

    /// <summary>
    /// Represents an attribute that provides additional information about an enum property for the editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EditorEnumAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorEnumAttribute"/> class with the specified enum type and values.
        /// </summary>
        /// <param name="type">The type of the enum.</param>
        /// <param name="values">An array of enum values.</param>
        public EditorEnumAttribute(Type type, object[] values)
        {
            Type = type;
            Values = values;
            ValueNames = values.Select(x => x?.ToString() ?? throw new()).ToArray();
        }

        /// <summary>
        /// Gets the type of the enum.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets an array of enum values.
        /// </summary>
        public object[] Values { get; }

        /// <summary>
        /// Gets an array of names corresponding to the enum values.
        /// </summary>
        public string[] ValueNames { get; }
    }

    /// <summary>
    /// Represents an attribute that provides additional information about an enum property for the editor.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    public class EditorEnumAttribute<T> : EditorEnumAttribute where T : struct, Enum
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorEnumAttribute{T}"/> class.
        /// </summary>
        public EditorEnumAttribute() : base(typeof(T), Enum.GetValues<T>().Select(x => (object)x).ToArray())
        {
        }
    }
}