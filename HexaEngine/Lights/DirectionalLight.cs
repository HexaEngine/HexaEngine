namespace HexaEngine.Lights
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Rendering;
    using HexaEngine.Shaders;
    using HexaEngine.Shaders.Light;
    using System;
    using System.Numerics;

    [EditorNode("Directional Light")]
    public class DirectionalLight : Light, IView
    {
        public new CameraTransform Transform = new();
        public IBuffer CB;
        private ConstantBuffer<DirectionalLightDesc> lightBuffer;
        private ConstantBuffer<CamDescription> camBuffer;
        private DirectionalLightShader lightShader;
        private Texture depthMap;

        public DirectionalLight()
        {
            base.Transform = Transform;
        }

        [EditorProperty("Depth view")]
        public Texture DepthMap => depthMap;

        public override LightType Type => LightType.Directional;

        CameraTransform IView.Transform => Transform;

        public override void Initialize(IGraphicsDevice device)
        {
            CB = device.CreateBuffer(new CBCamera(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            Transform.ProjectionType = ProjectionType.Othro;
            Transform.Width = 1024 / 8;
            Transform.Height = 1024 / 8;
            lightShader = new(device);
            if (CastShadows)
            {
                depthMap = new(device,
                    TextureDescription.CreateTexture2DWithRTV(1024 * 8, 1024 * 8, 1, Format.R32Float),
                    DepthStencilDesc.Default);
            }
            lightBuffer = new(device, new ShaderBinding(ShaderStage.Pixel, 0));
            camBuffer = new(device, ShaderStage.Pixel, 1);
            base.Initialize(device);
        }

        public override void Uninitialize()
        {
            CB.Dispose();
            depthMap?.Dispose();
            lightShader.Dispose();
            camBuffer.Dispose();
            lightBuffer.Dispose();
            base.Uninitialize();
        }

        public void Bind(IGraphicsContext context)
        {
            context.Write(CB, new CBCamera(Transform));
            context.SetConstantBuffer(CB, ShaderStage.Domain, 1);
        }

        public override void Render(IGraphicsContext context, Viewport viewport, IView view, IView scene, int indexCount)
        {
            camBuffer.Write(context, new CamDescription(scene.Transform));
            lightBuffer.Write(context, (DirectionalLightDesc)this);
            camBuffer.Bind(context);
            lightBuffer.Bind(context);
            lightShader.DrawIndexed(context, viewport, scene, Matrix4x4.Identity, indexCount, 0, 0);
        }

        public override void ClearDepth(IGraphicsContext context)
        {
            depthMap.ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil);
        }

        public override void BindDepth(IGraphicsContext context, int startSlot)
        {
            depthMap.Bind(context, startSlot);
        }

        public override void RenderDepth(IGraphicsContext context, Action<IGraphicsContext, Viewport, IView> callback)
        {
            depthMap.SetTarget(context);
            callback(context, depthMap.Viewport, this);
        }

        public static implicit operator DirectionalLightDesc(DirectionalLight light)
        {
            return new DirectionalLightDesc(light.color, light.Transform.Forward, light.Transform.View, light.Transform.Projection);
        }
    }

    /// <summary>
    /// Used for shader constant buffers.
    /// </summary>
    public struct DirectionalLightDesc
    {
        public Vector4 Color;
        public Vector3 LightDirection;
        public float reserved;
        public Matrix4x4 View;
        public Matrix4x4 Projection;

        public DirectionalLightDesc(Vector4 color, Vector3 lightDirection, Matrix4x4 view, Matrix4x4 projection)
        {
            Color = color;
            LightDirection = lightDirection;
            reserved = 0;
            View = Matrix4x4.Transpose(view);
            Projection = Matrix4x4.Transpose(projection);
        }
    }
}