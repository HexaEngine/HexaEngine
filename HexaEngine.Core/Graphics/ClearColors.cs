using System.Runtime.CompilerServices;

namespace HexaEngine.Core.Graphics
{
    [InlineArray(BlendDescription.SimultaneousRenderTargetCount)]
    public struct ClearColors
    {
        public ClearColorValue Color;
    }
}