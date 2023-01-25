/*namespace HexaEngine.Rendering
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    public class MeshBaker
    {
        public struct Vertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector3 Tangent;
            public Vector3 Binormal;
        }

        private const uint NumSHTargets = 3;

        public const float NearClip = 0.01f;
        public const float FarClip = 100.0f;
        public const float FOV = MathF.PI / 2;
        public const uint NumBounceSumThreads = 512;

        private const uint RTSize = 64;
        private const uint NumFaces = 5;
        private const uint NumBounces = 1;
        private const uint NumIterations = NumBounces + 1;

        private Texture initialRadiance;
        private UavBuffer reductionBuffer;
        private DepthStencil dsBuffer;

        private IComputePipeline integrateCS;
        private IComputePipeline reductionCS;
        private IComputePipeline sumBouncesCS;

        private unsafe struct IntegrateConstants
        {
            public Matrix4x4 ToTangentSpace0;
            public Matrix4x4 ToTangentSpace1;
            public Matrix4x4 ToTangentSpace2;
            public Matrix4x4 ToTangentSpace3;
            public Matrix4x4 ToTangentSpace4;
            public float FinalWeight;
            public uint VertexIndex;
            public uint NumElements;
            public float Reserved;
        };

        private ConstantBuffer<IntegrateConstants> integrateConstants;

        private CameraTransform camera;

        private const uint NumTimeDeltaSamples = 64;

        private float[] timeDeltaBuffer = new float[NumTimeDeltaSamples];
        private uint currentTimeDeltaSample;

        private readonly List<UavBuffer> currentMeshBuffers = new();
        private readonly List<UavBuffer>[] summedMeshBuffers = { new(), new() };
        private readonly List<IShaderResourceView> meshBakeData = new();

        public MeshBaker()
        {
            camera = new();
            camera.Width = 1;
            camera.Height = 1;
            camera.Fov = FOV;
            camera.Near = NearClip;
            camera.Far = FarClip;
            currentTimeDeltaSample = 0;
            for (uint i = 0; i < NumTimeDeltaSamples; ++i)
                timeDeltaBuffer[i] = 0;
        }

        private static unsafe Matrix4x4 CameraMatrixForVertex(uint face, Vertex vertex)
        {
            Matrix4x4 cameraWorld = default;

            Vector4 x = new(vertex.Tangent, 0.0f);
            Vector4 y = new(vertex.Binormal, 0.0f);
            Vector4 z = new(vertex.Normal, 0.0f);

            Matrix4x4* p = &cameraWorld;
            Vector4* r = (Vector4*)p;
            if (face == 0)
            {
                // +Z
                r[0] = y;
                r[1] = y;
                r[2] = z;
            }
            else if (face == 1)
            {
                // +X
                r[0] = -z;
                r[1] = y;
                r[2] = x;
            }
            else if (face == 2)
            {
                // -X
                r[0] = z;
                r[1] = y;
                r[2] = -x;
            }
            else if (face == 3)
            {
                // +Y
                r[0] = x;
                r[1] = -z;
                r[2] = y;
            }
            else if (face == 4)
            {
                // -Y
                r[0] = x;
                r[1] = z;
                r[2] = -y;
            }

            Vector4 position = new(vertex.Position, 1);
            r[3] = position;

            return cameraWorld;
        }

        // Calculates a scissor rectangle for the given cube face and render target size,
        // so that we only render a hemicube and not a full cube
        private static Rect GetFaceScissor(uint face, uint rtSize)
        {
            Rect rect;

            if (face == 0)
            {
                // +Z
                rect.Left = 0;
                rect.Right = rtSize;
                rect.Top = 0;
                rect.Bottom = rtSize;
            }
            else if (face == 1)
            {
                // +X
                rect.Left = 0;
                rect.Right = rtSize / 2;
                rect.Top = 0;
                rect.Bottom = rtSize;
            }
            else if (face == 2)
            {
                // -X
                rect.Left = rtSize / 2;
                rect.Right = rtSize;
                rect.Top = 0;
                rect.Bottom = rtSize;
            }
            else if (face == 3)
            {
                // +Y
                rect.Left = 0;
                rect.Right = rtSize;
                rect.Top = rtSize / 2;
                rect.Bottom = rtSize;
            }
            else if (face == 4)
            {
                // -Y
                rect.Left = 0;
                rect.Right = rtSize;
                rect.Top = 0;
                rect.Bottom = rtSize / 2;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(face));
            }

            return rect;
        }

        private unsafe void Initialize(IGraphicsDevice device)
        {
            initialRadiance = new(device, TextureDescription.CreateTexture2DWithRTV((int)RTSize, (int)RTSize, 1, Format.RGBA16Float));
            reductionBuffer = new(device, 16, RTSize * NumSHTargets * NumFaces, Format.RGBA32Float, false, false);
            dsBuffer = new(device, (int)RTSize, (int)RTSize, Format.Depth24UNormStencil8);

            integrateConstants = new(device, CpuAccessFlags.Write);

            // Load the integration shaders
            string facesString = NumFaces + "0";

            string rtSizeString = RTSize.ToString();

            string numThreadsString = NumBounceSumThreads.ToString();

            ShaderMacro[] defines = new ShaderMacro[3];
            defines[0].Name = "NumFaces_";
            defines[0].Definition = facesString;
            defines[1].Name = "RTSize_";
            defines[1].Definition = rtSizeString;
            defines[2].Name = "NumBounceSumThreads_";
            defines[2].Definition = numThreadsString;

            integrateCS = device.CreateComputePipeline(new() { Entry = "IntegrateCS", Path = "IntegrateCS.hlsl" });
            reductionCS = device.CreateComputePipeline(new() { Entry = "ReductionCS", Path = "IntegrateCS.hlsl" });
            sumBouncesCS = device.CreateComputePipeline(new() { Entry = "SumBouncesCS", Path = "IntegrateCS.hlsl" });

            // Compute the final weight for integration
            float weightSum = 0.0f;
            for (uint y = 0; y < RTSize; ++y)
            {
                for (uint x = 0; x < RTSize; ++x)
                {
                    float u = (float)x / RTSize * 2.0f - 1.0f;
                    float v = (float)y / RTSize * 2.0f - 1.0f;

                    float temp = 1.0f + u * u + v * v;
                    float weight = 4.0f / (MathF.Sqrt(temp) * temp);

                    weightSum += weight;
                }
            }

            weightSum *= 6.0f;
            integrateConstants.Local->FinalWeight = (4.0f * 3.14159f) / weightSum;
        }

        // Bakes an entire model
        private void Bake(
            Window& window,
            ID3D11Device* device,
            ID3D11DeviceContext* context,
            Model model,
            Skybox& skybox,
            XMFLOAT3& sunDirection,
            XMMATRIX& world,
            MeshRenderer& meshRenderer)
        {
            for (uint meshIdx = 0; meshIdx < model.Meshes().size(); ++meshIdx)
            {
                // Create an input layout for each mesh
                Mesh mesh = model.Meshes()[meshIdx];

                // Create bake data for each mesh
                UavBuffer meshData, summedMeshData, summedMeshData2;
                meshData.Initialize(device, DXGI_FORMAT_R32G32B32A32_FLOAT, 16, mesh.NumVertices() * NumSHTargets);
                summedMeshData.Initialize(device, DXGI_FORMAT_R32G32B32A32_FLOAT, 16, mesh.NumVertices() * NumSHTargets);
                summedMeshData2.Initialize(device, DXGI_FORMAT_R32G32B32A32_FLOAT, 16, mesh.NumVertices() * NumSHTargets);

                currentMeshBuffers.Add(meshData);
                summedMeshBuffers[0].Add(summedMeshData);
                summedMeshBuffers[1].Add(summedMeshData2);
            }

            // Iterate through the bounces
            for (uint bounce = 0; bounce < NumIterations; ++bounce)
            {
                //PIXEvent bounceEvent((L"Bounce " + ToString(bounce)).c_str());

                // Bake one mesh at a time
                for (uint meshIdx = 0; meshIdx < model.Meshes().size(); ++meshIdx)
                {
                    Mesh & mesh = model.Meshes()[meshIdx];

                    BakeMesh(deviceManager, window, device, context, model, skybox, sunDirection,
                                        world, mesh, meshIdx, bounce, meshRenderer);
                }

                if (bounce == 0)
                {
                    // We don't need to sum bounces, so just copy to the output buffers
                    for (uint meshIdx = 0; meshIdx < model.Meshes().size(); ++meshIdx)
                    {
                        context->CopyResource(summedMeshBuffers[0][meshIdx].Buffer, currentMeshBuffers[meshIdx].Buffer);
                        meshBakeData.Add(summedMeshBuffers[0][meshIdx].SRView);
                    }
                }
                else
                {
                    // Sum together this bounce with the previous bounces
                    SumBounces(context, bounce);

                    meshBakeData.Clear();
                    for (uint meshIdx = 0; meshIdx < model.Meshes().size(); ++meshIdx)
                        meshBakeData.Add(summedMeshBuffers[bounce % 2][meshIdx].SRView);
                }
            }
        }
    }
}*/