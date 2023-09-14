namespace D3D12Testing.Input.Events
{
    using D3D12Testing.Input;
    using System.Numerics;

    public class MouseWheelEventArgs : EventArgs
    {
        public MouseWheelEventArgs()
        {
        }

        public MouseWheelEventArgs(Vector2 wheel)
        {
            Wheel = wheel;
        }

        public Vector2 Wheel { get; internal set; }

        public MouseWheelDirection Direction { get; internal set; }
    }
}