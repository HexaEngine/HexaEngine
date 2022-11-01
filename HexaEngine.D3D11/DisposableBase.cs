namespace HexaEngine.D3D11
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;
    using System;
    using System.Text;

    public abstract unsafe class DeviceChildBase : DisposableBase, IDeviceChild
    {
        protected IntPtr nativePointer;

        public static readonly Guid D3DDebugObjectName = new(0x429b8c22, 0x9188, 0x4b0c, 0x87, 0x42, 0xac, 0xb0, 0xbf, 0x85, 0xc2, 0x00);

        public virtual string? DebugName
        {
            get
            {
                ID3D11DeviceChild* child = (ID3D11DeviceChild*)nativePointer;
                uint len;
                child->GetPrivateData(Utils.Guid(D3DDebugObjectName), &len, null);
                byte[] buffer = new byte[len];
                child->GetPrivateData(Utils.Guid(D3DDebugObjectName), &len, Utils.AsPointer(buffer));
                return Encoding.UTF8.GetString(buffer);
            }
            set
            {
                ID3D11DeviceChild* child = (ID3D11DeviceChild*)nativePointer;
                if (value != null)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(value);
                    child->SetPrivateData(Utils.Guid(D3DDebugObjectName), (uint)buffer.Length, Utils.AsPointer(buffer));
                }
                else
                {
                    child->SetPrivateData(Utils.Guid(D3DDebugObjectName), 0, null);
                }
            }
        }

        public IntPtr NativePointer => nativePointer;
    }

    public abstract class DisposableBase : IDisposable
    {
        private bool disposedValue;

        public DisposableBase()
        {
            LeakTracer.Allocate(this);
        }

        public bool IsDisposed => disposedValue;

        public event EventHandler? OnDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                DisposeCore();
                OnDisposed?.Invoke(this, EventArgs.Empty);
                LeakTracer.Release(this);
                disposedValue = true;
            }
        }

        protected abstract void DisposeCore();

        ~DisposableBase()
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
}