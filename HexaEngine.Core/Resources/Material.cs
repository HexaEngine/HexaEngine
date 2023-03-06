namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core.Effects;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.Meshes;

    public unsafe class Material : IDisposable
    {
        private readonly string name;
        private bool dirty = true;
        public MaterialData desc;
        public IBuffer CB;
        public ISamplerState SamplerState;
        public ResourceInstance<MaterialShader>? Shader;
        public ResourceInstance<IShaderResourceView>? AlbedoTexture;
        public ResourceInstance<IShaderResourceView>? NormalTexture;
        public ResourceInstance<IShaderResourceView>? DisplacementTexture;
        public ResourceInstance<IShaderResourceView>? RoughnessTexture;
        public ResourceInstance<IShaderResourceView>? MetalnessTexture;
        public ResourceInstance<IShaderResourceView>? EmissiveTexture;
        public ResourceInstance<IShaderResourceView>? AoTexture;
        public ResourceInstance<IShaderResourceView>? RoughnessMetalnessTexture;
        public void** SRVs;
        private int instances;
        private bool disposedValue;
        private bool loaded;

        public Material(MaterialData desc, IBuffer cB, ISamplerState samplerState)
        {
            this.desc = desc;
            name = desc.Name;
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
                context.Write(CB, (CBMaterial)desc);
                dirty = false;
            }
            context.DSSetConstantBuffer(CB, 2);
            context.PSSetConstantBuffer(CB, 2);
            context.PSSetSampler(SamplerState, 0);
            context.DSSetSampler(SamplerState, 0);
            context.PSSetShaderResources(SRVs, 7, 0);
            if (DisplacementTexture != null)
                context.DSSetShaderResource(DisplacementTexture.Value, 0);
            return true;
        }

        public void DrawDepth(IGraphicsContext context, IBuffer camera, uint indexCount, uint instanceCount)
        {
            if (!loaded) return;
            if (dirty)
            {
                context.Write(CB, (CBMaterial)desc);
                dirty = false;
            }
            if (!Shader.Value.BeginDrawDepth(context, camera))
            {
                return;
            }
            context.DSSetConstantBuffer(CB, 2);
            context.PSSetConstantBuffer(CB, 2);
            context.PSSetSampler(SamplerState, 0);
            context.DSSetSampler(SamplerState, 0);
            context.PSSetShaderResources(SRVs, 7, 0);
            if (DisplacementTexture != null)
                context.DSSetShaderResource(DisplacementTexture.Value, 0);
            context.DrawIndexedInstanced(indexCount, instanceCount, 0, 0, 0);
        }

        public void Draw(IGraphicsContext context, IBuffer camera, uint indexCount, uint instanceCount)
        {
            if (!loaded) return;
            if (dirty)
            {
                context.Write(CB, (CBMaterial)desc);
                dirty = false;
            }
            if (!Shader.Value.BeginDraw(context, camera))
            {
                return;
            }
            context.DSSetConstantBuffer(CB, 2);
            context.PSSetConstantBuffer(CB, 2);
            context.PSSetSampler(SamplerState, 0);
            context.DSSetSampler(SamplerState, 0);
            context.PSSetShaderResources(SRVs, 7, 0);
            if (DisplacementTexture != null)
                context.DSSetShaderResource(DisplacementTexture.Value, 0);
            context.DrawIndexedInstanced(indexCount, instanceCount, 0, 0, 0);
        }

        public void DrawIndirect(IGraphicsContext context, IBuffer camera, IBuffer argBuffer, uint offset)
        {
            if (!loaded) return;
            if (dirty)
            {
                context.Write(CB, (CBMaterial)desc);
                dirty = false;
            }
            if (!Shader.Value.BeginDraw(context, camera))
            {
                return;
            }
            context.DSSetConstantBuffer(CB, 2);
            context.PSSetConstantBuffer(CB, 2);
            context.PSSetSampler(SamplerState, 0);
            context.DSSetSampler(SamplerState, 0);
            context.PSSetShaderResources(SRVs, 7, 0);
            if (DisplacementTexture != null)
                context.DSSetShaderResource(DisplacementTexture.Value, 0);

            context.DrawIndexedInstancedIndirect(argBuffer, offset);
        }

        public void Update(MaterialData desc)
        {
            this.desc = desc;
            dirty = true;
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
            dirty = true;
            if (AlbedoTexture != null)
                SRVs[0] = (void*)AlbedoTexture.Pointer;
            else
                SRVs[0] = null;
            if (NormalTexture != null)
                SRVs[1] = (void*)NormalTexture.Pointer;
            else
                SRVs[1] = null;
            if (RoughnessTexture != null)
                SRVs[2] = (void*)RoughnessTexture.Pointer;
            else
                SRVs[2] = null;
            if (MetalnessTexture != null)
                SRVs[3] = (void*)MetalnessTexture.Pointer;
            else
                SRVs[3] = null;
            if (EmissiveTexture != null)
                SRVs[4] = (void*)EmissiveTexture.Pointer;
            else
                SRVs[4] = null;
            if (AoTexture != null)
                SRVs[5] = (void*)AoTexture.Pointer;
            else
                SRVs[5] = null;
            if (RoughnessMetalnessTexture != null)
                SRVs[6] = (void*)RoughnessMetalnessTexture.Pointer;
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

        ~Material()
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