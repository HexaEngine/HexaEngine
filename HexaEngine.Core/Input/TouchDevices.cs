namespace HexaEngine.Core.Input
{
    using HexaEngine.Core.Input.Events;
    using Silk.NET.SDL;

    /// <summary>
    /// Provides functionality to manage touch devices.
    /// </summary>
    public static class TouchDevices
    {
        private static readonly Sdl sdl = Sdl.GetApi();

        private static readonly List<TouchDevice> touchDevices = new();
        private static readonly Dictionary<long, TouchDevice> idToTouch = new();

        private static readonly TouchDeviceEventArgs touchDeviceEventArgs = new();

        /// <summary>
        /// Gets the list of available touch devices.
        /// </summary>
        public static IReadOnlyList<TouchDevice> Devices => touchDevices;

        /// <summary>
        /// Occurs when a touch device is added.
        /// </summary>
        public static event TouchDeviceEventHandler<TouchDeviceEventArgs>? TouchDeviceAdded;

        /// <summary>
        /// Occurs when a touch device is removed.
        /// </summary>
        public static event TouchDeviceEventHandler<TouchDeviceEventArgs>? TouchDeviceRemoved;

        /// <summary>
        /// Occurs when a touch event is detected (e.g., touch-up, touch-down, touch-motion).
        /// </summary>
        public static event TouchDeviceEventHandler<TouchEventArgs>? TouchUp;

        /// <summary>
        /// Occurs when a touch-down event is detected.
        /// </summary>
        public static event TouchDeviceEventHandler<TouchEventArgs>? TouchDown;

        /// <summary>
        /// Occurs when a touch-motion event is detected.
        /// </summary>
        public static event TouchDeviceEventHandler<TouchMoveEventArgs>? TouchMotion;

        /// <summary>
        /// Initializes the touch device management system.
        /// </summary>
        internal static void Init()
        {
            var touchDeviceCount = sdl.GetNumTouchDevices();

            for (int i = 0; i < touchDeviceCount; i++)
            {
                AddTouchDevice(i);
            }
        }

        /// <summary>
        /// Retrieves a touch device by its unique identifier.
        /// </summary>
        /// <param name="touchDeviceId">The unique identifier of the touch device.</param>
        /// <returns>The touch device associated with the specified identifier.</returns>
        public static TouchDevice GetById(long touchDeviceId)
        {
            return idToTouch[touchDeviceId];
        }

        internal static TouchDevice AddTouchDevice(int index)
        {
            TouchDevice dev = new(index);
            touchDevices.Add(dev);
            idToTouch.Add(dev.Id, dev);
            touchDeviceEventArgs.TouchDeviceId = dev.Id;
            TouchDeviceAdded?.Invoke(dev, touchDeviceEventArgs);
            return dev;
        }

        internal static TouchDevice AddTouchDevice(long touchId)
        {
            TouchDevice dev = new(touchId);
            touchDevices.Add(dev);
            idToTouch.Add(touchId, dev);
            touchDeviceEventArgs.TouchDeviceId = dev.Id;
            TouchDeviceAdded?.Invoke(dev, touchDeviceEventArgs);
            return dev;
        }

        internal static bool RemoveTouchDevice(long touchId)
        {
            if (idToTouch.TryGetValue(touchId, out var dev))
            {
                idToTouch.Remove(touchId);
                touchDevices.Remove(dev);
                touchDeviceEventArgs.TouchDeviceId = dev.Id;
                TouchDeviceRemoved?.Invoke(dev, touchDeviceEventArgs);
                return true;
            }
            return false;
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