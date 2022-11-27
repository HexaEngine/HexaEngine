namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Objects;

    public unsafe class ModelMaterial : IDisposable
    {
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
        public int Instances;
        private bool disposedValue;

        public ModelMaterial(Material material, IBuffer cB, ISamplerState samplerState)
        {
            Material = material;
            CB = cB;
            SamplerState = samplerState;
            SRVs = AllocArray(7);
        }

        public void Bind(IGraphicsContext context)
        {
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
            context.DSSetShaderResource(DisplacementTexture?.SRV, 0);
        }

        public void Update()
        {
#nullable disable
            dirty = true;
            SRVs[0] = (void*)(AlbedoTexture?.SRV.NativePointer);
            SRVs[1] = (void*)(NormalTexture?.SRV.NativePointer);
            SRVs[2] = (void*)(RoughnessTexture?.SRV.NativePointer);
            SRVs[3] = (void*)(MetalnessTexture?.SRV.NativePointer);
            SRVs[4] = (void*)(EmissiveTexture?.SRV.NativePointer);
            SRVs[5] = (void*)(AoTexture?.SRV.NativePointer);
            SRVs[6] = (void*)(RoughnessMetalnessTexture?.SRV.NativePointer);
#nullable enable
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Free(SRVs);
                CB.Dispose();
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
    }
}