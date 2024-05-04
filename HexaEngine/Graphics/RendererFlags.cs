namespace HexaEngine.Graphics
{
    [Flags]
    public enum RendererFlags
    {
        None = 0,
        Update = 1,
        Depth = 2,
        Draw = 4,
        Deferred = 8,
        Forward = 16,
        Culling = 32,
        CastShadows = 64,
        NoDepthTest = 128,
        Clustered = 256,
        Bake = 512,
        Dynamic = 1024,
        All = Update | Depth | Draw | Culling | CastShadows,
    }
}