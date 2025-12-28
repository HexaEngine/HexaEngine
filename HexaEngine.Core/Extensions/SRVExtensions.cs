using Hexa.NET.ImGui;
using HexaEngine.Core.Graphics;

namespace HexaEngine.Core.Extensions
{
    public unsafe static class SRVExtensions
    {
        public static ImTextureRef ToTexRef(this IShaderResourceView? srv)
        {
            if (srv == null)
            {
                return new ImTextureRef(texId: new(0));
            }
            return new ImTextureRef(texId: new(srv.NativePointer));
        }
    }
}