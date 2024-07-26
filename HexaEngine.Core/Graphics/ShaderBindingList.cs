namespace HexaEngine.Core.Graphics
{
    using Hexa.NET.Utilities;
    using System;

    /// <summary>
    /// Represents a list of shader bindings for a specific shader stage and binding type.
    /// </summary>
    public unsafe class ShaderBindingList : IDisposable
    {
        private UnsafeList<nint> list = [];
        private uint baseSlot = uint.MaxValue;
        private bool disposedValue;
        private readonly ShaderStage stage;
        private readonly ShaderBindingType type;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderBindingList"/> class.
        /// </summary>
        /// <param name="stage">The shader stage associated with this binding list.</param>
        /// <param name="type">The shader binding type associated with this binding list.</param>
        public ShaderBindingList(ShaderStage stage, ShaderBindingType type)
        {
            this.stage = stage;
            this.type = type;
        }

        /// <summary>
        /// Gets the shader stage associated with this binding list.
        /// </summary>
        public ShaderStage Stage => stage;

        /// <summary>
        /// Gets the shader binding type associated with this binding list.
        /// </summary>
        public ShaderBindingType Type => type;

        /// <summary>
        /// Sets a shader resource view, constant buffer, or sampler for the specified slot.
        /// </summary>
        /// <param name="slot">The slot to set the binding for.</param>
        /// <param name="resource">The pointer to the shader resource view, constant buffer, or sampler.</param>
        public void Set(uint slot, nint resource)
        {
            var size = Math.Max(slot + 1, list.Size);
            list.Resize(size);
            list[slot] = resource;
            baseSlot = Math.Min(baseSlot, slot);
        }

        /// <summary>
        /// Unsets a shader resource view, constant buffer, or sampler for the specified slot.
        /// </summary>
        /// <param name="slot">The slot to unset the binding for.</param>
        public void Unset(uint slot)
        {
            var newSize = list.Size;
            if (slot == newSize - 1)
            {
                newSize--;
                list.Resize(newSize);
                return;
            }

            list[slot] = 0;

            if (baseSlot == slot)
            {
                // underflow is okay in this case, because that indicates that the list is empty.
                baseSlot = unchecked((uint)list.FirstIndexOf(x => x != 0));
                if (baseSlot == uint.MaxValue)
                {
                    list.Clear();
                }
            }
        }

        /// <summary>
        /// Clears all bindings in the list.
        /// </summary>
        public void Clear()
        {
            list.Clear();
        }

        /// <summary>
        /// Binds the shader resource views, constant buffers, or samplers in the list to the graphics context.
        /// </summary>
        /// <param name="context">The graphics context to which the bindings will be bound.</param>
        public void Bind(IGraphicsContext context)
        {
            switch (type)
            {
                case ShaderBindingType.ConstantBuffer:
                    BindConstantBuffers(context);
                    break;

                case ShaderBindingType.ShaderResource:
                    BindShaderResources(context);
                    break;

                case ShaderBindingType.SamplerState:
                    BindSamplers(context);
                    break;
            }
        }

        /// <summary>
        /// Unbinds the shader resource views, constant buffers, or samplers in the list from the graphics context.
        /// </summary>
        /// <param name="context">The graphics context from which the bindings will be unbound.</param>
        public void Unbind(IGraphicsContext context)
        {
            switch (type)
            {
                case ShaderBindingType.ConstantBuffer:
                    UnbindConstantBuffers(context);
                    break;

                case ShaderBindingType.ShaderResource:
                    UnbindShaderResources(context);
                    break;

                case ShaderBindingType.SamplerState:
                    UnbindSamplers(context);
                    break;
            }
        }


        private void BindShaderResources(IGraphicsContext context)
        {
            uint size = list.Size - baseSlot;
            if (size == 0 || baseSlot == uint.MaxValue)
            {
                return;
            }

            switch (stage)
            {
                case ShaderStage.Vertex:
                    context.VSSetShaderResources(baseSlot, size, (void**)list.Data);
                    break;

                case ShaderStage.Hull:
                    context.HSSetShaderResources(baseSlot, size, (void**)list.Data);
                    break;

                case ShaderStage.Domain:
                    context.DSSetShaderResources(baseSlot, size, (void**)list.Data);
                    break;

                case ShaderStage.Geometry:
                    context.GSSetShaderResources(baseSlot, size, (void**)list.Data);
                    break;

                case ShaderStage.Pixel:
                    context.PSSetShaderResources(baseSlot, size, (void**)list.Data);
                    break;

                case ShaderStage.Compute:
                    context.CSSetShaderResources(baseSlot, size, (void**)list.Data);
                    break;
            }
        }

        private void UnbindShaderResources(IGraphicsContext context)
        {
            uint size = list.Size - baseSlot;
            if (size == 0 || baseSlot == uint.MaxValue)
            {
                return;
            }

            nint* temp = stackalloc nint[(int)size];

            switch (stage)
            {
                case ShaderStage.Vertex:
                    context.VSSetShaderResources(baseSlot, size, (void**)temp);
                    break;

                case ShaderStage.Hull:
                    context.HSSetShaderResources(baseSlot, size, (void**)temp);
                    break;

                case ShaderStage.Domain:
                    context.DSSetShaderResources(baseSlot, size, (void**)temp);
                    break;

                case ShaderStage.Geometry:
                    context.GSSetShaderResources(baseSlot, size, (void**)temp);
                    break;

                case ShaderStage.Pixel:
                    context.PSSetShaderResources(baseSlot, size, (void**)temp);
                    break;

                case ShaderStage.Compute:
                    context.CSSetShaderResources(baseSlot, size, (void**)temp);
                    break;
            }
        }

        private void BindConstantBuffers(IGraphicsContext context)
        {
            uint size = list.Size - baseSlot;
            if (size == 0 || baseSlot == uint.MaxValue)
            {
                return;
            }

            switch (stage)
            {
                case ShaderStage.Vertex:
                    context.VSSetConstantBuffers(baseSlot, size, (void**)list.Data);
                    break;

                case ShaderStage.Hull:
                    context.HSSetConstantBuffers(baseSlot, size, (void**)list.Data);
                    break;

                case ShaderStage.Domain:
                    context.DSSetConstantBuffers(baseSlot, size, (void**)list.Data);
                    break;

                case ShaderStage.Geometry:
                    context.GSSetConstantBuffers(baseSlot, size, (void**)list.Data);
                    break;

                case ShaderStage.Pixel:
                    context.PSSetConstantBuffers(baseSlot, size, (void**)list.Data);
                    break;

                case ShaderStage.Compute:
                    context.CSSetConstantBuffers(baseSlot, size, (void**)list.Data);
                    break;
            }
        }

        private void UnbindConstantBuffers(IGraphicsContext context)
        {
            uint size = list.Size - baseSlot;
            if (size == 0 || baseSlot == uint.MaxValue)
            {
                return;
            }

            nint* temp = stackalloc nint[(int)size];

            switch (stage)
            {
                case ShaderStage.Vertex:
                    context.VSSetConstantBuffers(baseSlot, size, (void**)temp);
                    break;

                case ShaderStage.Hull:
                    context.HSSetConstantBuffers(baseSlot, size, (void**)temp);
                    break;

                case ShaderStage.Domain:
                    context.DSSetConstantBuffers(baseSlot, size, (void**)temp);
                    break;

                case ShaderStage.Geometry:
                    context.GSSetConstantBuffers(baseSlot, size, (void**)temp);
                    break;

                case ShaderStage.Pixel:
                    context.PSSetConstantBuffers(baseSlot, size, (void**)temp);
                    break;

                case ShaderStage.Compute:
                    context.CSSetConstantBuffers(baseSlot, size, (void**)temp);
                    break;
            }
        }

        private void BindSamplers(IGraphicsContext context)
        {
            uint size = list.Size - baseSlot;
            if (size == 0 || baseSlot == uint.MaxValue)
            {
                return;
            }

            switch (stage)
            {
                case ShaderStage.Vertex:
                    context.VSSetSamplers(baseSlot, size, (void**)list.Data);
                    break;

                case ShaderStage.Hull:
                    context.HSSetSamplers(baseSlot, size, (void**)list.Data);
                    break;

                case ShaderStage.Domain:
                    context.DSSetSamplers(baseSlot, size, (void**)list.Data);
                    break;

                case ShaderStage.Geometry:
                    context.GSSetSamplers(baseSlot, size, (void**)list.Data);
                    break;

                case ShaderStage.Pixel:
                    context.PSSetSamplers(baseSlot, size, (void**)list.Data);
                    break;

                case ShaderStage.Compute:
                    context.CSSetSamplers(baseSlot, size, (void**)list.Data);
                    break;
            }
        }

        private void UnbindSamplers(IGraphicsContext context)
        {
            uint size = list.Size - baseSlot;
            if (size == 0 || baseSlot == uint.MaxValue)
            {
                return;
            }

            nint* temp = stackalloc nint[(int)size];

            switch (stage)
            {
                case ShaderStage.Vertex:
                    context.VSSetSamplers(baseSlot, size, (void**)temp);
                    break;

                case ShaderStage.Hull:
                    context.HSSetSamplers(baseSlot, size, (void**)temp);
                    break;

                case ShaderStage.Domain:
                    context.DSSetSamplers(baseSlot, size, (void**)temp);
                    break;

                case ShaderStage.Geometry:
                    context.GSSetSamplers(baseSlot, size, (void**)temp);
                    break;

                case ShaderStage.Pixel:
                    context.PSSetSamplers(baseSlot, size, (void**)temp);
                    break;

                case ShaderStage.Compute:
                    context.CSSetSamplers(baseSlot, size, (void**)temp);
                    break;
            }
        }

        /// <summary>
        /// Releases the resources used by the <see cref="ShaderBindingList"/>.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from the Dispose method.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                list.Release();
                disposedValue = true;
            }
        }

        /// <summary>
        /// Releases the resources used by the <see cref="ShaderBindingList"/>.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}