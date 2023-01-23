/*
namespace HexaEngine.Rendering
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Mathematics;
    using Silk.NET.Direct3D11;
    using System;
    using System.Drawing;
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

        public const float NearClip = 0.01f;
        public const float FarClip = 100.0f;
        public const float FOV = MathF.PI / 2;
        public const uint NumBounceSumThreads = 512;

        private const uint RTSize = 64;
        private const uint NumFaces = 5;
        private const uint NumBounces = 1;
        private const uint NumIterations = NumBounces + 1;

        private Texture initialRadiance;
        private StructuredUavBuffer reductionBuffer;
        private readonly DepthBuffer dsBuffer;

        public MeshBaker()
        {
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

        private void MeshBaker::Initialize(ID3D11Device* device)
        {
            initialRadiance.Initialize(device, RTSize, RTSize, DXGI_FORMAT_R16G16B16A16_FLOAT, 1, 1, 0, false, false, NumFaces);
            reductionBuffer.Initialize(device, DXGI_FORMAT_R32G32B32A32_FLOAT, 16, RTSize * NumSHTargets * NumFaces);
            dsBuffer.Initialize(device, RTSize, RTSize, DXGI_FORMAT_D24_UNORM_S8_UINT);

            integrateConstants.Initialize(device);

            blendStates.Initialize(device);
            rasterizerStates.Initialize(device);
            depthStencilStates.Initialize(device);
            samplerStates.Initialize(device);

            // Load the integration shaders
            char facesString[2] = { 0 };
            facesString[0] = static_cast<char>(NumFaces + '0');

            std::wstring rtSizeString = ToString(RTSize);
            char rtSizeStringChar[16] = { 0 };
            for (size_t i = 0; i < rtSizeString.size(); ++i)
                rtSizeStringChar[i] = static_cast<char>(rtSizeString[i]);

            std::wstring numThreadsString = ToString(NumBounceSumThreads);
            char numThreadsStringChar[16] = { 0 };
            for (size_t i = 0; i < numThreadsString.size(); ++i)
                numThreadsStringChar[i] = static_cast<char>(numThreadsString[i]);

            D3D10_SHADER_MACRO defines[4];
            defines[0].Name = "NumFaces_";
            defines[0].Definition = facesString;
            defines[1].Name = "RTSize_";
            defines[1].Definition = rtSizeStringChar;
            defines[2].Name = "NumBounceSumThreads_";
            defines[2].Definition = numThreadsStringChar;
            defines[3].Name = NULL;
            defines[3].Definition = NULL;

            integrateCS.Attach(CompileCSFromFile(device, L"IntegrateCS.hlsl", "IntegrateCS", "cs_5_0", defines));
            reductionCS.Attach(CompileCSFromFile(device, L"IntegrateCS.hlsl", "ReductionCS", "cs_5_0", defines));
            sumBouncesCS.Attach(CompileCSFromFile(device, L"IntegrateCS.hlsl", "SumBouncesCS", "cs_5_0", defines));

            // Compute the final weight for integration
            float weightSum = 0.0f;
            for (UINT y = 0; y < RTSize; ++y)
            {
                for (UINT x = 0; x < RTSize; ++x)
                {
                    const float u = (float(x) / float(RTSize)) * 2.0f - 1.0f;
                    const float v = (float(y) / float(RTSize)) * 2.0f - 1.0f;

                    const float temp = 1.0f + u * u + v * v;
                    const float weight = 4.0f / (sqrtf(temp) * temp);

                    weightSum += weight;
                }
            }

            weightSum *= 6.0f;
            integrateConstants.Data.FinalWeight = (4.0f * 3.14159f) / weightSum;

            font.Initialize(L"Arial", 18, SpriteFont::Regular, true, device);
            spriteRenderer.Initialize(device);
        }
    }
}*/