namespace HexaEngine.Core.Lights
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    [EditorNode<DirectionalLight>("Directional Light")]
    public class DirectionalLight : Light
    {
        public LightIndex Index;
        public new CameraTransform Transform = new();

        public DirectionalLight()
        {
            base.Transform = Transform;
            Transform.Updated += (s, e) => { Updated = true; };
        }

        public override LightType LightType => LightType.Directional;

        public unsafe ShadowDirectionalLightData* InsertLightData(Camera camera, StructuredUavBuffer<DirectionalLightData> l, StructuredUavBuffer<ShadowDirectionalLightData> sl)
        {
            if (CastShadows)
            {
                l.Add(new(this));
                return null;
            }
            else
            {
                var directionalLightData = (ShadowDirectionalLightData*)Unsafe.AsPointer(ref sl.Add(new(this)));
                Matrix4x4* views = directionalLightData->GetViews();
                float* cascades = directionalLightData->GetCascades();
                var mtxs = CSMHelper.GetLightSpaceMatrices(camera.Transform, Transform, views, cascades);
                return directionalLightData;
            }
        }

        public override void Initialize(IGraphicsDevice device)
        {
            base.Initialize(device);
        }

        public override void Uninitialize()
        {
            base.Uninitialize();
        }
    }
}