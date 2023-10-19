namespace HexaEngine.Core.Input
{
    using HexaEngine.Core.Input.Events;
    using Silk.NET.SDL;

    public static class TouchDevices
    {
        private static readonly Sdl sdl = Sdl.GetApi();

        private static readonly List<TouchDevice> touchDevices = new();
        private static readonly Dictionary<long, TouchDevice> idToTouch = new();

        public static IReadOnlyList<TouchDevice> Devices => touchDevices;

        public static event EventHandler<TouchEventArgs>? TouchUp;

        public static event EventHandler<TouchEventArgs>? TouchDown;

        public static event EventHandler<TouchMotionEventArgs>? TouchMotion;

        internal static void Init()
        {
            var touchDeviceCount = sdl.GetNumTouchDevices();

            for (int i = 0; i < touchDeviceCount; i++)
            {
                AddTouchDevice(i);
            }
        }

        public static TouchDevice GetById(long touchDeviceId)
        {
            return idToTouch[touchDeviceId];
        }

        private static TouchDevice AddTouchDevice(int index)
        {
            TouchDevice dev = new(index);
            touchDevices.Add(dev);
            idToTouch.Add(dev.Id, dev);
            return dev;
        }

        private static TouchDevice AddTouchDevice(long touchId)
        {
            TouchDevice dev = new(touchId);
            touchDevices.Add(dev);
            idToTouch.Add(touchId, dev);
            return dev;
        }

        internal static TouchDevice AddOrGetTouch(long id)
        {
            if (idToTouch.TryGetValue(id, out TouchDevice? dev))
            {
                return dev;
            }
            return AddTouchDevice(id);
        }

        internal static void FingerUp(TouchFingerEvent evnt)
        {
            var result = AddOrGetTouch(evnt.TouchId).OnFingerUp(evnt);
            TouchUp?.Invoke(result.Item1, result.Item2);
        }

        internal static void FingerDown(TouchFingerEvent evnt)
        {
            var result = AddOrGetTouch(evnt.TouchId).OnFingerDown(evnt);
            TouchDown?.Invoke(result.Item1, result.Item2);
        }

        internal static void FingerMotion(TouchFingerEvent evnt)
        {
            var result = AddOrGetTouch(evnt.TouchId).OnFingerMotion(evnt);
            TouchMotion?.Invoke(result.Item1, result.Item2);
        }
    }
}