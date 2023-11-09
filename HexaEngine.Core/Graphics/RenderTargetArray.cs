namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using YamlDotNet.Core.Tokens;

    public class RenderTargetViewArray : IDisposable
    {
        public Vector4 ClearColor;
        private bool disposedValue;
        private IRenderTargetView[] views;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RenderTargetViewArray(IRenderTargetView[] views)
        {
            this.views = views;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RenderTargetViewArray(IGraphicsDevice device, IResource[] resources, Viewport viewport)
        {
            views = new IRenderTargetView[resources.Length];
            for (int i = 0; i < resources.Length; i++)
            {
                views[i] = device.CreateRenderTargetView(resources[i]);
                views[i].DebugName = nameof(RenderTargetViewArray) + "." + i;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RenderTargetViewArray(IGraphicsDevice device, IResource resource, int arraySize, Viewport viewport)
        {
            Viewport = viewport;
            views = new IRenderTargetView[arraySize];
            RenderTargetViewDescription description = new(RenderTargetViewDimension.Texture2DArray, arraySize: 1);
            for (int i = 0; i < arraySize; i++)
            {
                description.Texture2DArray.FirstArraySlice = i;
                views[i] = device.CreateRenderTargetView(resource, description);
                views[i].DebugName = nameof(RenderTargetViewArray) + "." + i;
            }
        }

        public Viewport Viewport { get; }

        public IDepthStencilView? DepthStencil { get; set; }

        public void ClearAndSetTarget(IGraphicsContext context, int i)
        {
            context.ClearRenderTargetView(views[i], new(ClearColor.X, ClearColor.Y, ClearColor.Z, ClearColor.W));
            context.SetRenderTarget(views[i], DepthStencil);
        }

        public void ClearTarget(IGraphicsContext context, int i)
        {
            context.ClearRenderTargetView(views[i], new(ClearColor.X, ClearColor.Y, ClearColor.Z, ClearColor.W));
        }

        public void SetTarget(IGraphicsContext context, int i)
        {
            context.SetRenderTarget(views[i], DepthStencil);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                foreach (IRenderTargetView view in views)
                {
                    view.Dispose();
                }

                disposedValue = true;
            }
        }

        ~RenderTargetViewArray()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class ShaderResourceViewArray : IDisposable
    {
        private bool disposedValue;
        public readonly IShaderResourceView[] Views;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ShaderResourceViewArray(IShaderResourceView[] views)
        {
            Views = views;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ShaderResourceViewArray(IGraphicsDevice device, IResource[] resources)
        {
            Views = new IShaderResourceView[resources.Length];
            for (int i = 0; i < resources.Length; i++)
            {
                Views[i] = device.CreateShaderResourceView(resources[i]);
                Views[i].DebugName = nameof(RenderTargetViewArray) + "." + i;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ShaderResourceViewArray(IGraphicsDevice device, IResource resource, int arraySize)
        {
            Views = new IShaderResourceView[arraySize];
            ShaderResourceViewDescription description = new(ShaderResourceViewDimension.Texture2DArray, arraySize: 1);
            for (int i = 0; i < arraySize; i++)
            {
                description.Texture2DArray.FirstArraySlice = i;
                Views[i] = device.CreateShaderResourceView(resource, description);
                Views[i].DebugName = nameof(RenderTargetViewArray) + "." + i;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                foreach (IShaderResourceView view in Views)
                {
                    view.Dispose();
                }

                disposedValue = true;
            }
        }

        ~ShaderResourceViewArray()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Specifies the type of shader binding.
    /// </summary>
    public enum ShaderBindingType
    {
        /// <summary>
        /// Constant buffer binding (e.g., cX).
        /// </summary>
        ConstantBuffer,

        /// <summary>
        /// Shader resource binding (e.g., tX).
        /// </summary>
        ShaderResource,

        /// <summary>
        /// Sampler state binding (e.g., sX).
        /// </summary>
        SamplerState,

        /// <summary>
        /// Unordered access binding (e.g., uX).
        /// </summary>
        UnorderedAccess
    }

    /// <summary>
    /// Represents a shader binding with a name, type, shader stage, and slot.
    /// </summary>
    public struct ShaderBinding
    {
        /// <summary>
        /// Gets or sets the name of the binding.
        /// </summary>
        public string Name;

        /// <summary>
        /// Gets or sets the shader stage in which the binding is used.
        /// </summary>
        public ShaderStage Stage;

        /// <summary>
        /// Gets or sets the slot or index associated with the binding.
        /// </summary>
        public uint Slot;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderBinding"/> struct.
        /// </summary>
        /// <param name="name">The name of the binding.</param>
        /// <param name="type">The type of the binding (e.g., ConstantBuffer, Texture, Buffer, UnorderedAccess).</param>
        /// <param name="stage">The shader stage in which the binding is used.</param>
        /// <param name="slot">The slot or index associated with the binding.</param>
        public ShaderBinding(string name, ShaderStage stage, uint slot)
        {
            Name = name;
            Stage = stage;
            Slot = slot;
        }
    }

    public unsafe class ShaderResourceViewList : IDisposable
    {
        private readonly List<ShaderBinding> bindings = new();
        private readonly List<ShaderBindingList> stages = new();
        private bool disposedValue;

        public ShaderResourceViewList()
        {
            stages.Add(new(ShaderStage.Vertex, ShaderBindingType.ShaderResource));
            stages.Add(new(ShaderStage.Hull, ShaderBindingType.ShaderResource));
            stages.Add(new(ShaderStage.Domain, ShaderBindingType.ShaderResource));
            stages.Add(new(ShaderStage.Geometry, ShaderBindingType.ShaderResource));
            stages.Add(new(ShaderStage.Pixel, ShaderBindingType.ShaderResource));
            stages.Add(new(ShaderStage.Compute, ShaderBindingType.ShaderResource));
        }

        public bool TryGetBinding(string name, out ShaderBinding binding)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                var bind = bindings[i];
                if (bind.Name == name)
                {
                    binding = bind;
                    return true;
                }
            }

            binding = default;
            return false;
        }

        public void AddBinding(ShaderBinding binding)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                var bind = bindings[i];
                if (bind.Name == binding.Name)
                {
                    throw new InvalidOperationException("A binding with the same name already exists");
                }
            }

            bindings.Add(binding);
        }

        public bool RemoveBinding(string binding)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                var binding2 = bindings[i];
                if (binding2.Name == binding)
                {
                    bindings.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void ClearBindings()
        {
            bindings.Clear();
        }

        public ShaderBindingList GetSubList(ShaderStage stage)
        {
            return stages[(int)stage];
        }

        public bool Set(string binding, nint value)
        {
            if (TryGetBinding(binding, out var bind))
            {
                var subList = GetSubList(bind.Stage);
                subList.Set(bind.Slot, value);
                return true;
            }
            return false;
        }

        public bool Unset(string binding)
        {
            if (TryGetBinding(binding, out var bind))
            {
                var subList = GetSubList(bind.Stage);
                subList.Unset(bind.Slot);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            for (int i = 0; i < stages.Count; i++)
            {
                stages[i].Clear();
            }
        }

        public void Bind(IGraphicsContext context)
        {
            for (int i = 0; i < stages.Count; i++)
            {
                stages[i].Bind(context);
            }
        }

        public void Unbind(IGraphicsContext context)
        {
            for (int i = 0; i < stages.Count; i++)
            {
                stages[i].Unbind(context);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                for (int i = 0; i < stages.Count; i++)
                {
                    stages[i].Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public unsafe class ShaderBindingList : IDisposable
    {
        private UnsafeList<nint> list = [];
        private uint baseSlot = uint.MaxValue;
        private bool disposedValue;
        private readonly ShaderStage stage;
        private readonly ShaderBindingType type;

        public ShaderBindingList(ShaderStage stage, ShaderBindingType type)
        {
            this.stage = stage;
            this.type = type;
        }

        public ShaderStage Stage => stage;

        public ShaderBindingType Type => type;

        public void Set(uint slot, nint resource)
        {
            var size = Math.Max(slot + 1, list.Size);
            list.Resize(size);
            list[slot] = resource;
            baseSlot = Math.Min(baseSlot, slot);
        }

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

        public void Clear()
        {
            list.Clear();
        }

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

        public void BindShaderResources(IGraphicsContext context)
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

        public void UnbindShaderResources(IGraphicsContext context)
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

        public void BindConstantBuffers(IGraphicsContext context)
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

        public void UnbindConstantBuffers(IGraphicsContext context)
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

        public void BindSamplers(IGraphicsContext context)
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

        public void UnbindSamplers(IGraphicsContext context)
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                list.Release();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}