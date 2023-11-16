namespace HexaEngine.Editor.Attributes
{
    using HexaEngine.Core.Scenes;
    using System;

    /// <summary>
    /// Represents an attribute that provides additional information about a game object for the editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EditorGameObjectAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorGameObjectAttribute"/> class with the specified name, type, constructor, and type check function.
        /// </summary>
        /// <param name="name">The name of the game object.</param>
        /// <param name="type">The type of the game object.</param>
        /// <param name="constructor">A function that constructs an instance of the game object.</param>
        /// <param name="isType">A function that checks if a given game object is of the specified type.</param>
        public EditorGameObjectAttribute(string name, Type type, Func<GameObject> constructor, Func<GameObject, bool> isType)
        {
            Name = name;
            Type = type;
            Constructor = constructor;
            IsType = isType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorGameObjectAttribute"/> class with the specified name, category, type, constructor, and type check function.
        /// </summary>
        /// <param name="name">The name of the game object.</param>
        /// <param name="category">The category of the game object.</param>
        /// <param name="type">The type of the game object.</param>
        /// <param name="constructor">A function that constructs an instance of the game object.</param>
        /// <param name="isType">A function that checks if a given game object is of the specified type.</param>
        public EditorGameObjectAttribute(string name, string category, Type type, Func<GameObject> constructor, Func<GameObject, bool> isType)
        {
            Name = name;
            Category = category;
            Type = type;
            Constructor = constructor;
            IsType = isType;
        }

        /// <summary>
        /// Gets the name of the game object.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the category of the game object.
        /// </summary>
        public string? Category { get; }

        /// <summary>
        /// Gets the type of the game object.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets a function that constructs an instance of the game object.
        /// </summary>
        public Func<GameObject> Constructor { get; }

        /// <summary>
        /// Gets a function that checks if a given game object is of the specified type.
        /// </summary>
        public Func<GameObject, bool> IsType { get; }
    }

    /// <summary>
    /// Represents an attribute that provides additional information about a game object for the editor.
    /// </summary>
    /// <typeparam name="T">The type of the game object.</typeparam>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EditorGameObjectAttribute<T> : EditorGameObjectAttribute where T : GameObject, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorGameObjectAttribute{T}"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name of the game object.</param>
        public EditorGameObjectAttribute(string name) : base(name, typeof(T), () => new T(), (other) => other is T)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorGameObjectAttribute{T}"/> class with the specified name and category.
        /// </summary>
        /// <param name="name">The name of the game object.</param>
        /// <param name="category">The category of the game object.</param>
        public EditorGameObjectAttribute(string name, string category) : base(name, category, typeof(T), () => new T(), (other) => other is T)
        {
        }
    }
}