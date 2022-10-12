namespace HexaEngine.Rendering
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Objects;

    public class ModelMaterial : IDisposable
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
        public IShaderResourceView[] SRVs;
        public int Instances;
        private bool disposedValue;

        public ModelMaterial(Material material, IBuffer cB, ISamplerState samplerState)
        {
            Material = material;
            CB = cB;
            SamplerState = samplerState;
            SRVs = new IShaderResourceView[7];
        }

        public void Bind(IGraphicsContext context)
        {
            if (dirty)
            {
                context.Write(CB, new CBMaterial(Material));
                dirty = false;
            }
            context.SetConstantBuffer(CB, ShaderStage.Domain, 2);
            context.SetConstantBuffer(CB, ShaderStage.Pixel, 2);
            context.SetSampler(SamplerState, ShaderStage.Pixel, 0);
            context.SetSampler(SamplerState, ShaderStage.Domain, 0);
            context.SetShaderResources(SRVs, ShaderStage.Pixel, 0);
            context.SetShaderResource(DisplacementTexture?.SRV, ShaderStage.Domain, 0);
        }

        public void Update()
        {
#nullable disable
            dirty = true;
            SRVs[0] = AlbedoTexture?.SRV;
            SRVs[1] = NormalTexture?.SRV;
            SRVs[2] = RoughnessTexture?.SRV;
            SRVs[3] = MetalnessTexture?.SRV;
            SRVs[4] = EmissiveTexture?.SRV;
            SRVs[5] = AoTexture?.SRV;
            SRVs[6] = RoughnessMetalnessTexture?.SRV;
#nullable enable
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
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