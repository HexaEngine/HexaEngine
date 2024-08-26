namespace HexaEngine.Core.Input
{
    using Hexa.NET.SDL2;
    using HexaEngine.Core.Input.Events;

    /// <summary>
    /// Represents a generic delegate for handling events in the TouchDevice class.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of event arguments for the event.</typeparam>
    /// <param name="sender">The source of the event, which is the TouchDevice that raised the event.</param>
    /// <param name="e">The event arguments specific to the type of event being handled.</param>
    public delegate void TouchDeviceEventHandler<TEventArgs>(TouchDevice sender, TEventArgs e);

    /// <summary>
    /// Represents a touch input device.
    /// </summary>
    public unsafe class TouchDevice
    {
        private readonly long id;
        private readonly string name;
        private readonly TouchDeviceType type;
        private readonly Finger[] fingers;
        private readonly Dictionary<long, int> fingerIdToIndex = new();

        private readonly TouchEventArgs touchEventArgs = new();
        private readonly TouchMoveEventArgs touchMotionEventArgs = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchDevice"/> class using the specified index.
        /// </summary>
        /// <param name="index">The index of the touch device.</param>
        public TouchDevice(int index)
        {
            id = SDL.GetTouchDevice(index);
            name = SDL.GetTouchNameS(index);
            type = (TouchDeviceType)SDL.GetTouchDeviceType(id);

            var fingerCount = SDL.GetNumTouchFingers(id);
            fingers = new Finger[fingerCount];
            for (int i = 0; i < fingerCount; i++)
            {
                var finger = SDL.GetTouchFinger(id, i);
                fingers[i] = new(finger);
                fingerIdToIndex.Add(finger->Id, i);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchDevice"/> class using the specified device ID.
        /// </summary>
        /// <param name="id">The ID of the touch device.</param>
        public TouchDevice(long id)
        {
            this.id = id;
            name = "Unknown";
            type = (TouchDeviceType)SDL.GetTouchDeviceType(id);

            var fingerCount = SDL.GetNumTouchFingers(id);
            fingers = new Finger[fingerCount];
            for (int i = 0; i < fingerCount; i++)
            {
                var finger = SDL.GetTouchFinger(id, i);
                fingers[i] = new(finger);
                fingerIdToIndex.Add(finger->Id, i);
            }
        }

        /// <summary>
        /// Gets the ID of the touch device.
        /// </summary>
        public long Id => id;

        /// <summary>
        /// Gets the name of the touch device.
        /// </summary>
        public string Name => name;

        /// <summary>
        /// Gets the type of the touch device.
        /// </summary>
        public TouchDeviceType Type => type;

        /// <summary>
        /// Gets the number of fingers associated with the touch device.
        /// </summary>
        public int FingerCount => fingers.Length;

        /// <summary>
        /// Occurs when a finger is lifted off the touch device.
        /// </summary>
        public event TouchDeviceEventHandler<TouchEventArgs>? TouchUp;

        /// <summary>
        /// Occurs when a finger is placed on the touch device.
        /// </summary>
        public event TouchDeviceEventHandler<TouchEventArgs>? TouchDown;

        /// <summary>
        /// Occurs when a finger moves on the touch device.
        /// </summary>
        public event TouchDeviceEventHandler<TouchMoveEventArgs>? TouchMotion;

        internal (TouchDevice, TouchEventArgs) OnFingerUp(SDLTouchFingerEvent evnt)
        {
            touchEventArgs.Timestamp = evnt.Timestamp;
            touchEventArgs.TouchDeviceId = id;
            touchEventArgs.FingerId = evnt.FingerId;
            touchEventArgs.Pressure = evnt.Pressure;
            touchEventArgs.X = evnt.X;
            touchEventArgs.Y = evnt.Y;
            touchEventArgs.State = FingerState.Up;

            var idx = fingerIdToIndex[evnt.FingerId];
            var finger = fingers[idx];
            finger.OnFingerUp(touchEventArgs);

            TouchUp?.Invoke(this, touchEventArgs);
            return (this, touchEventArgs);
        }

        internal (TouchDevice, TouchEventArgs) OnFingerDown(SDLTouchFingerEvent evnt)
        {
            touchEventArgs.Timestamp = evnt.Timestamp;
            touchEventArgs.TouchDeviceId = id;
            touchEventArgs.FingerId = evnt.FingerId;
            touchEventArgs.Pressure = evnt.Pressure;
            touchEventArgs.X = evnt.X;
            touchEventArgs.Y = evnt.Y;
            touchEventArgs.State = FingerState.Down;

            var idx = fingerIdToIndex[evnt.FingerId];
            var finger = fingers[idx];
            finger.OnFingerDown(touchEventArgs);

            TouchDown?.Invoke(this, touchEventArgs);
            return (this, touchEventArgs);
        }

        internal (TouchDevice, TouchMoveEventArgs) OnFingerMotion(SDLTouchFingerEvent evnt)
        {
            touchMotionEventArgs.Timestamp = evnt.Timestamp;
            touchMotionEventArgs.TouchDeviceId = id;
            touchMotionEventArgs.FingerId = evnt.FingerId;
            touchMotionEventArgs.Pressure = evnt.Pressure;
            touchMotionEventArgs.X = evnt.X;
            touchMotionEventArgs.Y = evnt.Y;
            touchMotionEventArgs.Dx = evnt.Dx;
            touchMotionEventArgs.Dy = evnt.Dy;

            var idx = fingerIdToIndex[evnt.FingerId];
            var finger = fingers[idx];
            finger.OnFingerMotion(touchMotionEventArgs);

            TouchMotion?.Invoke(this, touchMotionEventArgs);
            return (this, touchMotionEventArgs);
        }

        /// <summary>
        /// Checks if a finger with the specified ID is in the "down" state.
        /// </summary>
        /// <param name="fingerId">The ID of the finger to check.</param>
        /// <returns>True if the finger is in the "down" state; otherwise, false.</returns>
        public bool IsDownById(long fingerId)
        {
            return IsDownByIndex(fingerIdToIndex[fingerId]);
        }

        /// <summary>
        /// Checks if a finger with the specified ID is in the "up" state.
        /// </summary>
        /// <param name="fingerId">The ID of the finger to check.</param>
        /// <returns>True if the finger is in the "up" state; otherwise, false.</returns>
        public bool IsUpById(long fingerId)
        {
            return IsUpByIndex(fingerIdToIndex[fingerId]);
        }

        /// <summary>
        /// Checks if a finger at the specified index is in the "down" state.
        /// </summary>
        /// <param name="index">The index of the finger to check.</param>
        /// <returns>True if the finger is in the "down" state; otherwise, false.</returns>
        public bool IsDownByIndex(int index)
        {
            return fingers[index].State == FingerState.Down;
        }

        /// <summary>
        /// Checks if a finger at the specified index is in the "up" state.
        /// </summary>
        /// <param name="index">The index of the finger to check.</param>
        /// <returns>True if the finger is in the "up" state; otherwise, false.</returns>
        public bool IsUpByIndex(int index)
        {
            return fingers[index].State == FingerState.Up;
        }

        /// <summary>
        /// Gets a finger by its ID.
        /// </summary>
        /// <param name="fingerId">The ID of the finger to retrieve.</param>
        /// <returns>The <see cref="Finger"/> associated with the specified ID.</returns>
        public Finger GetFingerById(long fingerId)
        {
            return GetFingerByIndex(fingerIdToIndex[fingerId]);
        }

        /// <summary>
        /// Gets a finger by its index.
        /// </summary>
        /// <param name="index">The index of the finger to retrieve.</param>
        /// <returns>The <see cref="Finger"/> at the specified index.</returns>
        public Finger GetFingerByIndex(int index)
        {
            return fingers[index];
        }
    }
}