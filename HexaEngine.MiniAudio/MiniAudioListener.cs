namespace HexaEngine.MiniAudio
{
    using Hexa.NET.Mathematics;
    using Hexa.NET.MiniAudio;
    using Hexa.NET.Utilities;
    using HexaEngine.Core.Audio;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public unsafe sealed class MiniAudioListener : IListener
    {
        private Atomic<nuint> dirty;
        private AudioOrientation orientation;
        private Vector3 position;
        private Vector3 velocity;
        private float coneInnerAngle = 6.283185f;
        private float coneOuterAngle = 6.283185f;
        private float coneOuterGain;
        private readonly MiniAudioDevice device;

        public MiniAudioListener(MiniAudioDevice device)
        {
            this.device = device;
        }

        public bool IsActive { get => device.ActiveListener == this; set => device.ActiveListener = this; }

        public AudioOrientation Orientation { get => orientation; set => SetAndDirty(ref orientation, value); }

        public Vector3 Position { get => position; set => SetAndDirty(ref position, value); }

        public Vector3 Velocity { get => velocity; set => SetAndDirty(ref velocity, value); }

        public float ConeInnerAngle { get => coneInnerAngle; set => SetAndDirty(ref coneInnerAngle, value); }

        public float ConeOuterAngle { get => coneOuterAngle; set => SetAndDirty(ref coneOuterAngle, value); }

        public float ConeOuterGain { get => coneOuterGain; set => SetAndDirty(ref coneOuterGain, value); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetAndDirty<T>(ref T field, in T value) where T : IEquatable<T>
        {
            if (field.Equals(value)) return;
            field = value;
            dirty.Store(1);
        }

        internal void SetActiveInternal(bool active)
        {
            if (active)
            {
                MiniAudio.EngineListenerSetEnabled(device.Engine, 0, 1);
                UpdateState();
            }
            else
            {
                MiniAudio.EngineListenerSetEnabled(device.Engine, 0, 0);
            }
        }

        internal void UpdateState()
        {
            if (!dirty.CompareExchange(0, 1))
            {
                return;
            }
            MiniAudio.EngineListenerSetPosition(device.Engine, 0, Position.X, Position.Y, -Position.Z);
            MiniAudio.EngineListenerSetDirection(device.Engine, 0, Orientation.At.X, Orientation.At.Y, -Orientation.At.Z);
            MiniAudio.EngineListenerSetWorldUp(device.Engine, 0, Orientation.Up.X, Orientation.Up.Y, -Orientation.Up.Z);
            MiniAudio.EngineListenerSetVelocity(device.Engine, 0, Velocity.X, Velocity.Y, Velocity.Z);
            MiniAudio.EngineListenerSetCone(device.Engine, 0, ConeInnerAngle, ConeOuterAngle, ConeOuterGain);
            MiniAudio.EngineListenerSetEnabled(device.Engine, 0, 1);
        }

        public void Dispose()
        {
            if (IsActive)
            {
                IsActive = false;
            }
        }
    }
}