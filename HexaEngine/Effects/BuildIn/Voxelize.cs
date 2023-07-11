namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Structs;
    using HexaEngine.Lights.Types;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Scenes;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using System.Threading.Tasks;

    internal class Voxelize
    {
        private IGraphicsPipeline voxelize;

        public bool VoxelGI = false;
        public bool VoxelDebug = false;
        public bool VoxelSecondBounce = false;
        public float VoxelSize = 4;
        public float VoxelCenterX = 0;
        public float VoxelCenterY = 0;
        public float VoxelCenterZ = 0;
        public uint VoxelNumCones = 2;
        public float VoxelRayStepDistance = 0.75f;
        public float VoxelMaxDistance = 20.0f;
        public uint VoxelMips = 7;
        public bool RecreateInitialSpectrum = true;
        public bool OceanColorChanged = false;

        public const uint VoxelResolution = 128;
        public const uint VoxelizeMaxLights = 8;

        private StructuredUavBuffer<VoxelType> voxels;
        private ConstantBuffer<CBVoxel> voxelConstantBuffer;
        private StructuredBuffer<LightData> lights;

        public struct VoxelType
        {
            public uint ColorMask;
            public uint NormalMask;
        }

        public async Task Initialize(IGraphicsDevice device)
        {
            voxelize = await device.CreateGraphicsPipelineAsync(new()
            {
                PixelShader = "gi/VoxelizePS.hlsl",
                GeometryShader = "gi/VoxelizeGS.hlsl",
                VertexShader = "gi/VoxelizeVS.hlsl",
            }, new GraphicsPipelineState()
            {
                Rasterizer = RasterizerDescription.CullNone
            });
            voxelConstantBuffer = new(device, CpuAccessFlags.Write);
            lights = new(device, (int)VoxelizeMaxLights, CpuAccessFlags.Write);
            voxels = new(device, VoxelResolution * VoxelResolution * VoxelResolution, false, false);
        }

        public void Update(IGraphicsContext context, Camera camera)
        {
            var lightView = LightManager.Current;

            if (lightView == null)
                return;

            float f = 0.05f / VoxelSize;

            Vector3 cam_pos = camera.Transform.GlobalPosition;
            Vector3 center = new(MathF.Floor(cam_pos.X * f) / f, MathF.Floor(cam_pos.Y * f) / f, MathF.Floor(cam_pos.Z * f) / f);

            if ((new Vector3(VoxelCenterX, VoxelCenterY, VoxelCenterZ) - center).LengthSquared() > 0.0001f)
            {
                VoxelCenterX = center.X;
                VoxelCenterY = center.Y;
                VoxelCenterZ = center.Z;
            }

            CBVoxel data;
            data.DataRes = VoxelResolution;
            data.DataResRCP = 1.0f / VoxelResolution;
            data.DataSize = VoxelSize;
            data.DataSizeRCP = 1.0f / VoxelSize;
            data.Mips = VoxelMips;
            data.NumCones = VoxelNumCones;
            data.NumConesRCP = 1.0f / VoxelNumCones;
            data.RayStepSize = VoxelRayStepDistance;
            data.MaxDistance = VoxelMaxDistance;
            data.GridCenter = center;

            voxelConstantBuffer.Update(context, data);

            lights.ResetCounter();

            for (int i = 0; i < lightView.ActiveCount; i++)
            {
                var light = lightView.Active[i];
                LightData lightData = default;
                lightData.Color = light.Color * light.Intensity;
                lightData.Position = new(light.Position, 1);
                lightData.Direction = new(light.Direction, 1);
                lightData.Range = light.Range;
                lightData.Type = (uint)light.LightType;
                if (light is Spotlight spotlight)
                {
                    lightData.OuterCosine = MathF.Cos((spotlight.ConeAngle / 2).ToRad());
                    lightData.InnerCosine = MathF.Cos((MathUtil.Lerp(0, spotlight.ConeAngle, 1 - spotlight.Blend) / 2).ToRad());
                }

                lightData.CastsShadows = light.ShadowMapEnable ? 1 : 0;
                lightData.CascadedShadows = light is DirectionalLight directional && directional.ShadowMapEnable ? 1 : 0;
                lightData.padd = 0;
                lights.Add(lightData);
                if (lights.Count == VoxelizeMaxLights)
                {
                    break;
                }
            }

            lights.Update(context);
        }

        public void Draw(IGraphicsContext context)
        {
            context.SetRenderTargetsAndUnorderedAccessViews(null, null, 0, voxels.UAV);
            context.PSSetShaderResource(1, lights.SRV);
            context.SetGraphicsPipeline(voxelize);
            context.SetViewport(new(VoxelResolution));
        }
    }
}