namespace HexaEngine.Core.Graphics
{
    using System;

    public unsafe interface IBuffer : IResource
    {
        public BufferDescription Description { get; }

        public int Length { get; }
    }
}