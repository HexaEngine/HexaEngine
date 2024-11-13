namespace HexaEngine.OpenGL
{
    using HexaEngine.Core.Graphics;
    using Hexa.NET.OpenGL;

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
                UnorderedAccessViewDimension.Buffer => GLObjectIdentifier.Buffer,
                UnorderedAccessViewDimension.Texture1D => GLObjectIdentifier.Texture,
                UnorderedAccessViewDimension.Texture1DArray => GLObjectIdentifier.Texture,
                UnorderedAccessViewDimension.Texture2D => GLObjectIdentifier.Texture,
                UnorderedAccessViewDimension.Texture2DArray => GLObjectIdentifier.Texture,
                UnorderedAccessViewDimension.Texture3D => GLObjectIdentifier.Texture,
                _ => throw new ArgumentException(),
            };
            nativePointer = resource;
        }

        public UnorderedAccessViewDescription Description => description;

        protected override GLObjectIdentifier Identifier { get; }

        protected override void DisposeCore()
        {
        }
    }
}