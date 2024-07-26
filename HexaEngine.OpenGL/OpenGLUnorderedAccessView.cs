namespace HexaEngine.OpenGL
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.OpenGL;

    public class OpenGLUnorderedAccessView : DeviceChildBase, IUnorderedAccessView
    {
        private UnorderedAccessViewDescription description;
        internal uint resource;

        public OpenGLUnorderedAccessView(uint resource, UnorderedAccessViewDescription description)
        {
            this.resource = resource;
            this.description = description;

            Identifier = description.ViewDimension switch
            {
                UnorderedAccessViewDimension.Unknown => throw new ArgumentException(),
                UnorderedAccessViewDimension.Buffer => ObjectIdentifier.Buffer,
                UnorderedAccessViewDimension.Texture1D => ObjectIdentifier.Texture,
                UnorderedAccessViewDimension.Texture1DArray => ObjectIdentifier.Texture,
                UnorderedAccessViewDimension.Texture2D => ObjectIdentifier.Texture,
                UnorderedAccessViewDimension.Texture2DArray => ObjectIdentifier.Texture,
                UnorderedAccessViewDimension.Texture3D => ObjectIdentifier.Texture,
                _ => throw new ArgumentException(),
            };
            nativePointer = resource;
        }

        public UnorderedAccessViewDescription Description => description;

        protected override ObjectIdentifier Identifier { get; }

        protected override void DisposeCore()
        {
        }
    }
}