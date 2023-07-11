namespace HexaEngine.Rendering.Graph
{
    public enum RenderPassPurpose
    {
        // Every-frame render pass.
        Default,

        // One-time render pass to setup common states and/or resources.
        Setup,

        // One-time render pass for asset preprocessing.
        // Asset resources will be specifically transitioned
        // to appropriate states before and after execution of such passes.
        AssetProcessing
    };
}