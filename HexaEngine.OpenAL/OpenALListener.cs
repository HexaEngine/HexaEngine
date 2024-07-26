namespace HexaEngine.OpenAL
{
    using HexaEngine.Core.Audio;
    using Hexa.NET.Mathematics;
    using Silk.NET.OpenAL;
    using System;
    using System.Numerics;

    public class OpenALListener : IListener
    {
        private static OpenALListener? active;
        private readonly IMasteringVoice masteringVoice;
        private bool isActive;
        private float gain;
        private AudioOrientation orientation;
        private Vector3 position;
        private Vector3 velocity;
        private bool disposedValue;

        internal OpenALListener(IMasteringVoice masteringVoice)
        {
            this.masteringVoice = masteringVoice;
            masteringVoice.GainChanged += MasteringVoice_GainChanged;
            gain = masteringVoice.Gain;
        }

        private void MasteringVoice_GainChanged(float value)
        {
            gain = value;
            if (!isActive)
            {
                return;
            }

            al.SetListenerProperty(ListenerFloat.Gain, value);
        }

        public AudioOrientation Orientation
        {
            get => orientation;
            set
            {
                orientation = value;
                if (!isActive)
                {
                    return;
                }

                unsafe
                {
                    var valu = value;
                    valu.At = new(value.At.X, value.At.Y, -value.At.Z);
                    al.SetListenerProperty(ListenerFloatArray.Orientation, (float*)&valu);
                }
            }
        }

        public Vector3 Position
        {
            get => position;
            set
            {
                position = value;
                if (!isActive)
                {
                    return;
                }

                al.SetListenerProperty(ListenerVector3.Position, new(value.X, value.Y, value.Z));
            }
        }

        public Vector3 Velocity
        {
            get => velocity;
            set
            {
                velocity = value;
                if (!isActive)
                {
                    return;
                }

                al.SetListenerProperty(ListenerVector3.Velocity, value);
            }
        }

        public bool IsActive
        {
            get => isActive;
            set
            {
                if (isActive == value)
                {
                    return;
                }

                if (value)
                {
                    Active = this;
                }
                else
                {
                    Active = null;
                }
            }
        }

        public static OpenALListener? Active
        {
            get => active;
            set
            {
                if (active == value)
                {
                    return;
                }

                if (active != null)
                {
                    active.isActive = false;
                }

                if (value != null)
                {
                    value.isActive = true;
                    value.SetListenerActive();
                    active = value;
                }
                else
                {
                    ResetListener();
                }
            }
        }

        private void SetListenerActive()
        {
            al.SetListenerProperty(ListenerFloat.Gain, gain);
            unsafe
            {
                var orient = orientation;
                al.SetListenerProperty(ListenerFloatArray.Orientation, (float*)&orient);
            }
            al.SetListenerProperty(ListenerVector3.Position, position);
            al.SetListenerProperty(ListenerVector3.Velocity, velocity);
        }

        private static void ResetListener()
        {
            al.SetListenerProperty(ListenerFloat.Gain, 1);
            unsafe
            {
                al.SetListenerProperty(ListenerFloatArray.Orientation, null);
            }
            al.SetListenerProperty(ListenerVector3.Position, Vector3.Zero);
            al.SetListenerProperty(ListenerVector3.Velocity, Vector3.Zero);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (isActive)
                {
                    Active = null;
                }

                masteringVoice.GainChanged -= MasteringVoice_GainChanged;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}