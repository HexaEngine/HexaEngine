namespace HexaEngine.Core.Graphics
{
    public struct Box
    {
        public uint Left;
        public uint Top;
        public uint Right;
        public uint Bottom;
        public uint Front;
        public uint Back;

        public Box(uint left, uint top, uint right, uint bottom, uint front, uint back)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
            Front = front;
            Back = back;
        }
    }
}