namespace HexaEngine.Core.Input.Events
{
    /// <summary>
    /// Provides data for a joystick ball motion event.
    /// </summary>
    public class JoystickBallMotionEventArgs : JoystickEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JoystickBallMotionEventArgs"/> class.
        /// </summary>
        public JoystickBallMotionEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JoystickBallMotionEventArgs"/> class with ball motion data.
        /// </summary>
        /// <param name="ball">The ID of the joystick ball that triggered the motion event.</param>
        /// <param name="relX">The relative motion in the X-axis of the joystick ball.</param>
        /// <param name="relY">The relative motion in the Y-axis of the joystick ball.</param>
        public JoystickBallMotionEventArgs(int ball, int relX, int relY)
        {
            Ball = ball;
            RelX = relX;
            RelY = relY;
        }

        /// <summary>
        /// Gets the ID of the joystick ball that triggered the motion event.
        /// </summary>
        public int Ball { get; internal set; }

        /// <summary>
        /// Gets the relative motion in the X-axis of the joystick ball.
        /// </summary>
        public int RelX { get; internal set; }

        /// <summary>
        /// Gets the relative motion in the Y-axis of the joystick ball.
        /// </summary>
        public int RelY { get; internal set; }
    }
}