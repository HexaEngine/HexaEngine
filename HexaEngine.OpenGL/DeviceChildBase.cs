namespace HexaEngine.OpenGL
{
    using HexaEngine.Core.Graphics;
    using Hexa.NET.OpenGL;

    public abstract unsafe class DeviceChildBase : DisposableBase, IDeviceChild
    {
        protected uint nativePointer;

        protected abstract GLObjectIdentifier Identifier { get; }

        public string? DebugName
        {
            get
            {
                int len;
                OpenGLGraphicsDevice.GL.GetObjectLabel(Identifier, nativePointer, 0, &len, (byte*)null);
                byte* pName = (byte*)Alloc((nuint)len);
                OpenGLGraphicsDevice.GL.GetObjectLabel(Identifier, nativePointer, 0, &len, (byte*)null);
                string str = ToStr(pName, len);
                Free(pName);
                return str;
            }
            set
            {
                if (value != null)
                {
                    byte* pName = value.ToUTF8Ptr();
                    OpenGLGraphicsDevice.GL.ObjectLabel(Identifier, nativePointer, value.Length, pName);
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