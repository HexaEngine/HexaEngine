namespace HexaEngine.OpenGL
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.OpenGL;

    public class OpenGLBuffer : DeviceChildBase, IBuffer
    {
        private readonly BufferDescription description;
        private uint buffer;

        public OpenGLBuffer(GL gl, uint buffer, BufferDescription description)
        {
            this.gl = gl;
            this.buffer = buffer;
            this.description = description;
            nativePointer = buffer;
        }

        public BufferDescription Description => description;

        public int Length => description.ByteWidth;

        public ResourceDimension Dimension => ResourceDimension.Buffer;

        protected override ObjectIdentifier Identifier => ObjectIdentifier.Buffer;

        protected override void DisposeCore()
        {
            gl.DeleteBuffer(buffer);
            buffer = 0;
            nativePointer = 0;
        }
    }
}