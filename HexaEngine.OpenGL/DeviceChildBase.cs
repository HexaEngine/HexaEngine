namespace HexaEngine.OpenGL
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.OpenGL;

    public abstract unsafe class DeviceChildBase : DisposableBase, IDeviceChild
    {
        protected GL gl;
        protected uint nativePointer;

        protected abstract ObjectIdentifier Identifier { get; }

        public string? DebugName
        {
            get
            {
                uint len;
                gl.GetObjectLabel(Identifier, nativePointer, 0, &len, null);
                byte* pName = (byte*)Malloc(len);
                gl.GetObjectLabel(Identifier, nativePointer, 0, &len, null);
                string str = Utils.ToStr(pName, len);
                Free(pName);
                return str;
            }
            set
            {
                if (value != null)
                {
                    byte* pName = value.ToUTF8();
                    gl.ObjectLabel(Identifier, nativePointer, (uint)value.Length, pName);
                }
                else
                {
                    gl.ObjectLabel(Identifier, nativePointer, 0, (byte*)null);
                }
            }
        }

        public nint NativePointer => (nint)nativePointer;
    }
}