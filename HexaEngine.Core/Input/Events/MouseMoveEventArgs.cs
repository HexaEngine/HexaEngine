namespace HexaEngine.Core.Input.Events
{
    /// <summary>
    /// Provides data for mouse motion events.
    /// </summary>
    public class MouseMoveEventArgs : MouseEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MouseMoveEventArgs"/> class.
        /// </summary>
        public MouseMoveEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseMoveEventArgs"/> class with specific values.
        /// </summary>
        /// <param name="x">The X coordinate of the mouse cursor.</param>
        /// <param name="y">The Y coordinate of the mouse cursor.</param>
        /// <param name="relX">The relative X motion of the mouse cursor.</param>
        /// <param name="relY">The relative Y motion of the mouse cursor.</param>
        public MouseMoveEventArgs(float x, float y, float relX, float relY)
        {
            X = x;
            Y = y;
            RelX = relX;
            RelY = relY;
        }

        /// <summary>
        /// Gets the X coordinate of the mouse cursor.
        /// </summary>
        public float X { get; internal set; }

        /// <summary>
        /// Gets the Y coordinate of the mouse cursor.
        /// </summary>
        public float Y { get; internal set; }

        /// <summary>
        /// Gets the relative X motion of the mouse cursor.
        /// </summary>
        public float RelX { get; internal set; }

        /// <summary>
        /// Gets the relative Y motion of the mouse cursor.
        /// </summary>
        public float RelY { get; internal set; }
    }
}