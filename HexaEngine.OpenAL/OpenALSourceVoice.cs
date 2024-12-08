namespace HexaEngine.OpenAL
{
    using HexaEngine.Core.Audio;
    using Hexa.NET.OpenAL;
    using System.Numerics;

    public class OpenALSourceVoice : ISourceVoice
    {
        public const int SourceSpatializeSoft = 0x1214;
        private uint source;
        private readonly IAudioStream stream;
        private int state;
        private bool disposedValue;
        private ISubmixVoice? submix;

        private float pitch = 1;
        private float gain = 1;

        internal OpenALSourceVoice(uint sourceVoice, IAudioStream audioStream)
        {
            source = sourceVoice;
            stream = audioStream;
            OpenAL.SetSourceProperty(source, ALEnum.ReferenceDistance, 1f);
            OpenAL.SetSourceProperty(source, ALEnum.MaxDistance, float.PositiveInfinity);
            OpenAL.SetSourceProperty(source, ALEnum.RolloffFactor, 1f);
            OpenAL.SetSourceProperty(source, ALEnum.Pitch, 1f);
            OpenAL.SetSourceProperty(source, ALEnum.Gain, 1f);
            OpenAL.SetSourceProperty(source, ALEnum.MinGain, 0f);
            OpenAL.SetSourceProperty(source, ALEnum.MaxGain, 1f);
            OpenAL.SetSourceProperty(source, ALEnum.ConeInnerAngle, 360f);
            OpenAL.SetSourceProperty(source, ALEnum.ConeOuterAngle, 360f);
            OpenAL.SetSourceProperty(source, ALEnum.ConeOuterGain, 0);
            OpenAL.SetSourceProperty(source, ALEnum.Position, Vector3.Zero);
            OpenAL.SetSourceProperty(source, ALEnum.Velocity, Vector3.Zero);
            OpenAL.SetSourceProperty(source, ALEnum.Direction, Vector3.Zero);
            OpenAL.SetSourceProperty(source, ALEnum.Looping, 0);
            OpenAL.SetSourceProperty(source, (ALEnum)SourceSpatializeSoft, 1);
            audioStream.Initialize(source);
        }

        public ISubmixVoice? Submix
        {
            get => submix;
            set
            {
                if (submix == value)
                {
                    return;
                }

                if (submix != null)
                {
                    submix.GainChanged -= Submix_GainChanged;
                }
                if (value != null)
                {
                    value.GainChanged += Submix_GainChanged;
                }
                submix = value;
            }
        }

        public IAudioStream Buffer => stream;

        /// <summary>
        /// Specify the pitch to be applied, either at Source, or on mixer results, at Listener.
        /// Range: [0.5f - 2.0f] Default: 1.0f.
        /// </summary>
        public float Pitch
        {
            get => pitch;
            set
            {
                pitch = value;
                OpenAL.SetSourceProperty(source, ALEnum.Pitch, 0f);
            }
        }

        /// <summary>
        /// Indicate the gain (volume amplification) applied. Type: float. Range: [0.0f -
        /// ? ] A value of 1.0 means un-attenuated/unchanged. Each division by 2 equals an
        /// attenuation of -6dB. Each multiplicaton with 2 equals an amplification of +6dB.
        /// A value of 0.0f is meaningless with respect to a logarithmic scale; it is interpreted
        /// as zero volume - the channel is effectively disabled.
        /// </summary>
        public float Gain
        {
            get => gain;
            set
            {
                gain = value;
                if (submix != null)
                {
                    OpenAL.SetSourceProperty(source, ALEnum.Gain, value * submix.Gain);
                }
                else
                {
                    OpenAL.SetSourceProperty(source, ALEnum.Gain, value);
                }
            }
        }

        public bool Looping
        {
            get => stream.Looping;
            set
            {
                stream.Looping = value;
            }
        }

        public AudioSourceState State => Convert(state);

        public event Action<AudioSourceState>? OnStateChanged;

        public event Action? OnPlay;

        public event Action? OnPause;

        public event Action? OnRewind;

        public event Action? OnStop;

        public IEmitter? Emitter { get; set; }

        public void Update()
        {
            if (Emitter != null)
            {
                OpenAL.SetSourceProperty(source, ALEnum.ReferenceDistance, Emitter.ReferenceDistance);
                OpenAL.SetSourceProperty(source, ALEnum.MaxDistance, Emitter.MaxDistance);
                OpenAL.SetSourceProperty(source, ALEnum.RolloffFactor, Emitter.RolloffFactor);
                OpenAL.SetSourceProperty(source, ALEnum.MinGain, Emitter.MinGain);
                OpenAL.SetSourceProperty(source, ALEnum.MaxGain, Emitter.MaxGain);
                OpenAL.SetSourceProperty(source, ALEnum.ConeInnerAngle, Emitter.ConeInnerAngle);
                OpenAL.SetSourceProperty(source, ALEnum.ConeOuterAngle, Emitter.ConeOuterAngle);
                OpenAL.SetSourceProperty(source, ALEnum.ConeOuterGain, Emitter.ConeOuterGain);
                OpenAL.SetSourceProperty(source, ALEnum.Position, Emitter.Position);
                OpenAL.SetSourceProperty(source, ALEnum.Velocity, Emitter.Velocity);
                OpenAL.SetSourceProperty(source, ALEnum.Direction, Emitter.Direction);
            }

            OpenAL.GetBufferProperty(source, ALEnum.SourceState, out int stateValue);
            var newState = stateValue;
            if (newState != state)
            {
                state = newState;
                OnStateChanged?.Invoke(Convert(state));
            }
            if (state == (int)ALEnum.Playing)
            {
                stream.Update(source);
            }
        }

        public void Play()
        {
            OpenAL.SourcePlay(source);
            OnPlay?.Invoke();
        }

        public void Stop()
        {
            OpenAL.SourceStop(source);
            stream.Reset();
            OnStop?.Invoke();
        }

        public void Pause()
        {
            OpenAL.SourcePause(source);
            OnPause?.Invoke();
        }

        public void Rewind()
        {
            OpenAL.SourceRewind(source);
            stream.Reset();
            OnRewind?.Invoke();
        }

        private void Submix_GainChanged(float submixGain)
        {
            OpenAL.SetSourceProperty(source, ALEnum.Gain, gain * submixGain);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                OpenAL.DeleteBuffers(1, ref source);
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