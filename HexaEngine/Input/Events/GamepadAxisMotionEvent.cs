namespace HexaEngine.Input.Events
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;

    public struct GamepadAxisMotionEvent(GamepadAxisMotionEventArgs eventArgs)
    {
        public GamepadAxis Axis = eventArgs.Axis;
        public short Value = eventArgs.Value;
    }
}