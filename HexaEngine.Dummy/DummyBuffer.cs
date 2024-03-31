namespace HexaEngine.Dummy
{
    using HexaEngine.Core.Graphics;

    public class DummyBuffer : DummyObject, IBuffer
    {
        public DummyBuffer(BufferDescription description)
        {
            Description = description;
            Length = description.ByteWidth;
        }

        public BufferDescription Description { get; }

        public int Length { get; }

        public ResourceDimension Dimension => ResourceDimension.Buffer;
    }
}