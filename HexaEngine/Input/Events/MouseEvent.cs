namespace HexaEngine.Input
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;

    public struct MouseEvent(MouseButtonEventArgs eventArgs)
    {
        public MouseButton Button = eventArgs.Button;
        public MouseButtonState State = eventArgs.State;
        public int Clicks = eventArgs.Clicks;
    }
}