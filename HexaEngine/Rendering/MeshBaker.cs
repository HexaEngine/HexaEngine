/*
namespace HexaEngine.Rendering
{
    using BepuUtilities;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Rendering;
    using HexaEngine.Mathematics;
    using HexaEngine.Pipelines.Forward;
    using HexaEngine.Windows;
    using ImGuiNET;
    using Silk.NET.Direct3D11;
    using Silk.NET.DXGI;
    using Silk.NET.OpenAL;
    using Silk.NET.SDL;
    using System;
    using System.Numerics;
    using System.Xml.Linq;

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
        private void Bake(IGraphicsDevice device, IGraphicsContext context, Model model, Skybox skybox, Vector3 sunDirection, Matrix4x4 world, MeshRenderer meshRenderer)
        {
            for (uint meshIdx = 0; meshIdx < model.Meshes.Length; ++meshIdx)
            {
                // Create an input layout for each mesh
                Mesh mesh = model.Meshes[meshIdx];

                // Create bake data for each mesh
                UavBuffer meshData, summedMeshData, summedMeshData2;
                meshData = new(device, 16, mesh.IndexCount * NumSHTargets, Format.RGBA32Float, true, true);
                summedMeshData = new(device, 16, mesh.IndexCount * NumSHTargets, Format.RGBA32Float, true, true);
                summedMeshData2 = new(device, 16, mesh.IndexCount * NumSHTargets, Format.RGBA32Float, true, true);

                currentMeshBuffers.Add(meshData);
                summedMeshBuffers[0].Add(summedMeshData);
                summedMeshBuffers[1].Add(summedMeshData2);
            }

            // Iterate through the bounces
            for (uint bounce = 0; bounce < NumIterations; ++bounce)
            {
                //PIXEvent bounceEvent((L"Bounce " + ToString(bounce)).c_str());

                // Bake one mesh at a time
                for (uint meshIdx = 0; meshIdx < model.Meshes.Length; ++meshIdx)
                {
                    Mesh mesh = model.Meshes[meshIdx];

                    BakeMesh(window, device, context, model, skybox, sunDirection, world, mesh, meshIdx, bounce, meshRenderer);
                }

                if (bounce == 0)
                {
                    // We don't need to sum bounces, so just copy to the output buffers
                    for (uint meshIdx = 0; meshIdx < model.Meshes.Length; ++meshIdx)
                    {
                        context.CopyResource(summedMeshBuffers[0][(int)meshIdx].Buffer, currentMeshBuffers[(int)meshIdx].Buffer);
                        meshBakeData.Add(summedMeshBuffers[0][(int)meshIdx].SRV);
                    }
                }
                else
                {
                    // Sum together this bounce with the previous bounces
                    SumBounces(context, bounce);

                    meshBakeData.Clear();
                    for (uint meshIdx = 0; meshIdx < model.Meshes.Length; ++meshIdx)
                        meshBakeData.Add(summedMeshBuffers[bounce % 2][(int)meshIdx].SRV);
                }
            }
        }

        // Bakes all vertices of a single mesh
        private void BakeMesh(IGraphicsDevice device, IGraphicsContext context, Model model, Skybox skybox, Vector3 sunDirection, Matrix4x4 world, Mesh mesh, uint meshIdx, uint bounce, MeshRenderer meshRenderer)
        {
            // Find the position, normal, tangent, and binormal elements
            uint posOffset = 0xFFFFFFFF;
            uint nmlOffset = 0xFFFFFFFF;
            uint tangentOffset = 0xFFFFFFFF;
            uint binormalOffset = 0xFFFFFFFF;
            for (uint i = 0; i < mesh.NumInputElements(); ++i)
            {
                D3D11_INPUT_ELEMENT_DESC & element = mesh.InputElements()[i];
                std::string semantic(element.SemanticName);
                if (semantic == "POSITION")
                    posOffset = element.AlignedByteOffset;
                else if (semantic == "NORMAL")
                    nmlOffset = element.AlignedByteOffset;
                else if (semantic == "TANGENT")
                    tangentOffset = element.AlignedByteOffset;
                else if (semantic == "BINORMAL")
                    binormalOffset = element.AlignedByteOffset;
            }

            if (posOffset == 0xFFFFFFFF)
                throw Exception(L"Can't bake a mesh with no positions!");

            if (nmlOffset == 0xFFFFFFFF)
                throw Exception(L"Can't bake a mesh with no normals!");

            if (tangentOffset == 0xFFFFFFFF || binormalOffset == 0xFFFFFFFF)
                throw Exception(L"Can't bake a mesh with no tangent frame!");

            // Pull out the vertex data from the mesh
            uint numVerts = mesh.NumVertices();
            uint vtxStride = mesh.VertexStride();
            vector<Vertex> verts;
            verts.reserve(numVerts);

            // Create a CPU-readable staging buffer to which we can copy the vertex data
            D3D11_BUFFER_DESC bufferDesc;
            bufferDesc.BindFlags = 0;
            bufferDesc.ByteWidth = numVerts * vtxStride;
            bufferDesc.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
            bufferDesc.MiscFlags = 0;
            bufferDesc.StructureByteStride = 0;
            bufferDesc.Usage = D3D11_USAGE_STAGING;

            ID3D11BufferPtr stagingBuffer;
            DXCall(device->CreateBuffer(&bufferDesc, NULL, &stagingBuffer));

            // Copy the vertex data
            context->CopyResource(stagingBuffer, mesh.VertexBuffer());

            // Map the staging buffer and read its contents
            D3D11_MAPPED_SUBRESOURCE mapped;
            DXCall(context->Map(stagingBuffer, 0, D3D11_MAP_READ, 0, &mapped));
            BYTE* srcPositions = reinterpret_cast<BYTE*>(mapped.pData) + posOffset;
            BYTE* srcNormals = reinterpret_cast<BYTE*>(mapped.pData) + nmlOffset;
            BYTE* srcTangents = reinterpret_cast<BYTE*>(mapped.pData) + tangentOffset;
            BYTE* srcBinormals = reinterpret_cast<BYTE*>(mapped.pData) + binormalOffset;

            for (uint i = 0; i < numVerts; ++i)
            {
                XMVECTOR position = XMLoadFloat3(reinterpret_cast<XMFLOAT3*>(srcPositions));
                XMVECTOR normal = XMLoadFloat3(reinterpret_cast<XMFLOAT3*>(srcNormals));
                XMVECTOR tangent = XMLoadFloat3(reinterpret_cast<XMFLOAT3*>(srcTangents));
                XMVECTOR binormal = XMLoadFloat3(reinterpret_cast<XMFLOAT3*>(srcBinormals));

                // Transform the positions + tangent frame to world space
                XMVECTOR positionWS = XMVector3TransformCoord(position, world);
                XMVECTOR normalWS = XMVector3TransformNormal(normal, world);
                normalWS = XMVector3Normalize(normalWS);

                XMVECTOR tangentWS = XMVector3TransformNormal(tangent, world);
                tangentWS = XMVector3Normalize(tangentWS);

                XMVECTOR binormalWS = XMVector3TransformNormal(binormal, world);
                binormalWS = XMVector3Normalize(binormalWS);

                Vertex vertex;
                XMStoreFloat3(&vertex.Position, positionWS);
                XMStoreFloat3(&vertex.Normal, normalWS);
                XMStoreFloat3(&vertex.Tangent, tangentWS);
                XMStoreFloat3(&vertex.Binormal, binormalWS);
                verts.push_back(vertex);

                srcPositions += vtxStride;
                srcNormals += vtxStride;
                srcTangents += vtxStride;
                srcBinormals += vtxStride;
            }
            context->Unmap(stagingBuffer, 0);

            RWBuffer & meshData = currentMeshBuffers[meshIdx];

            // Bake each vertex
            for (uint vertIdx = 0; vertIdx < numVerts; ++vertIdx)
            {
                BakeVertex(device, context, model, skybox, sunDirection,
                            world, verts[vertIdx], vertIdx, mesh, meshData, bounce, meshRenderer);

                // Calculate the fps
                timer.Update();
                CalculateFPS();

                // Render the FPS + current mesh/vertex info to the backbuffer, and keep pumping
                // the window's message loop. This way we keep updating the progress, and the user
                // can quit the app.
                ID3D11RenderTargetView* renderTargets[1] = { deviceManager.BackBuffer() };
                context->OMSetRenderTargets(1, renderTargets, NULL);
                context->ClearRenderTargetView(renderTargets[0], reinterpret_cast<float*>(&XMFLOAT4(0, 0, 0, 0)));

                D3D11_VIEWPORT vp;
                vp.Width = static_cast<float>(deviceManager.BackBufferWidth());
                vp.Height = static_cast<float>(deviceManager.BackBufferHeight());
                vp.TopLeftX = 0;
                vp.TopLeftY = 0;
                vp.MinDepth = 0;
                vp.MaxDepth = 1;
                context->RSSetViewports(1, &vp);

                spriteRenderer.Begin(context, SpriteRenderer::Point);

                XMMATRIX transform = XMMatrixTranslation(50.0f, 50.0f, 0);
                std::wstring text = L"Iteration " + ToString(bounce + 1) + L" of " + ToString(NumIterations) + L"\n";
                text += L"Baking mesh " + ToString(meshIdx + 1) + L" of " + ToString(model.Meshes().size());
                text += L"\nBaking vertex " + ToString(vertIdx + 1) + L" of " + ToString(numVerts);
                text += L" (" + ToString(fps) + L" verts per second)";

                spriteRenderer.RenderText(font, text.c_str(), transform);

                spriteRenderer.End();

                deviceManager.Present();

                KeyboardState kbState = KeyboardState::GetKeyboardState();
                if (kbState.IsKeyDown(Keys::Escape))
                    window.Destroy();

                window.MessageLoop();

                if (!window.IsAlive())
                    return;
            }
        }
    }
}*/