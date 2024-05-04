namespace HexaEngine.Input
{
    public struct VirtualAxisBindingState
    {
        public float Value;
        public VirtualAxisBindingStateFlags Flags;

        public VirtualAxisBindingState(float value, VirtualAxisBindingStateFlags flags)
        {
            Value = value;
            Flags = flags;
        }

        public override readonly string ToString()
        {
            return $"{Value}, {Flags}";
        }
    }
}