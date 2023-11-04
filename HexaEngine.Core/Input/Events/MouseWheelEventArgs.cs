namespace HexaEngine.Core.Input.Events
{
    using System.Numerics;

    /// <summary>
    /// Provides data for mouse wheel events.
    /// </summary>
    public class MouseWheelEventArgs : MouseEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MouseWheelEventArgs"/> class.
        /// </summary>
        public MouseWheelEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseWheelEventArgs"/> class with specific wheel data.
        /// </summary>
        /// <param name="wheel">The amount of wheel movement in both the X and Y directions.</param>
        public MouseWheelEventArgs(Vector2 wheel)
        {
            Wheel = wheel;
        }

        /// <summary>
        /// Gets the amount of wheel movement in both the X and Y directions.
        /// </summary>
        public Vector2 Wheel { get; internal set; }

        /// <summary>
        /// Gets the direction of the mouse wheel movement.
        /// </summary>
        public MouseWheelDirection Direction { get; internal set; }
    }
}