namespace HexaEngine.Input
{
    public struct VirtualAxisState
    {
        public float Value;
        public VirtualAxisStateFlags Flags;

        public VirtualAxisState(float value, VirtualAxisStateFlags flags)
        {
            Value = value;
            Flags = flags;
        }
    }
}