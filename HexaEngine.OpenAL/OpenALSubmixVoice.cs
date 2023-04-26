namespace HexaEngine.OpenAL
{
    using HexaEngine.Core.Audio;

    public unsafe class OpenALSubmixVoice : ISubmixVoice
    {
        private float gain = 1;

        public OpenALSubmixVoice(string name, IMasteringVoice master)
        {
            Name = name;
            Master = master;
        }

        public string Name { get; set; }

        public IMasteringVoice Master { get; }

        public float Gain
        {
            get => gain;
            set
            {
                if (gain == value)
                {
                    return;
                }

                gain = value;
                GainChanged?.Invoke(value);
            }
        }

        public event Action<float>? GainChanged;
    }
}