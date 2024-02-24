namespace HexaEngine.Components.Renderer
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Jobs;

    public class CubeRendererComponent : PrimitiveRenderComponent
    {
        private Cube? cube;

        protected override void UpdateModel(IGraphicsDevice device)
        {
            Job job = Job.Run("Model Load Job", this, state =>
            {
                if (state is not CubeRendererComponent component)
                {
                    return;
                }

                if (component.GameObject == null)
                {
                    return;
                }

                cube?.Dispose();
                cube = new(device);

                var materialData = MaterialData.GetMaterial(component.materialAsset);
            }, JobPriority.Normal, JobFlags.BlockOnSceneLoad);
        }
    }
}