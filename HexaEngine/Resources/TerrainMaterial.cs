namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Materials;

    public unsafe class TerrainMaterial : ResourceInstance
    {
        public MaterialData desc;

        public MaterialTextureList TextureList = new();

        private bool loaded;

        public TerrainMaterial(IResourceFactory factory, MaterialData desc) : base(factory, desc.Name)
        {
            this.desc = desc;
        }

        public bool Bind(IGraphicsContext context, uint offset)
        {
            if (!loaded)
            {
                return false;
            }

            context.PSSetSamplers(offset, TextureList.SlotCount, TextureList.Samplers);
            context.PSSetShaderResources(offset, TextureList.SlotCount, TextureList.ShaderResourceViews);

            return true;
        }

        public void Update(MaterialData desc)
        {
            this.desc = desc;
        }

        public void BeginUpdate()
        {
            loaded = false;
        }

        public void EndUpdate()
        {
#nullable disable
            TextureList.Update();
            loaded = true;
#nullable enable
        }

        protected override void ReleaseResources()
        {
            loaded = false;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}