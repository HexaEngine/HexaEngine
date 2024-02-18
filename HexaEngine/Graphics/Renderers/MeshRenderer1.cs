namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Structs;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Lights;
    using System.Numerics;

    public class MeshRenderer1 : BaseRenderer<MeshInstance>
    {
        private DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> drawIndirectArgs;
        private StructuredBuffer<Matrix4x4> transformNoBuffer;
        private StructuredBuffer<uint> transformNoOffsetBuffer;
        private StructuredUavBuffer<Matrix4x4> transformBuffer;
        private StructuredUavBuffer<uint> transformOffsetBuffer;

        public MeshRenderer1(IGraphicsDevice device)
        {
        }

        public override void Update(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public override void VisibilityTest(CullingContext context)
        {
            throw new NotImplementedException();
        }

        public override void DrawDepth(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public override void DrawForward(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public override void DrawDeferred(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public override void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            throw new NotImplementedException();
        }

        protected override void DisposeCore()
        {
            throw new NotImplementedException();
        }
    }
}