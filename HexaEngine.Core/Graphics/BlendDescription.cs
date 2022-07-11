namespace HexaEngine.Core.Graphics
{
    public struct BlendDescription
    {
        public static readonly BlendDescription Opaque = new(Blend.One, Blend.Zero);

        public static readonly BlendDescription AlphaBlend = new(Blend.One, Blend.InverseSourceAlpha);

        public static readonly BlendDescription Additive = new(Blend.SourceAlpha, Blend.One);

        public static readonly BlendDescription NonPremultiplied = new(Blend.SourceAlpha, Blend.InverseSourceAlpha);

        public const int SimultaneousRenderTargetCount = unchecked((int)8);
        public bool AlphaToCoverageEnable = false;
        public bool IndependentBlendEnable = false;
        public RenderTargetBlendDescription[] RenderTarget = new RenderTargetBlendDescription[SimultaneousRenderTargetCount];

        public BlendDescription()
        {
        }

        public BlendDescription(Blend sourceBlend, Blend destinationBlend) : this(sourceBlend, destinationBlend, sourceBlend, destinationBlend)
        {
        }

        public BlendDescription(Blend sourceBlend, Blend destinationBlend, Blend srcBlendAlpha, Blend destBlendAlpha)
            : this()
        {
            AlphaToCoverageEnable = false;
            IndependentBlendEnable = false;

            for (int i = 0; i < SimultaneousRenderTargetCount; i++)
            {
                RenderTarget[i].SourceBlend = sourceBlend;
                RenderTarget[i].DestinationBlend = destinationBlend;
                RenderTarget[i].BlendOperation = BlendOperation.Add;
                RenderTarget[i].SourceBlendAlpha = srcBlendAlpha;
                RenderTarget[i].DestinationBlendAlpha = destBlendAlpha;
                RenderTarget[i].BlendOperationAlpha = BlendOperation.Add;
                RenderTarget[i].RenderTargetWriteMask = ColorWriteEnable.All;
                RenderTarget[i].IsBlendEnabled = IsBlendEnabled(ref RenderTarget[i]);
            }
        }

        private static bool IsBlendEnabled(ref RenderTargetBlendDescription renderTarget)
        {
            return renderTarget.BlendOperationAlpha != BlendOperation.Add
                    || renderTarget.SourceBlendAlpha != Blend.One
                    || renderTarget.DestinationBlendAlpha != Blend.Zero
                    || renderTarget.BlendOperation != BlendOperation.Add
                    || renderTarget.SourceBlend != Blend.One
                    || renderTarget.DestinationBlend != Blend.Zero;
        }
    }
}