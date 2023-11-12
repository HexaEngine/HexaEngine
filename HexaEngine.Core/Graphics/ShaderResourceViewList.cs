namespace HexaEngine.Core.Graphics
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a list of shader resource views for different shader stages.
    /// </summary>
    public unsafe class ShaderResourceViewList : IDisposable
    {
        private readonly List<ShaderBinding> bindings = new();
        private readonly List<ShaderBindingList> stages = new();
        private bool disposedValue;


        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderResourceViewList"/> class.
        /// </summary>
        public ShaderResourceViewList()
        {
            stages.Add(new(ShaderStage.Vertex, ShaderBindingType.ShaderResource));
            stages.Add(new(ShaderStage.Hull, ShaderBindingType.ShaderResource));
            stages.Add(new(ShaderStage.Domain, ShaderBindingType.ShaderResource));
            stages.Add(new(ShaderStage.Geometry, ShaderBindingType.ShaderResource));
            stages.Add(new(ShaderStage.Pixel, ShaderBindingType.ShaderResource));
            stages.Add(new(ShaderStage.Compute, ShaderBindingType.ShaderResource));
        }

        /// <summary>
        /// Tries to get a shader binding by name.
        /// </summary>
        /// <param name="name">The name of the shader binding.</param>
        /// <param name="binding">When this method returns, contains the binding with the specified name, if found; otherwise, the default value.</param>
        /// <returns><c>true</c> if a binding with the specified name is found; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Adds a shader binding to the list.
        /// </summary>
        /// <param name="binding">The shader binding to add.</param>
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

        /// <summary>
        /// Removes a shader binding from the list.
        /// </summary>
        /// <param name="binding">The name of the shader binding to remove.</param>
        /// <returns><c>true</c> if the binding was successfully removed; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Clears all shader bindings from the list.
        /// </summary>
        public void ClearBindings()
        {
            bindings.Clear();
        }

        /// <summary>
        /// Gets the shader binding list for a specific shader stage.
        /// </summary>
        /// <param name="stage">The shader stage.</param>
        /// <returns>The shader binding list for the specified shader stage.</returns>
        public ShaderBindingList GetSubList(ShaderStage stage)
        {
            return stages[(int)stage];
        }

        /// <summary>
        /// Sets a shader resource view for a specific shader binding.
        /// </summary>
        /// <param name="binding">The name of the shader binding.</param>
        /// <param name="value">The pointer to the shader resource view.</param>
        /// <returns><c>true</c> if the shader resource view was set successfully; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Unsets a shader resource view for a specific shader binding.
        /// </summary>
        /// <param name="binding">The name of the shader binding.</param>
        /// <returns><c>true</c> if the shader resource view was unset successfully; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Clears all shader resource views in the list.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < stages.Count; i++)
            {
                stages[i].Clear();
            }
        }

        /// <summary>
        /// Binds all shader resource views in the list to the graphics context.
        /// </summary>
        /// <param name="context">The graphics context to which the resource views will be bound.</param>
        public void Bind(IGraphicsContext context)
        {
            for (int i = 0; i < stages.Count; i++)
            {
                stages[i].Bind(context);
            }
        }

        /// <summary>
        /// Unbinds all shader resource views in the list from the graphics context.
        /// </summary>
        /// <param name="context">The graphics context from which the resource views will be unbound.</param>
        public void Unbind(IGraphicsContext context)
        {
            for (int i = 0; i < stages.Count; i++)
            {
                stages[i].Unbind(context);
            }
        }

        /// <summary>
        /// Releases the resources used by the <see cref="ShaderResourceViewList"/>.
        /// </summary>
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

        /// <summary>
        /// Releases the resources used by the <see cref="ShaderResourceViewList"/>.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}