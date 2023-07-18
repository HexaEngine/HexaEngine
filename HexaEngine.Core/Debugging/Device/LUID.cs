namespace HexaEngine.Core.Debugging.Device
{
    public struct LUID
    {
        public uint Low;

        public int High;

        public LUID(uint low, int high)
        {
            Low = low;
            High = high;
        }
    }
}