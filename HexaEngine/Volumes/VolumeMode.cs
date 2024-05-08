namespace HexaEngine.Volumes
{
    public enum VolumeMode
    {
        Global,
        Local,
    }

    public enum VolumeShape
    {
        Box,
        Sphere
    }

    public enum VolumeTransitionMode
    {
        Constant,
        Linear,
        Smoothstep,
    }
}