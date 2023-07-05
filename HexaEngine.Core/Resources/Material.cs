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

        public bool BeginDrawForward(IGraphicsContext context)
        {
            if (!loaded)
            {
                return false;
            }

            if (!Shader.Value.BeginDrawForward(context))
            {
                return false;
            }

            context.PSSetSamplers(0, TextureList.SlotCount, TextureList.Samplers);
            context.PSSetShaderResources(0, TextureList.SlotCount, TextureList.ShaderResourceViews);

            return true;
        }

        public bool BeginDrawDeferred(IGraphicsContext context)
        {
            if (!loaded)
            {
                return false;
            }

            if (!Shader.Value.BeginDrawDeferred(context))
            {
                return false;
            }

            context.PSSetSamplers(0, TextureList.SlotCount, TextureList.Samplers);
            context.PSSetShaderResources(0, TextureList.SlotCount, TextureList.ShaderResourceViews);

            return true;
        }

        public bool BeginDrawForward(IGraphicsContext context, IBuffer camera)
        {
            if (!loaded)
            {
                return false;
            }

            if (!Shader.Value.BeginDrawForward(context, camera))
            {
                return false;
            }

            context.PSSetSamplers(0, TextureList.SlotCount, TextureList.Samplers);
            context.PSSetShaderResources(0, TextureList.SlotCount, TextureList.ShaderResourceViews);

            return true;
        }

        public unsafe void EndDrawForward(IGraphicsContext context)
        {
            nint* temp = stackalloc nint[(int)TextureList.SlotCount];
            context.PSSetSamplers(0, TextureList.SlotCount, (void**)temp);
            context.PSSetShaderResources(0, TextureList.SlotCount, (void**)temp);
            Shader.Value.EndDrawForward(context);
        }

        public bool BeginDrawDeferred(IGraphicsContext context, IBuffer camera)
        {
            if (!loaded)
            {
                return false;
            }

            if (!Shader.Value.BeginDrawDeferred(context, camera))
            {
                return false;
            }

            context.PSSetSamplers(0, TextureList.SlotCount, TextureList.Samplers);
            context.PSSetShaderResources(0, TextureList.SlotCount, TextureList.ShaderResourceViews);

            return true;
        }

        public unsafe void EndDrawDeferred(IGraphicsContext context)
        {
            nint* temp = stackalloc nint[(int)TextureList.SlotCount];
            context.PSSetSamplers(0, TextureList.SlotCount, (void**)temp);
            context.PSSetShaderResources(0, TextureList.SlotCount, (void**)temp);
            Shader.Value.EndDrawDeferred(context);
        }

        public void DrawForward(IGraphicsContext context, uint indexCount, uint instanceCount)
        {
            if (!BeginDrawForward(context))
            {
                return;
            }

            context.DrawIndexedInstanced(indexCount, instanceCount, 0, 0, 0);

            EndDrawForward(context);
        }

        public void DrawForward(IGraphicsContext context, IBuffer camera, uint indexCount, uint instanceCount)
        {
            if (!BeginDrawForward(context, camera))
            {
                return;
            }

            context.DrawIndexedInstanced(indexCount, instanceCount, 0, 0, 0);

            EndDrawForward(context);
        }

        public void DrawDeferred(IGraphicsContext context, uint indexCount, uint instanceCount)
        {
            if (!BeginDrawDeferred(context))
            {
                return;
            }

            context.DrawIndexedInstanced(indexCount, instanceCount, 0, 0, 0);

            EndDrawDeferred(context);
        }

        public void DrawDeferred(IGraphicsContext context, IBuffer camera, uint indexCount, uint instanceCount)
        {
            if (!BeginDrawDeferred(context, camera))
            {
                return;
            }

            context.DrawIndexedInstanced(indexCount, instanceCount, 0, 0, 0);

            EndDrawDeferred(context);
        }

        public bool BeginDrawShadow(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (!loaded)
            {
                return false;
            }

            if (!Shader.Value.BeginDrawShadow(context, light, type))
            {
                return false;
            }

            context.PSSetSamplers(0, TextureList.SlotCount, TextureList.Samplers);
            context.PSSetShaderResources(0, TextureList.SlotCount, TextureList.ShaderResourceViews);

            return true;
        }

        public void EndDrawShadow(IGraphicsContext context)
        {
            nint* temp = stackalloc nint[(int)TextureList.SlotCount];
            context.PSSetSamplers(0, TextureList.SlotCount, (void**)temp);
            context.PSSetShaderResources(0, TextureList.SlotCount, (void**)temp);
            Shader.Value.EndDrawShadow(context);
        }

        public void DrawShadow(IGraphicsContext context, IBuffer light, ShadowType type, uint indexCount, uint instanceCount)
        {
            if (!BeginDrawShadow(context, light, type))
            {
                return;
            }

            context.DrawIndexedInstanced(indexCount, instanceCount, 0, 0, 0);

            EndDrawShadow(context);
        }

        public bool BeginDrawDepth(IGraphicsContext context, IBuffer camera)
        {
            if (!loaded)
            {
                return false;
            }

            if (!Shader.Value.BeginDrawDepth(context, camera))
            {
                return false;
            }

            context.PSSetSamplers(0, TextureList.SlotCount, TextureList.Samplers);
            context.PSSetShaderResources(0, TextureList.SlotCount, TextureList.ShaderResourceViews);

            return true;
        }

        public void EndDrawDepth(IGraphicsContext context)
        {
            nint* temp = stackalloc nint[(int)TextureList.SlotCount];
            context.PSSetSamplers(0, TextureList.SlotCount, (void**)temp);
            context.PSSetShaderResources(0, TextureList.SlotCount, (void**)temp);
            Shader.Value.EndDrawDepth(context);
        }

        public void DrawDepth(IGraphicsContext context, IBuffer camera, uint indexCount, uint instanceCount)
        {
            if (!BeginDrawDepth(context, camera))
            {
                return;
            }

            context.DrawIndexedInstanced(indexCount, instanceCount, 0, 0, 0);

            EndDrawDepth(context);
        }

        public void DrawIndirect(IGraphicsContext context, IBuffer camera, IBuffer argBuffer, uint offset)
        {
            if (!BeginDrawDeferred(context, camera))
            {
                return;
            }

            context.DrawIndexedInstancedIndirect(argBuffer, offset);

            EndDrawDeferred(context);
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