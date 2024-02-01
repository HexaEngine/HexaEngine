﻿namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Lights;

    public unsafe class Material : ResourceInstance, IDisposable
    {
        private MaterialData desc;

        public ResourceInstance<MaterialShader>? Shader;

        public MaterialTextureList TextureList = new();

        private bool loaded;

        public Material(IResourceFactory factory, MaterialData desc) : base(factory, desc.Name)
        {
            this.desc = desc;
        }

        public MaterialData Desc => desc;

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

            TextureList.Bind(context);

            //context.PSSetSamplers(TextureList.StartSlot, TextureList.SlotCount, TextureList.Samplers);
            //context.PSSetShaderResources(TextureList.StartSlot, TextureList.SlotCount, TextureList.ShaderResourceViews);

            return true;
        }

        public unsafe void EndDrawForward(IGraphicsContext context)
        {
            nint* temp = stackalloc nint[(int)TextureList.SlotCount];
            context.PSSetSamplers(0, TextureList.SlotCount, (void**)temp);
            context.PSSetShaderResources(0, TextureList.SlotCount, (void**)temp);
            Shader.Value.EndDrawForward(context);
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

        public void DrawDeferred(IGraphicsContext context, uint indexCount, uint instanceCount)
        {
            if (!BeginDrawDeferred(context))
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

        public bool BeginDrawDepth(IGraphicsContext context)
        {
            if (!loaded)
            {
                return false;
            }

            if (!Shader.Value.BeginDrawDepth(context))
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

        public void DrawDepth(IGraphicsContext context, uint indexCount, uint instanceCount)
        {
            if (!BeginDrawDepth(context))
            {
                return;
            }

            context.DrawIndexedInstanced(indexCount, instanceCount, 0, 0, 0);

            EndDrawDepth(context);
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