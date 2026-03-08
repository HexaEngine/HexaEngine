namespace HexaEngine.MiniAudio
{
    using Hexa.NET.MiniAudio;
    using HexaEngine.Core;
    using HexaEngine.Core.Audio;

    public unsafe class MiniAudioSoundBase : DisposableRefBase, IAudioDeviceChild, IMiniAudioNode
    {
        internal protected MaSound* Sound;

        public MiniAudioSoundBase(MaSound* sound)
        {
            Sound = sound;
        }

        public nint NativePointer => (nint)Sound;

        public void* Node => Sound;

        protected override void DisposeCore()
        {
            if (Sound != null)
            {
                MiniAudio.SoundGroupUninit(Sound);
                Free(Sound);
                Sound = null;
            }
        }
    }
}
