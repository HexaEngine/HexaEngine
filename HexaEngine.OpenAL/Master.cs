namespace HexaEngine.OpenAL
{
    public unsafe class MasteringVoice
    {
        private float gain = 1;

        public MasteringVoice(string name)
        {
            Name = name;
        }

        public string Name { get; }

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