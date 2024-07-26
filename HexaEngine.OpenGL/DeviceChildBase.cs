namespace HexaEngine.OpenGL
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.OpenGL;

    public abstract unsafe class DeviceChildBase : DisposableBase, IDeviceChild
    {
        protected uint nativePointer;

        protected abstract ObjectIdentifier Identifier { get; }

        public string? DebugName
        {
            get
            {
                uint len;
                OpenGLGraphicsDevice.GL.GetObjectLabel(Identifier, nativePointer, 0, &len, null);
                byte* pName = (byte*)Alloc((nuint)len);
                OpenGLGraphicsDevice.GL.GetObjectLabel(Identifier, nativePointer, 0, &len, null);
                string str = ToStr(pName, len);
                Free(pName);
                return str;
            }
            set
            {
                if (value != null)
                {
                    byte* pName = value.ToUTF8Ptr();
                    OpenGLGraphicsDevice.GL.ObjectLabel(Identifier, nativePointer, (uint)value.Length, pName);
                }
                else
                {
                    OpenGLGraphicsDevice.GL.ObjectLabel(Identifier, nativePointer, 0, (byte*)null);
                }
            }
        }

        public nint NativePointer => (nint)nativePointer;
    }
}