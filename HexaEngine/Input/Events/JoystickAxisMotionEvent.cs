namespace HexaEngine.Input.Events
{
    using HexaEngine.Core.Input.Events;

    public struct JoystickAxisMotionEvent(JoystickAxisMotionEventArgs eventArgs)
    {
        public int Axis = eventArgs.Axis;
        public short Value = eventArgs.Value;
    }
}