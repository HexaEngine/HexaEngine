namespace HexaEngine.Materials
{
    public struct PinId
    {
        public int Id;
        public int IdParent;

        public PinId(int id, int idParent)
        {
            Id = id;
            IdParent = idParent;
        }
    }
}