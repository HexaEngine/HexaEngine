namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Rendering.Graph;

    public class FXAAPass : RenderPass
    {
        public FXAAPass() : base("FXAA")
        {
        }
    }

    public class TAAPass : RenderPass
    {
        public TAAPass() : base("TAA")
        {
        }
    }

    public class VelocityBufferPass : RenderPass
    {
        public VelocityBufferPass() : base("VelocityBuffer")
        {
        }
    }

    public class ComposePass : RenderPass
    {
        public ComposePass() : base("Compose")
        {
        }
    }

    public class AutoExposurePass : RenderPass
    {
        public AutoExposurePass() : base("AutoExposure")
        {
        }
    }

    public class BloomPass : RenderPass
    {
        public BloomPass() : base("Bloom")
        {
        }
    }

    public class DepthOfFieldPass : RenderPass
    {
        public DepthOfFieldPass() : base("DepthOfField")
        {
        }
    }

    public class GodRaysPass : RenderPass
    {
        public GodRaysPass() : base("GodRays")
        {
        }
    }

    public class LensFlarePass : RenderPass
    {
        public LensFlarePass() : base("LensFlare")
        {
        }
    }

    public class MotionBlurPass : RenderPass
    {
        public MotionBlurPass() : base("MotionBlur")
        {
        }
    }

    public class SSRPass : RenderPass
    {
        public SSRPass() : base("SSR")
        {
        }
    }

    public class VolumetricCloudsPass : RenderPass
    {
        public VolumetricCloudsPass() : base("VolumetricClouds")
        {
        }
    }

    public class VolumetricLightsPass : RenderPass
    {
        public VolumetricLightsPass() : base("VolumetricLights")
        {
        }
    }
}