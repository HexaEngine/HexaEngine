namespace HexaEngine.Scenes
{
    using Assimp;
    using Assimp.Configs;
    using AssScene = Assimp.Scene;

    public class AssimpSceneLoader
    {
        public static void Load(string path)
        {
            AssimpContext context = new();
            NormalSmoothingAngleConfig config = new(66.0f);
            context.SetConfig(config);
            AssScene scene = context.ImportFile(path,
                PostProcessSteps.Triangulate |
                PostProcessSteps.FlipUVs |
                PostProcessSteps.CalculateTangentSpace |
                PostProcessSteps.MakeLeftHanded);
            for (int i = 0; i < scene.Materials.Count; i++)
            {
                var mat = scene.Materials[i];
            }
        }
    }
}