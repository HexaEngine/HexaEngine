using HexaEngine.Core;
using HexaEngine.Core.Assets;
using HexaEngine.Core.Audio;
using HexaEngine.Core.IO;

namespace HexaEngine.Audio
{
    public sealed class Sound : ISound
    {
        private readonly ISound sound;
        private readonly EventHandlers<AudioSourceState> stateChangedHandlers = new();

        private Sound(ISound sound)
        {
            this.sound = sound;
            sound.StateChanged += OnStateChanged;
        }

        private void OnStateChanged(object? sender, AudioSourceState state)
        {
            stateChangedHandlers.Invoke(this, state);
        }

        public static Sound Create(in Stream stream, SoundFlags flags = SoundFlags.None)
        {
            var device = AudioManager.Device;
            var sound = device.CreateSound(stream, flags);
            return new Sound(sound);
        }

        public static Sound Create(in AssetRef assetRef, SoundFlags flags = SoundFlags.None)
        {
            var device = AudioManager.Device;
            var sound = device.CreateSound(assetRef, flags);
            return new Sound(sound);
        }

        public static Sound Create(in AssetPath filePath, SoundFlags flags = SoundFlags.None)
        {
            var device = AudioManager.Device;
            var sound = device.CreateSound(filePath, flags);
            return new Sound(sound);
        }

        public IEmitter? Emitter { get => sound.Emitter; set => sound.Emitter = value; }

        public float Gain { get => sound.Gain; set => sound.Gain = value; }

        public bool Looping { get => sound.Looping; set => sound.Looping = value; }

        public float Pitch { get => sound.Pitch; set => sound.Pitch = value; }

        public float Pan { get => sound.Pan; set => sound.Pan = value; }

        public PanMode PanMode { get => sound.PanMode; set => sound.PanMode = value; }

        public AudioSourceState State => sound.State;

        public IAudioInputNode? Submix { get => sound.Submix; set => sound.Submix = value; }

        public event EventHandler<AudioSourceState>? StateChanged
        {
            add => stateChangedHandlers.AddHandler(value);
            remove => stateChangedHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Gets the underlying native sound object. This property provides access to the low-level sound implementation.
        /// </summary>
        public ISound NativeSound => sound;

        public void Pause()
        {
            sound.Pause();
        }

        public void Play()
        {
            sound.Play();
        }

        public void Rewind()
        {
            sound.Rewind();
        }

        public void Stop()
        {
            sound.Stop();
        }

        public void Dispose()
        {
            stateChangedHandlers.Clear();
            sound.StateChanged -= OnStateChanged;
            sound.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}