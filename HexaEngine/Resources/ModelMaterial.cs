namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Objects;

    public unsafe class ModelMaterial : IDisposable
    {
        private readonly string name;
        private bool dirty = true;
        public Material Material;
        public IBuffer CB;
        public ISamplerState SamplerState;
        public ModelTexture? AlbedoTexture;
        public ModelTexture? NormalTexture;
        public ModelTexture? DisplacementTexture;
        public ModelTexture? RoughnessTexture;
        public ModelTexture? MetalnessTexture;
        public ModelTexture? EmissiveTexture;
        public ModelTexture? AoTexture;
        public ModelTexture? RoughnessMetalnessTexture;
        public void** SRVs;
        private int instances;
        private bool disposedValue;
        private bool loaded;

        public ModelMaterial(Material material, IBuffer cB, ISamplerState samplerState)
        {
            name = material.Name;
            Material = material;
            CB = cB;
            SamplerState = samplerState;
            SRVs = AllocArray(7);
        }

        public string Name => name;

        public bool IsUsed => Volatile.Read(ref instances) != 0;

        public bool Bind(IGraphicsContext context)
        {
            if (!loaded) return false;
            if (dirty)
            {
                context.Write(CB, new CBMaterial(Material));
                dirty = false;
            }
            context.DSSetConstantBuffer(CB, 2);
            context.PSSetConstantBuffer(CB, 2);
            context.PSSetSampler(SamplerState, 0);
            context.DSSetSampler(SamplerState, 0);
            context.PSSetShaderResources(SRVs, 7, 0);
            if (DisplacementTexture != null)
                context.DSSetShaderResource(DisplacementTexture.Pointer, 0);
            return true;
        }

        public void AddRef()
        {
            Volatile.Write(ref instances, Volatile.Read(ref instances) + 1);
        }

        public void RemoveRef()
        {
            Volatile.Write(ref instances, Volatile.Read(ref instances) - 1);
        }

        public void BeginUpdate()
        {
            loaded = false;
        }

        public void EndUpdate()
        {
#nullable disable
            dirty = true;
            if (AlbedoTexture != null)
                SRVs[0] = AlbedoTexture.Pointer;
            else
                SRVs[0] = null;
            if (NormalTexture != null)
                SRVs[1] = NormalTexture.Pointer;
            else
                SRVs[1] = null;
            if (RoughnessTexture != null)
                SRVs[2] = RoughnessTexture.Pointer;
            else
                SRVs[2] = null;
            if (MetalnessTexture != null)
                SRVs[3] = MetalnessTexture.Pointer;
            else
                SRVs[3] = null;
            if (EmissiveTexture != null)
                SRVs[4] = EmissiveTexture.Pointer;
            else
                SRVs[4] = null;
            if (AoTexture != null)
                SRVs[5] = AoTexture.Pointer;
            else
                SRVs[5] = null;
            if (RoughnessMetalnessTexture != null)
                SRVs[6] = RoughnessMetalnessTexture.Pointer;
            else
                SRVs[6] = null;
            loaded = true;
#nullable enable
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                loaded = false;
                Free(SRVs);
                CB.Dispose();
                SamplerState.Dispose();
                disposedValue = true;
            }
        }

        ~ModelMaterial()
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