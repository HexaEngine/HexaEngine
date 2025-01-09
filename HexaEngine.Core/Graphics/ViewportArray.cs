using Hexa.NET.Mathematics;
using System.Runtime.CompilerServices;

namespace HexaEngine.Core.Graphics
{
    [InlineArray(BlendDescription.SimultaneousRenderTargetCount)]
    public struct ViewportArray
    {
        public Viewport Viewport;
    }
}