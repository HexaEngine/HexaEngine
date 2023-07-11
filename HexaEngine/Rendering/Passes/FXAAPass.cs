namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Rendering.Graph;

    public class FXAAPass : RenderPassOld
    {
        public FXAAPass() : base("FXAA")
        {
        }
    }

    public class TAAPass : RenderPassOld
    {
        public TAAPass() : base("TAA")
        {
        }
    }

    public class VelocityBufferPass : RenderPassOld
    {
        public VelocityBufferPass() : base("VelocityBuffer")
        {
        }
    }

    public class ComposePass : RenderPassOld
    {
        public ComposePass() : base("Compose")
        {
        }
    }

    public class AutoExposurePass : RenderPassOld
    {
        public AutoExposurePass() : base("AutoExposure")
        {
        }
    }

    public class BloomPass : RenderPassOld
    {
        public BloomPass() : base("Bloom")
        {
        }
    }

    public class DepthOfFieldPass : RenderPassOld
    {
        public DepthOfFieldPass() : base("DepthOfField")
        {
        }
    }

    public class GodRaysPass : RenderPassOld
    {
        public GodRaysPass() : base("GodRays")
        {
        }
    }

    public class LensFlarePass : RenderPassOld
    {
        public LensFlarePass() : base("LensFlare")
        {
        }
    }

    public class MotionBlurPass : RenderPassOld
    {
        public MotionBlurPass() : base("MotionBlur")
        {
        }
    }

    public class SSRPass : RenderPassOld
    {
        public SSRPass() : base("SSR")
        {
        }
    }

    public class VolumetricCloudsPass : RenderPassOld
    {
        public VolumetricCloudsPass() : base("VolumetricClouds")
        {
        }
    }

    public class VolumetricLightsPass : RenderPassOld
    {
        public VolumetricLightsPass() : base("VolumetricLights")
        {
        }
    }
}