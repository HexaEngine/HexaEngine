namespace HexaEngine.Editor.Attributes
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a delegate that defines a condition for an editor property without a strongly typed instance.
    /// </summary>
    /// <param name="instance">The instance of the object the condition is evaluated against.</param>
    /// <returns><c>true</c> if the condition is met; otherwise, <c>false</c>.</returns>
    public delegate bool EditorPropertyCondition(object instance);

    /// <summary>
    /// Represents a delegate that defines a condition for an editor property with a strongly typed instance.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <param name="instance">The strongly typed instance of the object the condition is evaluated against.</param>
    /// <returns><c>true</c> if the condition is met; otherwise, <c>false</c>.</returns>
    public delegate bool EditorPropertyCondition<T>(T instance);

    /// <summary>
    /// Specifies a condition for the visibility, enabling, or read-only state of an editor property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class EditorPropertyConditionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorPropertyConditionAttribute"/> class with the specified method name and mode.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="mode">The editor property condition mode.</param>
        public EditorPropertyConditionAttribute(string methodName, EditorPropertyConditionMode mode = EditorPropertyConditionMode.Visible)
        {
            MethodName = methodName;
            Mode = mode;
        }

        /// <summary>
        /// Gets the condition function.
        /// </summary>
        public EditorPropertyCondition Condition { get; set; }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Gets the mode specifying how the condition affects the property.
        /// </summary>
        public EditorPropertyConditionMode Mode { get; }
    }

    /// <summary>
    /// Specifies a condition for the visibility, enabling, or read-only state of an editor property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class EditorPropertyConditionAttribute<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)] T> : EditorPropertyConditionAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorPropertyConditionAttribute{T}"/> class with the specified method name and mode.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="mode">The editor property condition mode.</param>
        public EditorPropertyConditionAttribute(string methodName, EditorPropertyConditionMode mode = EditorPropertyConditionMode.Visible) : base(methodName, mode)
        {
            var method = typeof(T).GetMethod(methodName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);
            if (method == null)
            {
                throw new InvalidOperationException($"Method ({methodName}) was not found, make sure that the method is public and static.");
            }
            GenericCondition = method.CreateDelegate<EditorPropertyCondition<T>>();
            Condition = ConditionMethod;
        }

        /// <summary>
        /// Gets or sets the generic condition function.
        /// </summary>
        public EditorPropertyCondition<T> GenericCondition { get; set; }

        private bool ConditionMethod(object instance)
        {
            return GenericCondition((T)instance);
        }
    }
}