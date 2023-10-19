namespace HexaEngine.Input
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;

    public struct JoystickHatMotionEvent(JoystickHatMotionEventArgs eventArgs)
    {
        public int Hat = eventArgs.Hat;
        public JoystickHatState State = eventArgs.State;
    }
}