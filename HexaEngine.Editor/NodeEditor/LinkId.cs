namespace HexaEngine.Editor.NodeEditor
{
    public struct LinkId
    {
        public int Id;
        public int IdInputNode;
        public int IdOutputNode;
        public int IdInput;
        public int IdOutput;

        public LinkId(int id, int idInputNode, int idOutputNode, int idInput, int idOutput)
        {
            Id = id;
            IdInputNode = idInputNode;
            IdOutputNode = idOutputNode;
            IdInput = idInput;
            IdOutput = idOutput;
        }
    }
}