namespace HexaEngine.Input.Events
{
    using HexaEngine.Core.Input.Events;

    public struct MouseMovedEvent(MouseMoveEventArgs eventArgs)
    {
        public float X = eventArgs.X;
        public float Y = eventArgs.Y;
        public float RelX = eventArgs.RelX;
        public float RelY = eventArgs.RelY;

        public float GetAxis(int axis)
        {
            if (axis == 0)
            {
                return RelX;
            }
            else if (axis == 1)
            {
                return RelY;
            }
            else
            {
                return 0;
            }
        }
    }
}