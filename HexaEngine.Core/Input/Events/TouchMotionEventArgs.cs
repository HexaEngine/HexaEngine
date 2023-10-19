namespace HexaEngine.Core.Input.Events
{
    using HexaEngine.Core.Windows.Events;

    public class TouchMotionEventArgs : RoutedEventArgs
    {
        public TouchMotionEventArgs()
        {
        }

        public TouchMotionEventArgs(long touchDeviceId, long fingerId, float x, float y, float pressure)
        {
            TouchDeviceId = touchDeviceId;
            FingerId = fingerId;
            X = x;
            Y = y;
            Pressure = pressure;
        }

        public long TouchDeviceId { get; internal set; }

        public TouchDevice TouchDevice => TouchDevices.GetById(TouchDeviceId);

        public long FingerId { get; internal set; }

        public Finger Finger => TouchDevice.GetFingerById(FingerId);

        public float X { get; internal set; }

        public float Y { get; internal set; }

        public float Dx { get; internal set; }

        public float Dy { get; internal set; }

        public float Pressure { get; internal set; }
    }
}