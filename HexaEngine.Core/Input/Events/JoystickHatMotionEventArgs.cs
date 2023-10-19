namespace HexaEngine.Core.Input.Events
{
    public class JoystickHatMotionEventArgs : JoystickEventArgs
    {
        public JoystickHatMotionEventArgs()
        {
        }

        public JoystickHatMotionEventArgs(int hat, JoystickHatState state)
        {
            Hat = hat;
            State = state;
        }

        public int Hat { get; internal set; }

        public JoystickHatState State { get; internal set; }
    }
}