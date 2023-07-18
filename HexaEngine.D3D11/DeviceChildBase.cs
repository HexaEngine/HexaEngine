namespace HexaEngine.D3D11
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;
    using System;
    using System.Text;

    internal static class DebugNameRegistry
    {
        private static readonly HashSet<string> names = new();

        public static void CheckAndAdd(string name)
        {
#if DEBUG
            if (names.Contains(name))
            {
                throw new Exception("Name is already present");
            }
            names.Add(name);
#endif
        }

        public static void RemoveName(string name)
        {
#if DEBUG
            names.Remove(name);
#endif
        }
    }

    public abstract unsafe class DeviceChildBase : DisposableBase, IDeviceChild
    {
        protected IntPtr nativePointer;
        private string? debugName;

        public static readonly Guid D3DDebugObjectName = new(0x429b8c22, 0x9188, 0x4b0c, 0x87, 0x42, 0xac, 0xb0, 0xbf, 0x85, 0xc2, 0x00);

        public virtual string? DebugName
        {
            get
            {
#if DEBUG
                ID3D11DeviceChild* child = (ID3D11DeviceChild*)nativePointer;
                if (child == null || IsDisposed)
                {
                    return null;
                }

                if (debugName != null)
                {
                    return debugName;
                }

                uint len;
                Guid guid = D3DDebugObjectName;
                child->GetPrivateData(&guid, &len, null);
                byte* pName = Alloc<byte>(len);
                child->GetPrivateData(&guid, &len, pName);
                string str = Utils.ToStr(pName, len);
                Free(pName);
                return str;
#else
                return "DEBUG is disabled cannot get name";
#endif
            }
            set
            {
#if DEBUG
                ID3D11DeviceChild* child = (ID3D11DeviceChild*)nativePointer;
                if (child == null || debugName == value || IsDisposed || debugName != null)
                {
                    return;
                }

                Guid guid = D3DDebugObjectName;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var byteCount = Encoding.UTF8.GetByteCount(value);
                    byte* pName = Alloc<byte>(Encoding.UTF8.GetByteCount(value));
                    fixed (char* src = value)
                    {
                        Encoding.UTF8.GetBytes(src, value.Length, pName, byteCount);
                    }

                    child->SetPrivateData(&guid, (uint)Encoding.UTF8.GetByteCount(value), pName).ThrowHResult();
                    Free(pName);
                }
                else
                {
                    child->SetPrivateData(&guid, 0, null).ThrowHResult();
                }

                if (debugName != null)
                {
                    DebugNameRegistry.RemoveName(debugName);
                }

                debugName = value;
#endif
            }
        }

        public IntPtr NativePointer => nativePointer;
    }
}