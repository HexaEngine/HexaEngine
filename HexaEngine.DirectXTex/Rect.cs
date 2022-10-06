namespace HexaEngine.DirectXTex
{
    public struct Rect
    {
        public ulong X;
        public ulong Y;
        public ulong W;
        public ulong H;

        public Rect(ulong x, ulong y, ulong w, ulong h)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
        }
    }
}