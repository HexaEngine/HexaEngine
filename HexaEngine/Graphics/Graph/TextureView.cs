namespace HexaEngine.Graphics.Graph
{
    using Hexa.NET.Mathematics;
    using Hexa.NET.Utilities;
    using HexaEngine.Core.Graphics;
    using System;

    public abstract class TextureView<T, TDesc> : ResourceRef, IRenderTargetView, IShaderResourceView, IUnorderedAccessView, IDeviceChild where T : class, IDisposable, IRenderTargetView, IShaderResourceView, IUnorderedAccessView where TDesc : struct
    {
        protected readonly TDesc description;

        public TextureView(GraphResourceBuilder builder, string name, in TDesc description) : base(builder, name)
        {
            this.description = description;
        }

        public T Texture => (T)Value!;

        public TDesc Description => description;

        public abstract Viewport Viewport { get; }

        UnorderedAccessViewDescription IUnorderedAccessView.Description => ((IUnorderedAccessView)Texture).Description;

        RenderTargetViewDescription IRenderTargetView.Description => ((IRenderTargetView)Texture).Description;

        ShaderResourceViewDescription IShaderResourceView.Description => ((IShaderResourceView)Texture).Description;

        string? IUnorderedAccessView.DebugName { get => ((IUnorderedAccessView)Texture).DebugName; set => ((IUnorderedAccessView)Texture).DebugName = value; }

        string? IRenderTargetView.DebugName { get => ((IRenderTargetView)Texture).DebugName; set => ((IRenderTargetView)Texture).DebugName = value; }

        string? IShaderResourceView.DebugName { get => ((IShaderResourceView)Texture).DebugName; set => ((IShaderResourceView)Texture).DebugName = value; }

        string? IDeviceChild.DebugName { get => ((IDeviceChild)Texture).DebugName; set => ((IDeviceChild)Texture).DebugName = value; }

        bool IDeviceChild.IsDisposed => Texture.IsDisposed;

        nint IUnorderedAccessView.NativePointer => ((IUnorderedAccessView)Texture).NativePointer;

        nint IRenderTargetView.NativePointer => ((IRenderTargetView)Texture).NativePointer;

        nint IShaderResourceView.NativePointer => ((IShaderResourceView)Texture).NativePointer;

        nint INative.NativePointer => ((INative)Texture).NativePointer;

        event EventHandler? IDeviceChild.OnDisposed
        {
            add => Texture.OnDisposed += value;
            remove => Texture.OnDisposed -= value;
        }
    }
}