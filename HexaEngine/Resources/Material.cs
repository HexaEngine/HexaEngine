namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Materials;

    public unsafe class Material : ResourceInstance, IDisposable
    {
        private MaterialData desc;

        public MaterialShader Shader;
        public MaterialTextureList TextureList = [];
        public MaterialTextureList TextureListDS = [];

        private readonly Dictionary<string, int> nameToPassIndex = [];

        private volatile bool loaded;

        public Material(IResourceFactory factory, MaterialData desc, ResourceGuid id) : base(factory, id)
        {
            this.desc = desc;
        }

        public MaterialData Data => desc;

        public bool BeginDraw(IGraphicsContext context, string passName)
        {
            if (!loaded)
            {
                return false;
            }

            var pass = Shader.Find(passName);
            if (pass == null)
            {
                return false;
            }

            if (!pass.BeginDraw(context))
            {
                return false;
            }

            TextureList.BindPS(context);
            TextureListDS.BindDS(context);

            return true;
        }

        public void EndDraw(IGraphicsContext context)
        {
            TextureList.UnbindPS(context);
            TextureListDS.UnbindDS(context);
            context.SetGraphicsPipelineState(null);
        }

        public void Update(MaterialData desc)
        {
            this.desc = desc;
        }

        public void BeginUpdate()
        {
            loaded = false;
            nameToPassIndex.Clear();
        }

        public void EndUpdate()
        {
#nullable disable
            TextureListDS.Update();
            TextureList.Update();
            loaded = true;
#nullable enable
        }

        protected override void ReleaseResources()
        {
            nameToPassIndex.Clear();
            loaded = false;
        }

        public override string ToString()
        {
            return $"{Data.Name}##{Id}";
        }

        public void DrawIndexedInstanced(IGraphicsContext context, string pass, uint indexCount, uint instanceCount, uint indexOffset = 0, int vertexOffset = 0, uint instanceOffset = 0)
        {
            if (!BeginDraw(context, pass))
            {
                return;
            }
            context.DrawIndexedInstanced(indexCount, instanceCount, indexOffset, vertexOffset, instanceOffset);
            EndDraw(context);
        }

        public void DrawIndexedInstancedIndirect(IGraphicsContext context, string pass, IBuffer drawArgs, uint offset)
        {
            if (!BeginDraw(context, pass))
            {
                return;
            }
            context.DrawIndexedInstancedIndirect(drawArgs, offset);
            EndDraw(context);
        }

        public void DrawInstanced(IGraphicsContext context, string pass, uint vertexCount, uint instanceCount, uint vertexOffset = 0, uint instanceOffset = 0)
        {
            if (!BeginDraw(context, pass))
            {
                return;
            }
            context.DrawInstanced(vertexCount, instanceCount, vertexOffset, instanceOffset);
            EndDraw(context);
        }
    }
}