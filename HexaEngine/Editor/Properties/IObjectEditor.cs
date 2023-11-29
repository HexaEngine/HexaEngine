namespace HexaEngine.Editor.Properties
{
    using HexaEngine.Core.Graphics;
    using System;

    /// <summary>
    /// Represents an interface for managing object editing in a graphical context.
    /// </summary>
    public interface IObjectEditor : IDisposable
    {
        /// <summary>
        /// Gets the name associated with the object editor.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the type of the object being edited.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets or sets the instance of the object being edited.
        /// </summary>
        object? Instance { get; set; }

        /// <summary>
        /// Gets a value indicating whether the object editor is empty.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Gets a value indicating whether the object editor is hidden.
        /// </summary>
        bool IsHidden { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to skip the table setup when drawing the object editor.
        /// </summary>
        bool NoTable { get; set; }

        /// <summary>
        /// Draws the object editor within the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context used for drawing.</param>
        void Draw(IGraphicsContext context);
    }
}