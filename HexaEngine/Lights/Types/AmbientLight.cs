namespace HexaEngine.Lights.Types
{
    using HexaEngine.Editor.Attributes;

    [EditorCategory("Lights")]
    [EditorGameObject<AmbientLight>("Ambient Light")]
    public class AmbientLight : LightSource
    {
        public override LightType LightType => LightType.Ambient;
    }
}