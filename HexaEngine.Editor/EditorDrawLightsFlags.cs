namespace HexaEngine.Editor
{
    [Flags]
    public enum EditorDrawLightsFlags
    {
        None = 0,
        DrawLights = 1,
        NoDirectionalLights = 2,
        NoPointLights = 4,
        NoSpotLights = 8,
    }
}