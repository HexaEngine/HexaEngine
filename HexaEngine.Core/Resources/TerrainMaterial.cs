namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Materials;

    public unsafe class TerrainMaterial : IDisposable
    {
        private readonly string name;
        public MaterialData desc;

        public ResourceInstance<TerrainShader>? Shader;

        public MaterialTextureList TextureList = new();

        private int instances;
        private bool disposedValue;
        private bool loaded;

        public TerrainMaterial(MaterialData desc)
        {
            this.desc = desc;
            name = desc.Name;
        }

        public string Name => name;

        public bool IsUsed => Volatile.Read(ref instances) != 0;

        public bool Bind(IGraphicsContext context, int offset)
        {
            if (!loaded)
            {
                return false;
            }

            context.PSSetSamplers(TextureList.Samplers, (uint)TextureList.SlotCount, offset);
            context.PSSetShaderResources(TextureList.ShaderResourceViews, (uint)TextureList.SlotCount, offset);

            return true;
        }

        public void Update(MaterialData desc)
        {
            this.desc = desc;
        }

        public void AddRef()
        {
            Interlocked.Increment(ref instances);
        }

        public void RemoveRef()
        {
            Interlocked.Decrement(ref instances);
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                loaded = false;
                disposedValue = true;
            }
        }

        ~TerrainMaterial()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            return name;
        }
    }
}