namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.Lights;

    public unsafe class Material : IDisposable
    {
        private readonly string name;
        public MaterialData desc;

        public ResourceInstance<MaterialShader>? Shader;

        public MaterialTextureList TextureList = new();

        private int instances;
        private bool disposedValue;
        private bool loaded;

        public Material(MaterialData desc)
        {
            this.desc = desc;
            name = desc.Name;
        }

        public string Name => name;

        public bool IsUsed => Volatile.Read(ref instances) != 0;

        public bool Bind(IGraphicsContext context)
        {
            if (!loaded) return false;

            context.PSSetSamplers(TextureList.Samplers, (uint)TextureList.SlotCount, 0);
            context.PSSetShaderResources(TextureList.ShaderResourceViews, (uint)TextureList.SlotCount, 0);

            return true;
        }

        public void DrawShadow(IGraphicsContext context, IBuffer light, ShadowType type, uint indexCount, uint instanceCount)
        {
            if (!loaded) return;

            if (!Shader.Value.BeginDrawShadow(context, light, type))
            {
                return;
            }

            context.PSSetSamplers(TextureList.Samplers, (uint)TextureList.SlotCount, 0);
            context.PSSetShaderResources(TextureList.ShaderResourceViews, (uint)TextureList.SlotCount, 0);

            context.DrawIndexedInstanced(indexCount, instanceCount, 0, 0, 0);
        }

        public void DrawDepth(IGraphicsContext context, IBuffer camera, uint indexCount, uint instanceCount)
        {
            if (!loaded) return;

            if (!Shader.Value.BeginDrawDepth(context, camera))
            {
                return;
            }

            context.PSSetSamplers(TextureList.Samplers, (uint)TextureList.SlotCount, 0);
            context.PSSetShaderResources(TextureList.ShaderResourceViews, (uint)TextureList.SlotCount, 0);

            context.DrawIndexedInstanced(indexCount, instanceCount, 0, 0, 0);
        }

        public void Draw(IGraphicsContext context, IBuffer camera, uint indexCount, uint instanceCount)
        {
            if (!loaded) return;

            if (!Shader.Value.BeginDraw(context, camera))
            {
                return;
            }

            context.PSSetSamplers(TextureList.Samplers, (uint)TextureList.SlotCount, 0);
            context.PSSetShaderResources(TextureList.ShaderResourceViews, (uint)TextureList.SlotCount, 0);
            context.DrawIndexedInstanced(indexCount, instanceCount, 0, 0, 0);
        }

        public void DrawIndirect(IGraphicsContext context, IBuffer camera, IBuffer argBuffer, uint offset)
        {
            if (!loaded) return;
            if (!Shader.Value.BeginDraw(context, camera))
            {
                return;
            }
            //TextureList.Bind(context);
            context.PSSetSamplers(TextureList.Samplers, (uint)TextureList.SlotCount, 0);
            context.PSSetShaderResources(TextureList.ShaderResourceViews, (uint)TextureList.SlotCount, 0);
            context.DrawIndexedInstancedIndirect(argBuffer, offset);
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