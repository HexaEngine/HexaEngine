namespace HexaEngine.Core.Graphics
{
    public struct RenderTargetBlendDescription
    {
        public bool IsBlendEnabled;
        public Blend SourceBlend;
        public Blend DestinationBlend;
        public BlendOperation BlendOperation;
        public Blend SourceBlendAlpha;
        public Blend DestinationBlendAlpha;
        public BlendOperation BlendOperationAlpha;
        public ColorWriteEnable RenderTargetWriteMask;
    }
}