namespace HexaEngine.Core.Input.Events
{
    public class JoystickAxisMotionEventArgs : JoystickEventArgs
    {
        public JoystickAxisMotionEventArgs()
        {
        }

        public JoystickAxisMotionEventArgs(int axis, short value)
        {
            Axis = axis;
            Value = value;
        }

        public int Axis { get; internal set; }

        public short Value { get; internal set; }
    }
}