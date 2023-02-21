namespace HexaEngine.Editor.Materials.Generator.Structs
{
    public struct Operator
    {
        public string Op;
        public SType Left;
        public SType Right;
        public SType ReturnType;

        public Operator(string op, SType left, SType right, SType returnType)
        {
            Op = op;
            Left = left;
            Right = right;
            ReturnType = returnType;
        }
    }
}