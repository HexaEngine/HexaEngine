using System.Runtime.InteropServices;

namespace HexaEngine.Core.Graphics
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ClearValue
    {
        [FieldOffset(0)]
        public ClearColorValue ColorValue;

        [FieldOffset(0)]
        public ClearDepthStencilValue DepthStencilValue;
    }
}