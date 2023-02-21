namespace HexaEngine.Core.Graphics
{
    using System.ComponentModel;

    public struct RenderTargetBlendDescription
    {
        [DefaultValue(false)]
        public bool IsBlendEnabled;

        [DefaultValue(Blend.One)]
        public Blend SourceBlend;

        [DefaultValue(Blend.Zero)]
        public Blend DestinationBlend;

        [DefaultValue(BlendOperation.Add)]
        public BlendOperation BlendOperation;

        [DefaultValue(Blend.One)]
        public Blend SourceBlendAlpha;

        [DefaultValue(Blend.Zero)]
        public Blend DestinationBlendAlpha;

        [DefaultValue(BlendOperation.Add)]
        public BlendOperation BlendOperationAlpha;

        [DefaultValue(ColorWriteEnable.All)]
        public ColorWriteEnable RenderTargetWriteMask;
    }
}