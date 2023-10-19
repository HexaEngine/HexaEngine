namespace HexaEngine.Core.Input.Events
{
    using HexaEngine.Core.Windows.Events;

    public class TouchEventArgs : RoutedEventArgs
    {
        public TouchEventArgs()
        {
        }

        public TouchEventArgs(int touchDeviceId, long fingerId, FingerState state, float x, float y, float pressure)
        {
            TouchDeviceId = touchDeviceId;
            FingerId = fingerId;
            State = state;
            X = x;
            Y = y;
            Pressure = pressure;
        }

        public long TouchDeviceId { get; internal set; }

        public TouchDevice TouchDevice => TouchDevices.GetById(TouchDeviceId);

        public long FingerId { get; internal set; }

        public Finger Finger => TouchDevice.GetFingerById(FingerId);

        public FingerState State { get; internal set; }

        public float X { get; internal set; }

        public float Y { get; internal set; }

        public float Pressure { get; internal set; }
    }
}