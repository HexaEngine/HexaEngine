namespace HexaEngine.OpenGL
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.OpenGL;

    public class OpenGLShaderResourceView : DeviceChildBase, IShaderResourceView
    {
        private ShaderResourceViewDescription description;
        internal uint resource;

        public OpenGLShaderResourceView(uint resource, ShaderResourceViewDescription description, ObjectIdentifier identifier)
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
                ShaderResourceViewDimension.Buffer => ObjectIdentifier.Buffer,
                ShaderResourceViewDimension.Texture1D => ObjectIdentifier.Texture,
                ShaderResourceViewDimension.Texture1DArray => ObjectIdentifier.Texture,
                ShaderResourceViewDimension.Texture2D => ObjectIdentifier.Texture,
                ShaderResourceViewDimension.Texture2DArray => ObjectIdentifier.Texture,
                ShaderResourceViewDimension.Texture3D => ObjectIdentifier.Texture,
                _ => throw new ArgumentException(),
            };
            nativePointer = resource;
        }

        public ShaderResourceViewDescription Description => description;

        protected override ObjectIdentifier Identifier { get; }

        protected override void DisposeCore()
        {
        }
    }
}