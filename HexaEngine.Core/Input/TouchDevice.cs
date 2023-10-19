namespace HexaEngine.Core.Input
{
    using HexaEngine.Core.Input.Events;
    using Silk.NET.SDL;
    using System;

    public unsafe class TouchDevice
    {
        private static readonly Sdl sdl = Application.sdl;
        private readonly long id;
        private readonly string name;
        private readonly TouchDeviceType type;
        private readonly Finger[] fingers;
        private readonly Dictionary<long, int> fingerIdToIndex = new();

        private readonly TouchEventArgs touchEventArgs = new();
        private readonly TouchMotionEventArgs touchMotionEventArgs = new();

        public TouchDevice(int index)
        {
            id = sdl.GetTouchDevice(index);
            name = sdl.GetTouchNameS(index);
            type = (TouchDeviceType)sdl.GetTouchDeviceType(id);

            var fingerCount = sdl.GetNumTouchFingers(id);
            fingers = new Finger[fingerCount];
            for (int i = 0; i < fingerCount; i++)
            {
                var finger = sdl.GetTouchFinger(id, i);
                fingers[i] = new(finger);
                fingerIdToIndex.Add(finger->Id, i);
            }
        }

        public TouchDevice(long id)
        {
            this.id = id;
            name = "Unknown";
            type = (TouchDeviceType)sdl.GetTouchDeviceType(id);

            var fingerCount = sdl.GetNumTouchFingers(id);
            fingers = new Finger[fingerCount];
            for (int i = 0; i < fingerCount; i++)
            {
                var finger = sdl.GetTouchFinger(id, i);
                fingers[i] = new(finger);
                fingerIdToIndex.Add(finger->Id, i);
            }
        }

        public long Id => id;

        public string Name => name;

        public TouchDeviceType Type => type;

        public int FingerCount => fingers.Length;

        public event EventHandler<TouchEventArgs>? TouchUp;

        public event EventHandler<TouchEventArgs>? TouchDown;

        public event EventHandler<TouchMotionEventArgs>? TouchMotion;

        internal (TouchDevice, TouchEventArgs) OnFingerUp(TouchFingerEvent evnt)
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

        internal (TouchDevice, TouchEventArgs) OnFingerDown(TouchFingerEvent evnt)
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

        internal (TouchDevice, TouchMotionEventArgs) OnFingerMotion(TouchFingerEvent evnt)
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

        public bool IsDownById(long fingerId)
        {
            return IsDownByIndex(fingerIdToIndex[fingerId]);
        }

        public bool IsUpById(long fingerId)
        {
            return IsUpByIndex(fingerIdToIndex[fingerId]);
        }

        public bool IsDownByIndex(int index)
        {
            return fingers[index].State == FingerState.Down;
        }

        public bool IsUpByIndex(int index)
        {
            return fingers[index].State == FingerState.Up;
        }

        public Finger GetFingerById(long fingerId)
        {
            return GetFingerByIndex(fingerIdToIndex[fingerId]);
        }

        public Finger GetFingerByIndex(int index)
        {
            return fingers[index];
        }
    }
}