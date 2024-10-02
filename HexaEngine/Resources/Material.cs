namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Materials;
    using System.Diagnostics.CodeAnalysis;

    public unsafe class Material : ResourceInstance, IDisposable
    {
        private MaterialData desc;

        public MaterialShader Shader
        {
            get => shader; set
            {
                shader = value;
                TextureList?.Dispose();
                TextureList = new();
            }
        }

        public MaterialTextureList TextureList;
        private MaterialShader shader;

        private readonly SemaphoreSlim semaphore = new(1);
        private int activeUseCount;

        public Material(IResourceFactory factory, MaterialData desc, ResourceGuid id) : base(factory, id)
        {
            this.desc = desc;
        }

        public MaterialData Data => desc;

        public bool Dirty { get; set; }

        public bool IsBeingModified => semaphore.CurrentCount == 0;

        /// <summary>
        /// Gets a pass by its name.
        /// </summary>
        /// <param name="passName"></param>
        /// <remarks>Call <see cref="EndUse"/> after finishing access.</remarks>
        /// <returns></returns>
        public MaterialShaderPass? GetPass(string passName)
        {
            if (IsBeingModified)
            {
                return null;
            }

            Interlocked.Increment(ref activeUseCount);

            var pass = Shader.Find(passName);
            if (pass == null)
            {
                Interlocked.Decrement(ref activeUseCount);
                return null;
            }

            return pass;
        }

        public bool BeginUse()
        {
            if (IsBeingModified)
            {
                return false;
            }

            Interlocked.Increment(ref activeUseCount);

            return true;
        }

        public bool BeginDraw(string passName, [NotNullWhen(true)] out MaterialShaderPass? pass)
        {
            if (IsBeingModified)
            {
                pass = null;
                return false;
            }

            Interlocked.Increment(ref activeUseCount);

            pass = Shader.Find(passName);
            return pass != null;
        }

        public bool BeginDraw(IGraphicsContext context, string passName)
        {
            if (IsBeingModified)
            {
                return false;
            }

            Interlocked.Increment(ref activeUseCount);

            var pass = Shader.Find(passName);
            if (pass == null)
            {
                Interlocked.Decrement(ref activeUseCount);
                return false;
            }

            if (!pass.BeginDraw(context))
            {
                Interlocked.Decrement(ref activeUseCount);
                return false;
            }

            return true;
        }

        public void EndDraw(IGraphicsContext context)
        {
            context.SetGraphicsPipelineState(null);
            Interlocked.Decrement(ref activeUseCount);
        }

        public void EndUse()
        {
            Interlocked.Decrement(ref activeUseCount);
        }

        public void Update(MaterialData desc)
        {
            this.desc = desc;
        }

        public void BeginUpdate()
        {
            semaphore.Wait();

            while (Interlocked.CompareExchange(ref activeUseCount, 0, 0) > 0)
            {
                Thread.Yield();
            }
        }

        public void EndUpdate()
        {
#nullable disable
            TextureList.Update(Shader);

            Dirty = true;
            semaphore.Release();
#nullable enable
        }

        protected override void ReleaseResources()
        {
            semaphore.Wait();
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