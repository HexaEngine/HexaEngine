namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Threading;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Filters;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Resources;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System;
    using System.Diagnostics;
    using System.Numerics;

    public struct MeshBakerStatistics
    {
        public bool Running;
        public uint CurrentBounce;
        public uint BounceCount;
        public uint CurrentMesh;
        public uint MeshCount;
        public uint CurrentVertex;
        public uint VertexCount;
        public uint VerticesPerSecond;
    }

    public unsafe class MeshBaker
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

        private Texture2D initialRadiance;
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

        private Camera camera;
        private ConstantBuffer<CBCamera> cameraBuffer;

        private const uint NumTimeDeltaSamples = 64;

        private float[] timeDeltaBuffer = new float[NumTimeDeltaSamples];
        private uint currentTimeDeltaSample;

        private readonly List<UavBuffer> currentMeshBuffers = new();
        private readonly List<UavBuffer>[] summedMeshBuffers = { new(), new() };
        private readonly List<IShaderResourceView> meshBakeData = new();

        private static MeshBakerStatistics statistics;

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
            {
                timeDeltaBuffer[i] = 0;
            }
        }

        public static MeshBakerStatistics Statistics => statistics;

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

        public unsafe void Initialize(IGraphicsDevice device)
        {
            initialRadiance = new(device, Format.R16G16B16A16Float, (int)RTSize, (int)RTSize, (int)NumFaces, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            initialRadiance.CreateArraySlices(device);
            reductionBuffer = new(device, 16, RTSize * NumSHTargets * NumFaces, Format.R32G32B32A32Float, false, false);
            dsBuffer = new(device, Format.D32Float, (int)RTSize, (int)RTSize);

            cameraBuffer = new(device, CpuAccessFlags.Write);

            integrateConstants = new(device, CpuAccessFlags.Write);

            // Load the integration shaders

            ShaderMacro[] defines =
            [
                new("NUM_FACES", NumFaces),
                new("RT_SIZE", RTSize),
                new("NUM_BOUNCE_SUM_THREADS", NumBounceSumThreads)
            ];

            integrateCS = device.CreateComputePipeline(new()
            {
                Path = "compute/bake/integrate.hlsl",
                Macros = defines
            });
            reductionCS = device.CreateComputePipeline(new()
            {
                Path = "compute/bake/reduction.hlsl",
                Macros = defines
            });
            sumBouncesCS = device.CreateComputePipeline(new()
            {
                Path = "compute/bake/sumBounces.hlsl",
                Macros = defines
            });

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
            integrateConstants.Local->FinalWeight = 4.0f * 3.14159f / weightSum;
        }

        // Bakes an entire model
        public void Bake(IGraphicsDevice device, IGraphicsContext context, Model model, ISceneRenderer sceneRenderer, MeshRendererComponent meshRenderer, SkyRendererComponent skyRenderer)
        {
            statistics.Running = true;
            for (uint meshIdx = 0; meshIdx < model.Meshes.Length; ++meshIdx)
            {
                // Create an input layout for each mesh
                Mesh mesh = model.Meshes[meshIdx];

                // Create bake data for each mesh
                UavBuffer meshData, summedMeshData, summedMeshData2;
                meshData = new(device, 16, mesh.IndexCount * NumSHTargets, Format.R32G32B32A32Float, true, true);
                summedMeshData = new(device, 16, mesh.IndexCount * NumSHTargets, Format.R32G32B32A32Float, true, true);
                summedMeshData2 = new(device, 16, mesh.IndexCount * NumSHTargets, Format.R32G32B32A32Float, true, true);

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

                    BakeMesh(context, model, mesh, meshIdx, bounce, sceneRenderer, meshRenderer, skyRenderer);
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
                    {
                        meshBakeData.Add(summedMeshBuffers[bounce % 2][(int)meshIdx].SRV);
                    }
                }
            }
            statistics.Running = false;
        }

        public Task BakeAsync(IGraphicsDevice device, IGraphicsContext context, ISceneRenderer sceneRenderer, Model model, MeshRendererComponent meshRenderer, SkyRendererComponent skyRenderer, CancellationToken? cancellationToken = null)
        {
            return Task.Run(() =>
            {
                statistics.Running = true;
                var dispatcher = Application.MainWindow.Dispatcher;
                dispatcher.TimeBudget = TimeSpan.FromMilliseconds(16);
                for (uint meshIdx = 0; meshIdx < model.Meshes.Length; ++meshIdx)
                {
                    // Create an input layout for each mesh
                    Mesh mesh = model.Meshes[meshIdx];

                    // Create bake data for each mesh
                    UavBuffer meshData, summedMeshData, summedMeshData2;
                    meshData = new(device, 16, mesh.IndexCount * NumSHTargets, Format.R32G32B32A32Float, true, true);
                    summedMeshData = new(device, 16, mesh.IndexCount * NumSHTargets, Format.R32G32B32A32Float, true, true);
                    summedMeshData2 = new(device, 16, mesh.IndexCount * NumSHTargets, Format.R32G32B32A32Float, true, true);

                    currentMeshBuffers.Add(meshData);
                    summedMeshBuffers[0].Add(summedMeshData);
                    summedMeshBuffers[1].Add(summedMeshData2);
                }

                // Bake one mesh at a time
                for (uint meshIdx = 0; meshIdx < model.Meshes.Length; ++meshIdx)
                {
                    Mesh mesh = model.Meshes[meshIdx];

                    // Iterate through the bounces
                    for (uint bounce = 0; bounce < NumIterations; ++bounce)
                    {
                        //PIXEvent bounceEvent((L"Bounce " + ToString(bounce)).c_str());

                        cancellationToken?.ThrowIfCancellationRequested();
                        BakeMeshAsync(dispatcher, context, model, mesh, meshIdx, bounce, sceneRenderer, meshRenderer, skyRenderer, cancellationToken);

                        if (bounce == 0)
                        {
                            cancellationToken?.ThrowIfCancellationRequested();
                            // We don't need to sum bounces, so just copy to the output buffers

                            dispatcher.InvokeBlocking(() =>
                            {
                                context.CopyResource(summedMeshBuffers[0][(int)meshIdx].Buffer, currentMeshBuffers[(int)meshIdx].Buffer);
                            });

                            meshBakeData.Add(summedMeshBuffers[0][(int)meshIdx].SRV);
                        }
                        else
                        {
                            cancellationToken?.ThrowIfCancellationRequested();

                            // Sum together this bounce with the previous bounces
                            SumBouncesAsync(dispatcher, context, bounce);

                            meshBakeData.Add(summedMeshBuffers[bounce % 2][(int)meshIdx].SRV);
                        }
                    }
                }

                statistics.Running = false;
            });
        }

        // Bakes all vertices of a single mesh
        private void BakeMesh(IGraphicsContext context, Model model, Mesh mesh, uint meshIdx, uint bounce, ISceneRenderer sceneRenderer, MeshRendererComponent meshRenderer, SkyRendererComponent skyRenderer)
        {
            // FindByName the position, normal, tangent, and binormal elements
            int posOffset = 0xFFFFFFF;
            int nmlOffset = 0xFFFFFFF;
            int tangentOffset = 0xFFFFFFF;
            int binormalOffset = 0xFFFFFFF;
            for (uint i = 0; i < mesh.InputElements.Length; ++i)
            {
                InputElementDescription element = mesh.InputElements[i];
                string semantic = element.SemanticName;
                if (semantic == "POSITION")
                {
                    posOffset = element.AlignedByteOffset;
                }
                else if (semantic == "NORMAL")
                {
                    nmlOffset = element.AlignedByteOffset;
                }
                else if (semantic == "TANGENT")
                {
                    tangentOffset = element.AlignedByteOffset;
                }
                else if (semantic == "BINORMAL")
                {
                    binormalOffset = element.AlignedByteOffset;
                }
            }

            if (posOffset == 0xFFFFFFF)
            {
                throw new Exception("Can't bake a mesh with no positions!");
            }

            if (nmlOffset == 0xFFFFFFF)
            {
                throw new Exception("Can't bake a mesh with no normals!");
            }

            if (tangentOffset == 0xFFFFFFF || binormalOffset == 0xFFFFFFF)
            {
                throw new Exception("Can't bake a mesh with no tangent frame!");
            }

            // Pull out the vertex data from the mesh
            uint numVerts = mesh.VertexCount;
            uint vtxStride = mesh.Stride;
            Vertex[] verts = new Vertex[numVerts];

            int vertexIndex = 0;
            for (uint i = 0; i < numVerts; ++i)
            {
                Vector3 position = mesh.Data.Positions[i];
                Vector3 normal = mesh.Data.Normals[i];
                Vector3 tangent = mesh.Data.Tangents[i];
                Vector3 binormal = mesh.Data.Bitangents[i];

                // Transform the positions + tangent frame to world space
                Vector3 positionWS = Vector3.Transform(position, meshRenderer.Transform);
                Vector3 normalWS = Vector3.TransformNormal(normal, meshRenderer.Transform);
                normalWS = Vector3.Normalize(normalWS);

                Vector3 tangentWS = Vector3.TransformNormal(tangent, meshRenderer.Transform);
                tangentWS = Vector3.Normalize(tangentWS);

                Vector3 binormalWS = Vector3.TransformNormal(binormal, meshRenderer.Transform);
                binormalWS = Vector3.Normalize(binormalWS);

                Vertex vertex;
                vertex.Position = positionWS;
                vertex.Normal = normalWS;
                vertex.Tangent = tangentWS;
                vertex.Binormal = binormalWS;
                verts[vertexIndex++] = vertex;
            }

            UavBuffer meshData = currentMeshBuffers[(int)meshIdx];

            long timestamp = Stopwatch.GetTimestamp() + 1 * Stopwatch.Frequency;

            // Bake each vertex
            for (uint vertIdx = 0; vertIdx < numVerts; ++vertIdx)
            {
                BakeVertex(context, verts[vertIdx], vertIdx, meshData, bounce, sceneRenderer, meshRenderer, skyRenderer);

                long now = Stopwatch.GetTimestamp();
                if (now >= timestamp)
                {
                    timestamp = now + 1 * Stopwatch.Frequency;

                    var oldVertex = statistics.CurrentVertex;

                    statistics.CurrentBounce = bounce + 1;
                    statistics.BounceCount = NumIterations;
                    statistics.CurrentMesh = meshIdx + 1;
                    statistics.MeshCount = (uint)model.Meshes.Length;
                    statistics.CurrentVertex = vertIdx + 1;
                    statistics.VertexCount = numVerts;
                    statistics.VerticesPerSecond = statistics.CurrentVertex - oldVertex;
                }
            }
        }

        // Bakes all vertices of a single mesh
        private void BakeMeshAsync(ThreadDispatcher dispatcher, IGraphicsContext context, Model model, Mesh mesh, uint meshIdx, uint bounce, ISceneRenderer sceneRenderer, MeshRendererComponent meshRenderer, SkyRendererComponent skyRenderer, CancellationToken? cancellationToken)
        {
            // FindByName the position, normal, tangent, and binormal elements
            int posOffset = 0xFFFFFFF;
            int nmlOffset = 0xFFFFFFF;
            int tangentOffset = 0xFFFFFFF;
            int binormalOffset = 0xFFFFFFF;
            for (uint i = 0; i < mesh.InputElements.Length; ++i)
            {
                InputElementDescription element = mesh.InputElements[i];
                string semantic = element.SemanticName;
                if (semantic == "POSITION")
                {
                    posOffset = element.AlignedByteOffset;
                }
                else if (semantic == "NORMAL")
                {
                    nmlOffset = element.AlignedByteOffset;
                }
                else if (semantic == "TANGENT")
                {
                    tangentOffset = element.AlignedByteOffset;
                }
                else if (semantic == "BINORMAL")
                {
                    binormalOffset = element.AlignedByteOffset;
                }
            }

            if (posOffset == 0xFFFFFFF)
            {
                Logger.Error("Can't bake a mesh with no positions!");
                return;
            }

            if (nmlOffset == 0xFFFFFFF)
            {
                Logger.Error("Can't bake a mesh with no normals!");
                return;
            }

            if (tangentOffset == 0xFFFFFFF || binormalOffset == 0xFFFFFFF)
            {
                Logger.Error("Can't bake a mesh with no tangent frame!");
                return;
            }

            // Pull out the vertex data from the mesh
            uint numVerts = mesh.VertexCount;
            uint vtxStride = mesh.Stride;
            Vertex[] verts = new Vertex[numVerts];

            int vertexIndex = 0;
            for (uint i = 0; i < numVerts; ++i)
            {
                Vector3 position = mesh.Data.Positions[i];
                Vector3 normal = mesh.Data.Normals[i];
                Vector3 tangent = mesh.Data.Tangents[i];
                Vector3 binormal = mesh.Data.Bitangents[i];

                // Transform the positions + tangent frame to world space
                Vector3 positionWS = Vector3.Transform(position, meshRenderer.Transform);
                Vector3 normalWS = Vector3.TransformNormal(normal, meshRenderer.Transform);
                normalWS = Vector3.Normalize(normalWS);

                Vector3 tangentWS = Vector3.TransformNormal(tangent, meshRenderer.Transform);
                tangentWS = Vector3.Normalize(tangentWS);

                Vector3 binormalWS = Vector3.TransformNormal(binormal, meshRenderer.Transform);
                binormalWS = Vector3.Normalize(binormalWS);

                Vertex vertex;
                vertex.Position = positionWS;
                vertex.Normal = normalWS;
                vertex.Tangent = tangentWS;
                vertex.Binormal = binormalWS;
                verts[vertexIndex++] = vertex;
            }

            UavBuffer meshData = currentMeshBuffers[(int)meshIdx];

            WaitHandle?[] tasks = new WaitHandle[numVerts];
            // Bake each vertex
            for (uint vertIdx = 0; vertIdx < numVerts; vertIdx++)
            {
                tasks[vertIdx] = dispatcher.InvokeWaitHandle(state =>
                {
                    var localVertIdx = (uint)state;

                    if (cancellationToken?.IsCancellationRequested ?? false)
                        return;

                    BakeVertex(context, verts[localVertIdx], localVertIdx, meshData, bounce, sceneRenderer, meshRenderer, skyRenderer);
                    var oldVertex = statistics.CurrentVertex;
                    statistics.CurrentBounce = bounce + 1;
                    statistics.BounceCount = NumIterations;
                    statistics.CurrentMesh = meshIdx + 1;
                    statistics.MeshCount = (uint)model.Meshes.Length;
                    statistics.CurrentVertex = localVertIdx + 1;
                    statistics.VertexCount = numVerts;
                }, vertIdx);
            }

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i]?.WaitOne();
                tasks[i]?.Dispose();
            }

            cancellationToken?.ThrowIfCancellationRequested();
        }

        // Bakes a single vertex by rendering the entire scene 5 times to generate the radiance hemicube,
        // and then integrate it down to a single set of H-basis coefficients
        private void BakeVertex(IGraphicsContext context, Vertex vertex, uint vertIdx, UavBuffer meshData, uint bounce, ISceneRenderer sceneRenderer, MeshRendererComponent meshRenderer, SkyRendererComponent skyRenderer)
        {
            Matrix4x4 worldToTangent = Matrix4x4.Identity;
            worldToTangent[0, 0] = vertex.Tangent.X;
            worldToTangent[0, 1] = vertex.Tangent.Y;
            worldToTangent[0, 2] = vertex.Tangent.Z;
            worldToTangent[1, 0] = vertex.Binormal.X;
            worldToTangent[1, 1] = vertex.Binormal.Y;
            worldToTangent[1, 2] = vertex.Binormal.Z;
            worldToTangent[2, 0] = vertex.Normal.X;
            worldToTangent[2, 1] = vertex.Normal.Y;
            worldToTangent[2, 2] = vertex.Normal.Z;

            worldToTangent = Matrix4x4.Transpose(worldToTangent);

            {
                // Set the viewport + RS State
                Viewport viewport = new(RTSize);

                context.SetViewport(viewport);

                // doesn't make sense ?!?!
                //context->RSSetState(rasterizerStates.NoCullScissor());

                // Render the scene in 5 directions
                for (uint face = 0; face < NumFaces; ++face)
                {
                    // Set the render target and clear it
                    IRenderTargetView renderTarget = initialRadiance.RTVArraySlices[face];

                    context.SetRenderTarget(renderTarget, dsBuffer.DSV);

                    context.ClearRenderTargetView(renderTarget, default);

                    context.ClearDepthStencilView(dsBuffer.DSV, DepthStencilClearFlags.All, 1.0f, 0);

                    // Set the scissor rectangle
                    Rect scissorRect;

                    scissorRect = GetFaceScissor(face, RTSize);

                    context.SetScissorRect((int)scissorRect.Left, (int)scissorRect.Top, (int)scissorRect.Right, (int)scissorRect.Bottom);

                    // Set up the camera for rendering from the POV of the vertex
                    Matrix4x4 cameraWorld = CameraMatrixForVertex(face, vertex);

                    camera.Transform.Local = cameraWorld;

                    meshRenderer.Update(context);
                    cameraBuffer.Update(context, new(camera, new(RTSize)));

                    // Render the mesh
                    if (bounce == 0)
                    {
                        meshRenderer.DrawDepth(context, cameraBuffer);
                    }
                    else if (bounce == 1)
                    {
                        sceneRenderer.Render(context, viewport, SceneManager.Current, camera);
                    }
                    else
                    {
                        // TODO: ObjectAdded way to suppress shadow mapping and directional lights
                        sceneRenderer.Render(context, viewport, SceneManager.Current, camera);
                    }

                    if (bounce == 0)
                    {
                        // Draw the skybox
                        camera.Near = 0.1f;

                        /*
                         * Also doesn't make sense let it decide the renderer
                        Vector4 blendFactor = new(1, 1, 1, 1);

                        context.OMSetBlendState(blendStates.BlendDisabled(), blendFactor, 0xFFFFFFFF);

                        context.OMSetDepthStencilState(depthStencilStates.DepthEnabled(), 0);
                        */

                        skyRenderer.Draw(context, RenderPath.Forward);

                        camera.Near = NearClip;
                    }
                }

                // Clear out the VS SRV's
                nint* vsSRViews = stackalloc nint[(int)NumSHTargets];

                context.VSSetShaderResources(0, NumSHTargets, (void**)vsSRViews);

                // Clear out the render target
                context.SetRenderTarget(null, null);
            }

            Integrate(context, vertex, vertIdx, meshData, worldToTangent);
        }

        // Computes a single set of H-basis coefficients representing the irradiance for a single
        // vertex, by integrating radiance about the hemisphere around the normal
        private void Integrate(IGraphicsContext context, Vertex vertex, uint vertIdx, UavBuffer meshData, Matrix4x4 worldToTangent)
        {
            // Set shaders
            context.SetComputePipeline(integrateCS);

            // Set shader resources
            context.CSSetShaderResource(0, initialRadiance.SRV);

            // Set the output textures
            context.CSSetUnorderedAccessView(0, (void*)reductionBuffer.UAV.NativePointer);

            // Set constants
            for (uint i = 0; i < NumFaces; ++i)
            {
                Matrix4x4 viewToWorld = CameraMatrixForVertex(i, vertex);
                viewToWorld[2, 0] = 0;
                viewToWorld[2, 1] = 0;
                viewToWorld[2, 2] = 0;
                Matrix4x4 viewToTangent = Matrix4x4.Multiply(viewToWorld, worldToTangent);
                ((Matrix4x4*)integrateConstants.Local)[i] = Matrix4x4.Transpose(viewToTangent);
            }

            integrateConstants.Local->VertexIndex = vertIdx;

            integrateConstants.Update(context);
            context.CSSetConstantBuffer(0, integrateConstants);

            // Do the initial integration + reduction
            context.Dispatch(1, RTSize, NumFaces);

            // Set the shader
            context.SetComputePipeline(reductionCS);

            // Set outputs
            context.CSSetUnorderedAccessView(0, (void*)meshData.UAV.NativePointer);

            // Set shader resources
            context.CSSetShaderResource(0, reductionBuffer.SRV);

            // Do the final reduction
            context.Dispatch(1, NumSHTargets, 1);

            // Clear out the SRV's and RT's
            context.CSSetShaderResource(0, null);
            context.CSSetUnorderedAccessView(0, null);
        }

        // Sums the results from two bounce passes
        private void SumBounces(IGraphicsContext context, uint bounce)
        {
            // Set shaders
            context.SetComputePipeline(sumBouncesCS);

            nint* inputBuffers = stackalloc nint[2];

            for (uint meshIdx = 0; meshIdx < meshBakeData.Count; ++meshIdx)
            {
                // Set input buffers
                inputBuffers[0] = meshBakeData[(int)meshIdx].NativePointer;
                inputBuffers[1] = currentMeshBuffers[(int)meshIdx].SRV.NativePointer;
                context.CSSetShaderResources(0, 2, (void**)inputBuffers);

                // Set output buffers
                context.CSSetUnorderedAccessView(0, (void*)summedMeshBuffers[bounce % 2][(int)meshIdx].UAV.NativePointer);

                // Set constants
                uint numElements = currentMeshBuffers[(int)meshIdx].Length;
                integrateConstants.Local->NumElements = numElements;
                integrateConstants.Update(context);
                context.CSSetConstantBuffer(0, integrateConstants);

                // Launch groups of 512 threads, with enough groups to sum all elements in the buffers
                uint numGroups = numElements / NumBounceSumThreads;
                if (numElements % NumBounceSumThreads > 0)
                {
                    ++numGroups;
                }

                context.Dispatch(numGroups, 1, 1);
            }

            // Clear out the inputs and outputs
            inputBuffers[0] = 0;
            inputBuffers[1] = 0;
            context.CSSetShaderResources(0, 2, (void**)inputBuffers);
            context.CSSetUnorderedAccessView(0, null);
        }

        // Sums the results from two bounce passes
        private void SumBouncesAsync(ThreadDispatcher dispatcher, IGraphicsContext context, uint bounce)
        {
            dispatcher.InvokeBlocking(() =>
            {
                SumBounces(context, bounce);
            });
        }

        // Retrieve the buffer containing H-basis coefficients for all verts in a mesh
        private IShaderResourceView GetBakedMeshData(uint idx)
        {
            return meshBakeData[(int)idx];
        }
    }

    public unsafe class MeshBaker2
    {
        private const int RTWidth = 2048;
        public readonly Texture2D Texture;
        public readonly Texture2D TextureFinal;
        private readonly Camera[] cameras = new Camera[6];

        private IBLDiffuseIrradianceCompute irradiance;

        public MeshBaker2(IGraphicsDevice device)
        {
            Texture = new(device, Format.R16G16B16A16UNorm, RTWidth, RTWidth, 6, 1, CpuAccessFlags.None, GpuAccessFlags.All, ResourceMiscFlag.TextureCube);
            Texture.CreateArraySlices(device);

            TextureFinal = new(device, Format.R16G16B16A16UNorm, 256, 256, 6, 1, CpuAccessFlags.None, GpuAccessFlags.All, ResourceMiscFlag.TextureCube);
            TextureFinal.CreateArraySlices(device);

            irradiance = new(device);

            irradiance.Target = TextureFinal;
            irradiance.Source = Texture;
        }

        public void Bake(IThreadDispatcher dispatcher, IGraphicsContext context, Transform objectTransform, Model model, ISceneRenderer sceneRenderer)
        {
            var global = objectTransform.Global;

            for (int i = 0; i < model.Nodes.Length; i++)
            {
                var node = model.Nodes[i];
                var nodeGlobal = node.GetGlobalTransform(global);

                for (int j = 0; j < node.Meshes.Count; j++)
                {
                    var mesh = model.Meshes[j];

                    BakeMesh(dispatcher, context, nodeGlobal, sceneRenderer);
                }
            }
        }

        private void BakeMesh(IThreadDispatcher dispatcher, IGraphicsContext context, Matrix4x4 nodeGlobal, ISceneRenderer sceneRenderer)
        {
            dispatcher.InvokeBlocking(() =>
            {
                var oldSize = sceneRenderer.Size;
                sceneRenderer.Size = Texture.Viewport.Size;
                sceneRenderer.DrawFlags = SceneDrawFlags.NoPostProcessing | SceneDrawFlags.NoOverlay;
                context.ClearRenderTargetView(Texture.RTV, default);
                var position = nodeGlobal.Translation;
                SetViewPoint(position);

                for (int i = 0; i < 6; i++)
                {
                    CameraManager.Current = cameras[i];
                    context.BeginEvent($"Render face {i}");
                    sceneRenderer.RenderTo(context, Texture.RTVArraySlices[i], Texture.Viewport, SceneManager.Current, cameras[i]);
                    context.EndEvent();
                }
                CameraManager.Current = null;

                irradiance.Dispatch(context, 256, 256);

                sceneRenderer.DrawFlags = SceneDrawFlags.None;
                sceneRenderer.Size = oldSize;
            });
        }

        public void SetViewPoint(Vector3 position)
        {
            // The TextureCube Texture2D assumes the
            // following order of faces.

            // The LookAt targets for view matrices
            var targets = new[] {
                  Vector3.UnitX, // +X
                  -Vector3.UnitX, // -X
                  Vector3.UnitY, // +Y
                 - Vector3.UnitY, // -Y
                   Vector3.UnitZ, // +Z
                - Vector3.UnitZ  // -Z
            };

            var upVectors = new[] {
                Vector3.UnitY, // +X
                Vector3.UnitY, // -X
                -Vector3.UnitZ,// +Y
                Vector3.UnitZ,// -Y
                Vector3.UnitY, // +Z
                Vector3.UnitY, // -Z
            };

            for (int i = 0; i < 6; i++)
            {
                var cam = cameras[i] ?? new();
                cameras[i] = cam;
                cam.Fov = 90f.ToRad();
                cam.Width = RTWidth;
                cam.Height = RTWidth;
                cam.Far = 1000f;
                cam.Near = 0.001f;
                cam.Transform.Position = position;

                Matrix4x4.Decompose(MathUtil.LookAtLH(Vector3.Zero, targets[i], upVectors[i]), out _, out var rot, out _);
                cam.Transform.Orientation = rot;
                cam.Transform.Recalculate();
            }
        }
    }
}