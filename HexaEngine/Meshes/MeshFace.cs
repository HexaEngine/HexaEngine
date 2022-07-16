namespace HexaEngine.Meshes
{
    public struct MeshFace
    {
        public int Vertex1;
        public int Vertex2;
        public int Vertex3;

        public static bool operator ==(MeshFace a, MeshFace b)
        {
            return a.Vertex1 == b.Vertex1 && a.Vertex2 == b.Vertex2 && a.Vertex3 == b.Vertex3;
        }

        public static bool operator !=(MeshFace a, MeshFace b)
        {
            return !(a == b);
        }

        public override bool Equals(object? obj)
        {
            if (obj is MeshFace face)
                return face == this;
            return false;
        }

        public override int GetHashCode()
        {
            return Vertex1.GetHashCode() ^ Vertex2.GetHashCode() ^ Vertex3.GetHashCode();
        }
    }
}