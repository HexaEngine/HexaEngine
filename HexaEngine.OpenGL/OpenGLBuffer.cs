namespace HexaEngine.OpenGL
{
    using HexaEngine.Core.Graphics;
    using Hexa.NET.OpenGL;

    public class OpenGLBuffer : DeviceChildBase, IBuffer
    {
        private readonly BufferDescription description;
        private uint buffer;

        public OpenGLBuffer(uint buffer, BufferDescription description)
        {
            this.buffer = buffer;
            this.description = description;
            nativePointer = buffer;
        }

        public BufferDescription Description => description;

        public int Length => description.ByteWidth;

        public ResourceDimension Dimension => ResourceDimension.Buffer;

        protected override GLObjectIdentifier Identifier => GLObjectIdentifier.Buffer;

        protected override void DisposeCore()
        {
            OpenGLGraphicsDevice.GL.DeleteBuffer(buffer);
            buffer = 0;
            nativePointer = 0;
        }
    }
}