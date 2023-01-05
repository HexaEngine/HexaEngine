namespace HexaEngine.OpenAL
{
    public unsafe class SubmixVoice
    {
        private float gain = 1;

        public SubmixVoice(string name, MasteringVoice master)
        {
            Name = name;
            Master = master;
        }

        public string Name { get; set; }

        public MasteringVoice Master { get; }

        public float Gain
        {
            get => gain;
            set
            {
                if (gain == value) return;
                gain = value;
                GainChanged?.Invoke(value);
            }
        }

        public event Action<float>? GainChanged;
    }
}