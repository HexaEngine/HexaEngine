﻿namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;
    using System;
    using ResourceDimension = Core.Graphics.ResourceDimension;

    public unsafe class D3D11Texture2D : DeviceChildBase, ITexture2D
    {
        private readonly ID3D11Texture2D* texture;

        public D3D11Texture2D(ID3D11Texture2D* texture, Texture2DDescription description)
        {
            this.texture = texture;
            nativePointer = new(texture);
            Description = description;
        }

        public Texture2DDescription Description { get; }

        public ResourceDimension Dimension => ResourceDimension.Texture1D;

        protected override void DisposeCore()
        {
            texture->Release();
        }
    }
}