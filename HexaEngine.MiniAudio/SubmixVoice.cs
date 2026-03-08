namespace HexaEngine.MiniAudio
{
    using Hexa.NET.MiniAudio;
    using HexaEngine.Core.Audio;

    public unsafe sealed class SubmixVoice : MiniAudioSoundBase, ISubmixVoice
    {
        private IAudioInputNode? parent;

        public SubmixVoice(string name, in MaSound* group) : base(group)
        {
            Name = name;
        }

        public IAudioInputNode? Parent
        {
            get => parent;
            set
            {
                if (parent == value) return;
                if (parent != null)
                {
                    MiniAudio.NodeDetachOutputBus(Sound, 0).CheckError();
                }

                if (value is IMiniAudioNode parentNode)
                {
                    MiniAudio.NodeAttachOutputBus(Sound, 0, parentNode.Node, 0).CheckError();
                }
                parent = value;
            }
        }

        public string Name { get; set; }

        public float Gain { get => MiniAudio.SoundGroupGetVolume(Sound); set => MiniAudio.SoundGroupSetVolume(Sound, value); }

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
