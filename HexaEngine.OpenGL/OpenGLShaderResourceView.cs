namespace HexaEngine.OpenGL
{
    using HexaEngine.Core.Graphics;
    using Hexa.NET.OpenGL;

    public class OpenGLShaderResourceView : DeviceChildBase, IShaderResourceView
    {
        private ShaderResourceViewDescription description;
        internal uint resource;

        public OpenGLShaderResourceView(uint resource, ShaderResourceViewDescription description, GLObjectIdentifier identifier)
        {
            this.resource = resource;
            this.description = description;
            Identifier = identifier;
            nativePointer = resource;
        }

        public OpenGLShaderResourceView(uint resource, ShaderResourceViewDescription description)
        {
            this.resource = resource;
            this.description = description;
            Identifier = description.ViewDimension switch
            {
                ShaderResourceViewDimension.Unknown => throw new ArgumentException(),
                ShaderResourceViewDimension.Buffer => GLObjectIdentifier.Buffer,
                ShaderResourceViewDimension.Texture1D => GLObjectIdentifier.Texture,
                ShaderResourceViewDimension.Texture1DArray => GLObjectIdentifier.Texture,
                ShaderResourceViewDimension.Texture2D => GLObjectIdentifier.Texture,
                ShaderResourceViewDimension.Texture2DArray => GLObjectIdentifier.Texture,
                ShaderResourceViewDimension.Texture3D => GLObjectIdentifier.Texture,
                _ => throw new ArgumentException(),
            };
            nativePointer = resource;
        }

        public ShaderResourceViewDescription Description => description;

        protected override GLObjectIdentifier Identifier { get; }

        protected override void DisposeCore()
        {
        }
    }
}