namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;
    using System;
    using ResourceDimension = Core.Graphics.ResourceDimension;

    public unsafe class D3D11Texture3D : DeviceChildBase, ITexture3D
    {
        private readonly ID3D11Texture3D* texture;

        public D3D11Texture3D(ID3D11Texture3D* texture, Texture3DDescription description)
        {
            this.texture = texture;
            nativePointer = new(texture);
            Description = description;
        }

        public Texture3DDescription Description { get; }

        public ResourceDimension Dimension => ResourceDimension.Texture1D;

        protected override void DisposeCore()
        {
            texture->Release();
        }
    }
}