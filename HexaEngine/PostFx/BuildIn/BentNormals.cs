namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Graph;
    using System;

    public class BentNormals : PostFxBase
    {
        public override string Name => "BentNormals";

        public override PostFxFlags Flags => PostFxFlags.NoOutput | PostFxFlags.NoInput | PostFxFlags.Optional;

        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            throw new NotImplementedException();
        }

        public override void Initialize(IGraphicsDevice device, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            throw new NotImplementedException();
        }

        public override void Draw(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        protected override void DisposeCore()
        {
            throw new NotImplementedException();
        }
    }
}