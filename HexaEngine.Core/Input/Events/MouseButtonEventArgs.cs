namespace HexaEngine.Core.Input.Events
{
    using HexaEngine.Core.Input;
    using Hexa.NET.Mathematics;

    /// <summary>
    /// Provides data for mouse button input events.
    /// </summary>
    public class MouseButtonEventArgs : MouseEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MouseButtonEventArgs"/> class.
        /// </summary>
        public MouseButtonEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseButtonEventArgs"/> class with the specified parameters.
        /// </summary>
        /// <param name="button">The mouse button associated with the event.</param>
        /// <param name="state">The state of the mouse button (Up or Down).</param>
        /// <param name="clicks">The number of clicks associated with the event.</param>
        /// <param name="position">The mouse position on the window.</param>
        public MouseButtonEventArgs(MouseButton button, MouseButtonState state, int clicks, Point2 position)
        {
            Button = button;
            State = state;
            Clicks = clicks;
            Position = position;
        }

        /// <summary>
        /// Gets the mouse button associated with the event.
        /// </summary>
        public MouseButton Button { get; internal set; }

        /// <summary>
        /// Gets the state of the mouse button (Up or Down).
        /// </summary>
        public MouseButtonState State { get; internal set; }

        /// <summary>
        /// Gets the number of clicks associated with the event.
        /// </summary>
        public int Clicks { get; internal set; }

        /// <summary>
        /// Gets the mouse position on the window.
        /// </summary>
        public Point2 Position { get; internal set; }
    }
}