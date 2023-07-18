namespace HexaEngine.Mathematics
{
    public struct Rational
    {
        public uint Numerator;
        public uint Denominator;

        public Rational(uint numerator, uint denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }
    }
}