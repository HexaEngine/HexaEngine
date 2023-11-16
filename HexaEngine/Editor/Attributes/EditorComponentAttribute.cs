namespace HexaEngine.Editor.Attributes
{
    using HexaEngine.Scenes;
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents an attribute used to mark classes as editor components.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EditorComponentAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorComponentAttribute"/> class with the specified type and name.
        /// </summary>
        /// <param name="type">The type of the editor component.</param>
        /// <param name="name">The name of the editor component.</param>
        public EditorComponentAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type, string name)
        {
            Type = type;
            Constructor = () => (IComponent?)Activator.CreateInstance(type) ?? throw new();
            Name = name;
            IsType = x => type.IsAssignableFrom(x.GetType());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorComponentAttribute"/> class with the specified type, name, and hidden status.
        /// </summary>
        /// <param name="type">The type of the editor component.</param>
        /// <param name="name">The name of the editor component.</param>
        /// <param name="isHidden">Indicates whether the editor component should be hidden.</param>
        public EditorComponentAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type, string name, bool isHidden) : this(type, name)
        {
            IsHidden = isHidden;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorComponentAttribute"/> class with the specified type, name, hidden status, and internal status.
        /// </summary>
        /// <param name="type">The type of the editor component.</param>
        /// <param name="name">The name of the editor component.</param>
        /// <param name="isHidden">Indicates whether the editor component should be hidden.</param>
        /// <param name="isInternal">Indicates whether the editor component is internal.</param>
        public EditorComponentAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type, string name, bool isHidden, bool isInternal) : this(type, name)
        {
            IsHidden = isHidden;
            IsInternal = isInternal;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorComponentAttribute"/> class with the specified type, name, and allowed types.
        /// </summary>
        /// <param name="type">The type of the editor component.</param>
        /// <param name="name">The name of the editor component.</param>
        /// <param name="allowedTypes">An array of allowed types for the editor component.</param>
        public EditorComponentAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type, string name, params Type[] allowedTypes) : this(type, name)
        {
            AllowedTypes = allowedTypes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorComponentAttribute"/> class with the specified type, name, and optional settings.
        /// </summary>
        /// <param name="type">The type of the editor component.</param>
        /// <param name="name">The name of the editor component.</param>
        /// <param name="allowedTypes">An array of allowed types for the editor component.</param>
        /// <param name="disallowedTypes">An array of disallowed types for the editor component.</param>
        public EditorComponentAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type, string name, Type[] allowedTypes, Type[] disallowedTypes) : this(type, name)
        {
            AllowedTypes = allowedTypes;
            DisallowedTypes = disallowedTypes;
        }

        /// <summary>
        /// Gets the constructor delegate for creating an instance of the editor component.
        /// </summary>
        public Func<IComponent> Constructor { get; }

        /// <summary>
        /// Gets the predicate for determining if an object is of the specified editor component type.
        /// </summary>
        public Func<IComponent, bool> IsType { get; }

        /// <summary>
        /// Gets the type of the editor component.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the name of the editor component.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether the editor component is hidden.
        /// </summary>
        public bool IsHidden { get; }

        /// <summary>
        /// Gets a value indicating whether the editor component is internal.
        /// </summary>
        public bool IsInternal { get; }

        /// <summary>
        /// Gets an array of allowed types for the editor component.
        /// </summary>
        public Type[]? AllowedTypes { get; }

        /// <summary>
        /// Gets an array of disallowed types for the editor component.
        /// </summary>
        public Type[]? DisallowedTypes { get; }
    }

    /// <summary>
    /// Represents a generic version of the <see cref="EditorComponentAttribute"/> class.
    /// </summary>
    /// <typeparam name="T">The generic type of the editor component.</typeparam>
    public class EditorComponentAttribute<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T> : EditorComponentAttribute where T : class, IComponent, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorComponentAttribute{T}"/> class with the specified type and name.
        /// </summary>
        /// <param name="name">The name of the editor component.</param>
        public EditorComponentAttribute(string name) : base(typeof(T), name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorComponentAttribute{T}"/> class with the specified type, name, and hidden status.
        /// </summary>
        /// <param name="name">The name of the editor component.</param>
        /// <param name="isHidden">Indicates whether the editor component should be hidden.</param>
        public EditorComponentAttribute(string name, bool isHidden) : base(typeof(T), name, isHidden)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorComponentAttribute{T}"/> class with the specified type, name, and allowed types.
        /// </summary>
        /// <param name="name">The name of the editor component.</param>
        /// <param name="allowedTypes">An array of allowed types for the editor component.</param>
        public EditorComponentAttribute(string name, params Type[] allowedTypes) : base(typeof(T), name, allowedTypes)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorComponentAttribute{T}"/> class with the specified type, name, hidden status, and internal status.
        /// </summary>
        /// <param name="name">The name of the editor component.</param>
        /// <param name="isHidden">Indicates whether the editor component should be hidden.</param>
        /// <param name="isInternal">Indicates whether the editor component is internal.</param>
        public EditorComponentAttribute(string name, bool isHidden, bool isInternal) : base(typeof(T), name, isHidden, isInternal)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorComponentAttribute{T}"/> class with the specified type, name, allowed types, and disallowed types.
        /// </summary>
        /// <param name="name">The name of the editor component.</param>
        /// <param name="allowedTypes">An array of allowed types for the editor component.</param>
        /// <param name="disallowedTypes">An array of disallowed types for the editor component.</param>
        public EditorComponentAttribute(string name, Type[] allowedTypes, Type[] disallowedTypes) : base(typeof(T), name, allowedTypes, disallowedTypes)
        {
        }
    }
}