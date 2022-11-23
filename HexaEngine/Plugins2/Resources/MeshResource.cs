namespace HexaEngine.Plugins2.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class MeshResource : Resource
    {
        public MeshResource(string name) : base(name)
        {
        }

        public override ResourceType Type => ResourceType.Mesh;

        public override int Read(ReadOnlySpan<byte> source)
        {
            throw new NotImplementedException();
        }

        public override int Size()
        {
            throw new NotImplementedException();
        }

        public override int Write(Span<byte> source)
        {
            throw new NotImplementedException();
        }
    }
}