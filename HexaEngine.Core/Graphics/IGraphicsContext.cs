namespace HexaEngine.Core.Graphics
{
    using Hexa.NET.Mathematics;
    using System.Numerics;

    /// <summary>
    /// Provides an interface for graphics context management and operations on a graphics device.
    /// </summary>
    public interface IGraphicsContext : IDeviceChild
    {
        void SetResourceBindingList(IResourceBindingList resourceBindings);

        void UnsetResourceBindingList(IResourceBindingList resourceBindings);

        /// <summary>
        /// Gets the graphics device associated with this graphics context.
        /// </summary>
        IGraphicsDevice Device { get; }

        /// <summary>
        /// Sets the active compute pipeline state for rendering.
        /// </summary>
        /// <param name="state">The compute pipeline state to set. Pass null to unset the active pipeline state.</param>
        void SetComputePipelineState(IComputePipelineState? state);

        /// <summary>
        /// Sets the active graphics pipeline state for rendering.
        /// </summary>
        /// <param name="state">The graphics pipeline state to set. Pass null to unset the active pipeline state.</param>
        void SetGraphicsPipelineState(IGraphicsPipelineState? state);

        /// <summary>
        /// Copies data from one resource to another.
        /// </summary>
        /// <param name="dst">The destination resource to copy data to.</param>
        /// <param name="src">The source resource to copy data from.</param>
        void CopyResource(IResource dst, IResource src);

        /// <summary>
        /// Copies the structure count from an unordered access view (UAV) to a buffer.
        /// </summary>
        /// <param name="dst">The destination buffer to store the structure count.</param>
        /// <param name="alignedByteOffset">The aligned byte offset in the buffer.</param>
        /// <param name="uav">The unordered access view (UAV) containing the structure count.</param>
        void CopyStructureCount(IBuffer dst, uint alignedByteOffset, IUnorderedAccessView uav);

        /// <summary>
        /// Writes data to a buffer from a pointer.
        /// </summary>
        /// <param name="buffer">The target buffer to write data to.</param>
        /// <param name="value">A pointer to the data to write.</param>
        /// <param name="size">The size of the data to write, in bytes.</param>
        unsafe void Write(IBuffer buffer, void* value, int size);

        /// <summary>
        /// Writes data from a pointer to a buffer with optional mapping flags.
        /// </summary>
        /// <param name="buffer">The target buffer to write data to.</param>
        /// <param name="value">A pointer to the data to write.</param>
        /// <param name="size">The size of the data to write, in bytes.</param>
        /// <param name="flags">Optional mapping flags specifying how the buffer should be mapped.</param>
        unsafe void Write(IBuffer buffer, void* value, int size, MapMode flags);

        /// <summary>
        /// Writes data from a pointer to a buffer.
        /// </summary>
        /// <typeparam name="T">The type of data to write. Must be unmanaged.</typeparam>
        /// <param name="buffer">The target buffer to write data to.</param>
        /// <param name="value">A pointer to the data to write.</param>
        /// <param name="size">The size of the data to write, in bytes.</param>
        unsafe void Write<T>(IBuffer buffer, T* value, int size) where T : unmanaged;

        /// <summary>
        /// Writes data from a pointer to a buffer with optional mapping flags.
        /// </summary>
        /// <typeparam name="T">The type of data to write. Must be unmanaged.</typeparam>
        /// <param name="buffer">The target buffer to write data to.</param>
        /// <param name="value">A pointer to the data to write.</param>
        /// <param name="size">The size of the data to write, in bytes.</param>
        /// <param name="flags">Optional mapping flags specifying how the buffer should be mapped.</param>
        unsafe void Write<T>(IBuffer buffer, T* value, int size, MapMode flags) where T : unmanaged;

        /// <summary>
        /// Writes a single value of type T to a buffer.
        /// </summary>
        /// <typeparam name="T">The type of data to write. Must be unmanaged.</typeparam>
        /// <param name="buffer">The target buffer to write data to.</param>
        /// <param name="value">The value of type T to write to the buffer.</param>
        void Write<T>(IBuffer buffer, T value) where T : unmanaged;

        /// <summary>
        /// Reads data from a buffer to a pointer with optional mapping flags.
        /// </summary>
        /// <param name="buffer">The source buffer to read data from.</param>
        /// <param name="value">A pointer to store the read data.</param>
        /// <param name="size">The size of the data to read, in bytes.</param>
        unsafe void Read(IBuffer buffer, void* value, int size);

        /// <summary>
        /// Reads data from a buffer to an array of type T with optional mapping flags.
        /// </summary>
        /// <typeparam name="T">The type of data to read. Must be unmanaged.</typeparam>
        /// <param name="buffer">The source buffer to read data from.</param>
        /// <param name="values">An array to store the read data.</param>
        /// <param name="count">The number of elements to read from the buffer.</param>
        unsafe void Read<T>(IBuffer buffer, T* values, uint count) where T : unmanaged;

        /// <summary>
        /// Maps a resource to allow read or write access from the CPU.
        /// </summary>
        /// <param name="resource">The resource to map.</param>
        /// <param name="subresourceIndex">The index of the subresource to map.</param>
        /// <param name="mode">The mapping mode (read or write).</param>
        /// <param name="flags">Optional flags for mapping.</param>
        /// <returns>A <see cref="MappedSubresource"/> object representing the mapped data.</returns>
        MappedSubresource Map(IResource resource, int subresourceIndex, MapMode mode, MapFlags flags);

        /// <summary>
        /// Unmaps a previously mapped resource to invalidate CPU access.
        /// </summary>
        /// <param name="resource">The resource to unmap.</param>
        /// <param name="subresourceIndex">The index of the subresource to unmap.</param>
        void Unmap(IResource resource, int subresourceIndex);

        /// <summary>
        /// Sets a vertex buffer for the input assembler stage.
        /// </summary>
        /// <param name="vertexBuffer">The vertex buffer to set.</param>
        /// <param name="stride">The stride (size of each vertex) in bytes.</param>
        void SetVertexBuffer(IBuffer? vertexBuffer, uint stride);

        /// <summary>
        /// Sets a vertex buffer for the input assembler stage with an offset.
        /// </summary>
        /// <param name="vertexBuffer">The vertex buffer to set.</param>
        /// <param name="stride">The stride (size of each vertex) in bytes.</param>
        /// <param name="offset">The offset (in bytes) to start reading from the buffer.</param>
        void SetVertexBuffer(IBuffer? vertexBuffer, uint stride, uint offset);

        /// <summary>
        /// Sets a vertex buffer for the input assembler stage.
        /// </summary>
        /// <param name="slot">The vertex buffer slot to set.</param>
        /// <param name="vertexBuffer">The vertex buffer to set.</param>
        /// <param name="stride">The stride (size of each vertex) in bytes.</param>
        void SetVertexBuffer(uint slot, IBuffer? vertexBuffer, uint stride);

        /// <summary>
        /// Sets a vertex buffer for the input assembler stage with an offset.
        /// </summary>
        /// <param name="slot">The vertex buffer slot to set.</param>
        /// <param name="vertexBuffer">The vertex buffer to set.</param>
        /// <param name="stride">The stride (size of each vertex) in bytes.</param>
        /// <param name="offset">The offset (in bytes) to start reading from the buffer.</param>
        void SetVertexBuffer(uint slot, IBuffer? vertexBuffer, uint stride, uint offset);

        /// <summary>
        /// Sets an index buffer for the input assembler stage.
        /// </summary>
        /// <param name="indexBuffer">The index buffer to set.</param>
        /// <param name="format">The format of the indices in the buffer.</param>
        /// <param name="offset">The offset (in bytes) to start reading from the buffer.</param>
        void SetIndexBuffer(IBuffer? indexBuffer, Format format, int offset);

        void BeginDraw(BeginDrawDesc desc);

        /// <summary>
        /// Clears the specified render target view with the specified clear color.
        /// </summary>
        /// <param name="renderTargetView">The render target view to clear.</param>
        /// <param name="value">The clear color as a <see cref="Vector4"/>.</param>
        void ClearRenderTargetView(IRenderTargetView renderTargetView, Vector4 value);

        /// <summary>
        /// Clears the specified depth-stencil view with the specified clear flags, depth, and stencil values.
        /// </summary>
        /// <param name="depthStencilView">The depth-stencil view to clear.</param>
        /// <param name="flags">The clear flags specifying what to clear.</param>
        /// <param name="depth">The depth value to clear to.</param>
        /// <param name="stencil">The stencil value to clear to.</param>
        void ClearDepthStencilView(IDepthStencilView depthStencilView, DepthStencilClearFlags flags, float depth, byte stencil);

        /// <summary>
        /// Sets the render target and depth-stencil views for rendering.
        /// </summary>
        /// <param name="renderTargetView">The render target view to set. Pass null to unset the render target view.</param>
        /// <param name="depthStencilView">The depth-stencil view to set. Pass null to unset the depth-stencil view.</param>
        void SetRenderTarget(IRenderTargetView? renderTargetView, IDepthStencilView? depthStencilView);

        /// <summary>
        /// Sets the render target, depth-stencil view, and unordered access views for rendering.
        /// </summary>
        /// <param name="renderTargetView">The render target view to set. Pass null to unset the render target view.</param>
        /// <param name="depthStencilView">The depth-stencil view to set. Pass null to unset the depth-stencil view.</param>
        /// <param name="uavSlot">The slot for the unordered access view.</param>
        /// <param name="unorderedAccessView">The unordered access view to set. Pass null to unset the unordered access view.</param>
        /// <param name="uavInitialCount">The initial count for the unordered access view.</param>
        void SetRenderTargetsAndUnorderedAccessViews(IRenderTargetView? renderTargetView, IDepthStencilView? depthStencilView, uint uavSlot, IUnorderedAccessView? unorderedAccessView, uint uavInitialCount = uint.MaxValue);

        /// <summary>
        /// Sets the render targets, depth-stencil view, and unordered access views for rendering using arrays of views and counts.
        /// </summary>
        /// <param name="count">The number of views in the arrays.</param>
        /// <param name="views">An array of pointers to render target views and depth-stencil view.</param>
        /// <param name="depthStencilView">The depth-stencil view to set. Pass null to unset the depth-stencil view.</param>
        /// <param name="uavSlot">The slot for the unordered access view.</param>
        /// <param name="uavCount">The number of unordered access views in the array.</param>
        /// <param name="uavs">An array of pointers to unordered access views.</param>
        /// <param name="pUavInitialCount">An array of initial counts for the unordered access views.</param>
        unsafe void SetRenderTargetsAndUnorderedAccessViews(uint count, void** views, IDepthStencilView? depthStencilView, uint uavSlot, uint uavCount, void** uavs, uint* pUavInitialCount);

        /// <summary>
        /// Sets the scissor rectangle to restrict the rendering area.
        /// </summary>
        /// <param name="x">The x-coordinate of the top-left corner of the scissor rectangle.</param>
        /// <param name="y">The y-coordinate of the top-left corner of the scissor rectangle.</param>
        /// <param name="z">The width of the scissor rectangle.</param>
        /// <param name="w">The height of the scissor rectangle.</param>
        void SetScissorRect(int x, int y, int z, int w);

        /// <summary>
        /// Clears the graphics context's state to its default values.
        /// </summary>
        void ClearState();

        /// <summary>
        /// Sets the viewport for rendering.
        /// </summary>
        /// <param name="viewport">The viewport to set.</param>
        void SetViewport(Viewport viewport);

        /// <summary>
        /// Sets an array of viewports for rendering.
        /// </summary>
        /// <param name="count">The number of viewports in the array.</param>
        /// <param name="viewports">An array of viewports to set.</param>
        unsafe void SetViewports(uint count, Viewport* viewports);

        /// <summary>
        /// Sets the primitive topology for rendering.
        /// </summary>
        /// <param name="topology">The primitive topology to set.</param>
        void SetPrimitiveTopology(PrimitiveTopology topology);

        /// <summary>
        /// Draws non-indexed, instanced geometry using the specified vertex count and instance count.
        /// </summary>
        /// <param name="vertexCount">The number of vertices to draw.</param>
        /// <param name="instanceCount">The number of instances to draw.</param>
        /// <param name="vertexOffset">The offset to start reading vertices from the vertex buffer.</param>
        /// <param name="instanceOffset">The offset to start reading instance data from the instance buffer.</param>
        void DrawInstanced(uint vertexCount, uint instanceCount, uint vertexOffset, uint instanceOffset);

        /// <summary>
        /// Draws indexed, instanced geometry using the specified index count and instance count.
        /// </summary>
        /// <param name="indexCount">The number of indices to draw.</param>
        /// <param name="instanceCount">The number of instances to draw.</param>
        /// <param name="indexOffset">The offset to start reading indices from the index buffer.</param>
        /// <param name="vertexOffset">The offset to start reading vertices from the vertex buffer.</param>
        /// <param name="instanceOffset">The offset to start reading instance data from the instance buffer.</param>
        void DrawIndexedInstanced(uint indexCount, uint instanceCount, uint indexOffset, int vertexOffset, uint instanceOffset);

        /// <summary>
        /// Draws indexed, instanced geometry using indirect arguments from a buffer.
        /// </summary>
        /// <param name="bufferForArgs">The buffer containing the indirect draw arguments.</param>
        /// <param name="alignedByteOffsetForArgs">The aligned byte offset within the buffer for the draw arguments.</param>
        void DrawIndexedInstancedIndirect(IBuffer bufferForArgs, uint alignedByteOffsetForArgs);

        /// <summary>
        /// Draws indexed, instanced geometry using indirect arguments from a pointer.
        /// </summary>
        /// <param name="bufferForArgs">A pointer to the memory location containing the indirect draw arguments.</param>
        /// <param name="alignedByteOffsetForArgs">The aligned byte offset within the memory location for the draw arguments.</param>
        unsafe void DrawIndexedInstancedIndirect(void* bufferForArgs, uint alignedByteOffsetForArgs);

        /// <summary>
        /// Draws non-indexed, instanced geometry using indirect arguments from a buffer.
        /// </summary>
        /// <param name="bufferForArgs">The buffer containing the indirect draw arguments.</param>
        /// <param name="alignedByteOffsetForArgs">The aligned byte offset within the buffer for the draw arguments.</param>
        void DrawInstancedIndirect(IBuffer bufferForArgs, uint alignedByteOffsetForArgs);

        /// <summary>
        /// Draws non-indexed, instanced geometry using indirect arguments from a pointer.
        /// </summary>
        /// <param name="bufferForArgs">A pointer to the memory location containing the indirect draw arguments.</param>
        /// <param name="alignedByteOffsetForArgs">The aligned byte offset within the memory location for the draw arguments.</param>
        unsafe void DrawInstancedIndirect(void* bufferForArgs, uint alignedByteOffsetForArgs);

        /// <summary>
        /// Begins a query operation using the specified query object.
        /// </summary>
        /// <param name="query">The query object to begin the operation with.</param>
        void QueryBegin(IQuery query);

        /// <summary>
        /// Ends a query operation using the specified query object.
        /// </summary>
        /// <param name="query">The query object to end the operation with.</param>
        void QueryEnd(IQuery query);

        /// <summary>
        /// Retrieves the query data from the specified query object.
        /// </summary>
        /// <param name="query">The query object to retrieve data from.</param>
        void QueryGetData(IQuery query);

        /// <summary>
        /// Flushes any pending rendering commands to the GPU.
        /// </summary>
        void Flush();

        /// <summary>
        /// Dispatches a compute shader with the specified thread group counts.
        /// </summary>
        /// <param name="threadGroupCountX">The number of thread groups in the X direction.</param>
        /// <param name="threadGroupCountY">The number of thread groups in the Y direction.</param>
        /// <param name="threadGroupCountZ">The number of thread groups in the Z direction.</param>
        void Dispatch(uint threadGroupCountX, uint threadGroupCountY, uint threadGroupCountZ);

        /// <summary>
        /// Dispatches a compute shader indirectly using the specified dispatch arguments.
        /// </summary>
        /// <param name="dispatchArgs">The buffer containing the dispatch arguments.</param>
        /// <param name="offset">The offset to start reading dispatch arguments from the buffer.</param>
        void DispatchIndirect(IBuffer dispatchArgs, uint offset);

        /// <summary>
        /// Generates mipmaps for a shader resource view.
        /// </summary>
        /// <param name="resourceView">The shader resource view to generate mipmaps for.</param>
        void GenerateMips(IShaderResourceView resourceView);

        /// <summary>
        /// Executes the specified command list.
        /// </summary>
        /// <param name="commandList">The command list to execute.</param>
        /// <param name="restoreState">True to restore the rendering state after execution; otherwise, false.</param>
        [Obsolete("Use command buffers")]
        void ExecuteCommandList(ICommandList commandList, bool restoreState);

        /// <summary>
        /// Finishes and returns a new command list.
        /// </summary>
        /// <param name="restoreState">True to restore the rendering state after finishing; otherwise, false.</param>
        /// <returns>A new command list that can be used for rendering commands.</returns>
        [Obsolete("Use command buffers")]
        ICommandList FinishCommandList(bool restoreState);

        void ExecuteCommandBuffer(ICommandBuffer commandBuffer);

        /// <summary>
        /// Updates a subresource of a resource.
        /// </summary>
        /// <param name="resource">The resource to update.</param>
        /// <param name="destSubresource">The index of the destination subresource.</param>
        /// <param name="subresource">The subresource data containing the update.</param>
        void UpdateSubresource(IResource resource, int destSubresource, MappedSubresource subresource);

        void UpdateSubresource(IResource resource, int destSubresource, Box? box, MappedSubresource subresource);

        /// <summary>
        /// Sets render targets for rendering using an array of views and an optional depth-stencil view.
        /// </summary>
        /// <param name="count">The number of render targets to set.</param>
        /// <param name="views">An array of pointers to render target views to set.</param>
        /// <param name="depthStencilView">The optional depth-stencil view to set.</param>
        unsafe void SetRenderTargets(uint count, void** views, IDepthStencilView? depthStencilView);

        /// <summary>
        /// Clears multiple render target views with the specified color values.
        /// </summary>
        /// <param name="count">The number of render target views to clear.</param>
        /// <param name="rtvs">An array of pointers to render target views to clear.</param>
        /// <param name="value">The color value to clear the render target views with.</param>
        unsafe void ClearRenderTargetViews(uint count, void** rtvs, Vector4 value);

        /// <summary>
        /// Clears an unordered access view with unsigned integer values.
        /// </summary>
        /// <param name="uav">The unordered access view to clear.</param>
        /// <param name="r">The red channel value to clear the unordered access view with.</param>
        /// <param name="g">The green channel value to clear the unordered access view with.</param>
        /// <param name="b">The blue channel value to clear the unordered access view with.</param>
        /// <param name="a">The alpha channel value to clear the unordered access view with.</param>
        void ClearUnorderedAccessViewUint(IUnorderedAccessView uav, uint r, uint g, uint b, uint a);

        /// <summary>
        /// Clears a render target view with the specified color value within a specified rectangular region.
        /// </summary>
        /// <param name="rtv">The render target view to clear.</param>
        /// <param name="color">The color value to clear the render target view with.</param>
        /// <param name="rect">The rectangular region to clear in the render target view.</param>
        void ClearView(IRenderTargetView rtv, Vector4 color, Rect rect);

        /// <summary>
        /// Clears a depth-stencil view with the specified color value within a specified rectangular region.
        /// </summary>
        /// <param name="dsv">The depth-stencil view to clear.</param>
        /// <param name="color">The color value to clear the depth-stencil view with.</param>
        /// <param name="rect">The rectangular region to clear in the depth-stencil view.</param>
        void ClearView(IDepthStencilView dsv, Vector4 color, Rect rect);

        /// <summary>
        /// Clears an unordered access view with the specified color value within a specified rectangular region.
        /// </summary>
        /// <param name="uav">The unordered access view to clear.</param>
        /// <param name="color">The color value to clear the unordered access view with.</param>
        /// <param name="rect">The rectangular region to clear in the unordered access view.</param>
        void ClearView(IUnorderedAccessView uav, Vector4 color, Rect rect);

        /// <summary>
        /// Marks the beginning of a section of event code.
        /// </summary>
        /// <param name="name">A string that contains the name of the event. The name is not relevant to the operating system.</param>
        void BeginEvent(string name);

        /// <summary>
        /// Marks the end of a section of event code.
        /// </summary>
        void EndEvent();

        /// <summary>
        /// Updates a fence to a specified value after all previous work has completed.
        /// </summary>
        /// <remarks>Note: This method only applies to immediate-mode contexts.</remarks>
        /// <param name="fence">A Fence object.</param>
        /// <param name="value">The value to set the fence to.</param>
        void Signal(IFence fence, ulong value);

        /// <summary>
        /// Waits until the specified fence reaches or exceeds the specified value before future work can begin.
        /// </summary>
        /// <remarks>Note: This method only applies to immediate-mode contexts.</remarks>
        /// <param name="fence">A Fence object.</param>
        /// <param name="value">The value that the device context is waiting for the fence to reach or exceed. So when <seealso cref="IFence.GetCompletedValue"/> is greater than or equal to Value, the wait is terminated.</param>
        void Wait(IFence fence, ulong value);
    }

    public static class IGraphicsContextExtensions
    {
        public struct EventScope : IDisposable
        {
            private IGraphicsContext? context;

            public EventScope(IGraphicsContext? context)
            {
                this.context = context;
            }

            public void Dispose()
            {
                context?.EndEvent();
                context = null;
            }
        }

        public static EventScope BeginEventScope(this IGraphicsContext context, string name)
        {
            context.BeginEvent(name);
            return new EventScope(context);
        }
    }
}