namespace HexaEngine.OpenAL
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Audio;
    using Hexa.NET.OpenAL;
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

            OpenAL.SetListenerProperty(ALEnum.Gain, value);
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
                    OpenAL.SetListenerProperty(ALEnum.Orientation, (float*)&valu);
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

                OpenAL.SetListenerProperty(ALEnum.Position, value.X, value.Y, value.Z);
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

                OpenAL.SetListenerProperty(ALEnum.Velocity, value.X, value.Y, value.Z);
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
            OpenAL.SetListenerProperty(ALEnum.Gain, gain);
            unsafe
            {
                var orient = orientation;
                OpenAL.SetListenerProperty(ALEnum.Orientation, (float*)&orient);
            }
            OpenAL.SetListenerProperty(ALEnum.Position, position.X, position.Y, position.Z);
            OpenAL.SetListenerProperty(ALEnum.Velocity, velocity.X, velocity.Y, velocity.Z);
        }

        private static void ResetListener()
        {
            OpenAL.SetListenerProperty(ALEnum.Gain, 1f);
            unsafe
            {
                OpenAL.SetListenerProperty(ALEnum.Orientation, (float*)null);
            }
            OpenAL.SetListenerProperty(ALEnum.Position, 0, 0, 0);
            OpenAL.SetListenerProperty(ALEnum.Velocity, 0, 0, 0);
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