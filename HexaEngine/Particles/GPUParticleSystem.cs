namespace HexaEngine.Particles
{
    using Hexa.NET.Mathematics;
    using Hexa.NET.Utilities;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Structs;
    using HexaEngine.Weather;
    using Silk.NET.OpenAL;
    using System;
    using System.Numerics;

    //based on AMD GPU Particles Sample: https://github.com/GPUOpen-LibrariesAndSDKs/GPUParticles11

    public class GPUParticleSystem : IDisposable
    {
        private readonly IGraphicsDevice device;
        private readonly ParticleEmitter emitter;
        private readonly Texture2D randomTexture;
        private readonly StructuredUavBuffer<uint> deadListBuffer;
        private readonly StructuredUavBuffer<GPUParticleA> particleBufferA;
        private readonly StructuredUavBuffer<GPUParticleB> particleBufferB;
        private readonly StructuredUavBuffer<ViewSpacePositionRadius> viewSpacePositionsBuffer;
        private readonly StructuredUavBuffer<IndexBufferElement> aliveIndexBuffer;

        private readonly DrawIndirectArgsUavBuffer<DrawIndexedInstancedIndirectArgs> indirectRenderArgsBuffer;
        private readonly DrawIndirectArgsUavBuffer<DrawInstancedIndirectArgs> indirectSortArgsBuffer;

        private readonly ConstantBuffer<UPoint4> deadListCountBuffer;
        private readonly ConstantBuffer<UPoint4> activeListCountBuffer;
        private readonly ConstantBuffer<EmitterCBuffer> emitterBuffer;
        private readonly ConstantBuffer<SortDispatchInfo> sortDispatchInfoBuffer;
        private readonly ConstantBuffer<SimulationCBuffer> simulationBuffer;

        private readonly IndexBuffer<uint> indexBuffer;

        private readonly IComputePipelineState particleInitDeadList;
        private readonly IComputePipelineState particleReset;
        private readonly IComputePipelineState particleEmit;
        private readonly IComputePipelineState particleSimulate;
        private readonly IGraphicsPipelineState particles;
        private readonly IComputePipelineState particleSortInitArgs;
        private readonly IComputePipelineState particleSort512;
        private readonly IComputePipelineState particleBitonicSortStep;
        private readonly IComputePipelineState particleSortInner512;

        private readonly ISamplerState linearClampSampler;
        private readonly ISamplerState linearWrapSampler;

        private bool disposedValue;

        #region Structs

        private const int MaxParticles = 400 * 1024;

        private struct GPUParticleA
        {
            public Vector4 TintAndAlpha;
            public float Rotation;
            public uint IsSleeping;
        };

        private struct GPUParticleB
        {
            public Vector3 Position;
            public float Mass;

            public Vector3 Velocity;
            public float Lifespan;

            public float DistanceToEye;
            public float Age;
            public float StartSize;
            public float EndSize;
        };

        private struct EmitterCBuffer
        {
            public Vector4 EmitterPosition;
            public Vector4 EmitterVelocity;
            public Vector4 PositionVariance;

            public int MaxParticlesThisFrame;
            public float ParticleLifeSpan;
            public float StartSize;
            public float EndSize;

            public float VelocityVariance;
            public float Mass;
            public float ElapsedTime;
            public int Collisions;

            public int CollisionThickness;
            public UPoint3 padd;
        };

        private struct IndexBufferElement
        {
            public float Distance;
            public float Index;
        };

        private struct ViewSpacePositionRadius
        {
            public Vector3 ViewspacePosition;
            public float Radius;
        };

        private struct SortDispatchInfo
        {
            public int X, Y, Z, W;
        };

        private struct SimulationCBuffer
        {
            public float WindDirectionX;
            public float WindDirectionY;
            public float DeltaTime;
            public uint padd;
        }

        #endregion Structs

        public unsafe GPUParticleSystem(IGraphicsDevice device, ParticleEmitter emitter)
        {
            this.device = device;
            this.emitter = emitter;
            deadListBuffer = new(MaxParticles, CpuAccessFlags.None, BufferUnorderedAccessViewFlags.Append);
            particleBufferA = new(MaxParticles, CpuAccessFlags.None);
            particleBufferB = new(MaxParticles, CpuAccessFlags.None);
            viewSpacePositionsBuffer = new(MaxParticles, CpuAccessFlags.None);
            aliveIndexBuffer = new(MaxParticles, CpuAccessFlags.None, BufferUnorderedAccessViewFlags.Counter);

            indirectRenderArgsBuffer = new(1, CpuAccessFlags.None);
            indirectSortArgsBuffer = new(1, CpuAccessFlags.None);

            deadListCountBuffer = new(CpuAccessFlags.None, true);
            activeListCountBuffer = new(CpuAccessFlags.None, true);
            emitterBuffer = new(CpuAccessFlags.Write);
            sortDispatchInfoBuffer = new(CpuAccessFlags.Write);
            simulationBuffer = new(CpuAccessFlags.Write);

            Texture2DDescription desc = default;
            desc.Width = 1024;
            desc.Height = 1024;
            desc.Format = Format.R32G32B32A32Float;
            desc.Usage = Usage.Immutable;
            desc.BindFlags = BindFlags.ShaderResource;
            desc.ArraySize = 1;
            desc.MipLevels = 1;
            desc.SampleDescription = SampleDescription.Default;
            desc.CPUAccessFlags = CpuAccessFlags.None;

            UnsafeList<float> randomTextureData = new();
            for (uint i = 0; i < desc.Width * desc.Height; i++)
            {
                randomTextureData.PushBack(2.0f * Random.Shared.NextSingle() - 1.0f);
                randomTextureData.PushBack(2.0f * Random.Shared.NextSingle() - 1.0f);
                randomTextureData.PushBack(2.0f * Random.Shared.NextSingle() - 1.0f);
                randomTextureData.PushBack(2.0f * Random.Shared.NextSingle() - 1.0f);
            }

            SubresourceData initData = default;
            initData.DataPointer = (nint)randomTextureData.Data;
            initData.RowPitch = desc.Width * 4 * sizeof(float);

            randomTexture = new(desc, initData);
            randomTextureData.Release();

            uint[] indices = new uint[(MaxParticles * 6)];
            uint start = 0;
            uint offset = 0;
            for (int i = 0; i < MaxParticles; i++)
            {
                indices[offset + 0] = start + 0;
                indices[offset + 1] = start + 1;
                indices[offset + 2] = start + 2;

                indices[offset + 3] = start + 2;
                indices[offset + 4] = start + 1;
                indices[offset + 5] = start + 3;

                start += 4;
                offset += 6;
            }

            indexBuffer = new(indices, CpuAccessFlags.None);

            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
            linearWrapSampler = device.CreateSamplerState(SamplerStateDescription.LinearWrap);

            particleInitDeadList = device.CreateComputePipelineState(new ComputePipelineDesc("forward/particles/InitDeadListCS.hlsl"));
            particleInitDeadList.Bindings.SetUAV("deadList", deadListBuffer.UAV, 0);

            particleReset = device.CreateComputePipelineState(new ComputePipelineDesc("forward/particles/ParticleResetCS.hlsl"));
            particleReset.Bindings.SetUAV("ParticleBufferA", particleBufferA.UAV);
            particleReset.Bindings.SetUAV("ParticleBufferB", particleBufferB.UAV);

            particleEmit = device.CreateComputePipelineState(new ComputePipelineDesc("forward/particles/ParticleEmitCS.hlsl"));
            particleEmit.Bindings.SetCBV("DeadListCountCBuffer", deadListCountBuffer);
            particleEmit.Bindings.SetCBV("EmitterCBuffer", emitterBuffer);
            particleEmit.Bindings.SetSRV("RandomBuffer", randomTexture);
            particleEmit.Bindings.SetUAV("ParticleBufferA", particleBufferA.UAV);
            particleEmit.Bindings.SetUAV("ParticleBufferB", particleBufferB.UAV);
            particleEmit.Bindings.SetUAV("DeadListToAllocFrom", deadListBuffer.UAV);
            particleEmit.Bindings.SetSampler("linearWrapSampler", linearWrapSampler);

            particleSimulate = device.CreateComputePipelineState(new ComputePipelineDesc("forward/particles/ParticleSimulateCS.hlsl"));
            particleEmit.Bindings.SetCBV("ComputeCBuffer", simulationBuffer);
            particleEmit.Bindings.SetCBV("EmitterCBuffer", emitterBuffer);
            particleSimulate.Bindings.SetUAV("ParticleBufferA", particleBufferA.UAV);
            particleSimulate.Bindings.SetUAV("ParticleBufferB", particleBufferB.UAV);
            particleSimulate.Bindings.SetUAV("DeadListToAddTo", deadListBuffer.UAV);
            particleSimulate.Bindings.SetUAV("IndexBuffer", aliveIndexBuffer.UAV, 0);
            particleSimulate.Bindings.SetUAV("ViewSpacePositions", viewSpacePositionsBuffer.UAV);
            particleSimulate.Bindings.SetUAV("DrawArgs", indirectRenderArgsBuffer.UAV);

            particleSortInitArgs = device.CreateComputePipelineState(new ComputePipelineDesc("forward/particles/InitSortDispatchArgsCS.hlsl"));
            particleSortInitArgs.Bindings.SetCBV("NumElementsCB", activeListCountBuffer);
            particleSortInitArgs.Bindings.SetUAV("Data", indirectSortArgsBuffer.UAV);

            particleSort512 = device.CreateComputePipelineState(new ComputePipelineDesc("forward/particles/Sort512CS.hlsl"));
            particleSort512.Bindings.SetCBV("NumElementsCB", activeListCountBuffer);
            particleSort512.Bindings.SetUAV("Data", aliveIndexBuffer.UAV);

            particleBitonicSortStep = device.CreateComputePipelineState(new ComputePipelineDesc("forward/particles/BitonicSortStepCS.hlsl"));
            particleBitonicSortStep.Bindings.SetCBV("NumElementsCB", activeListCountBuffer);
            particleBitonicSortStep.Bindings.SetCBV("SortConstants", sortDispatchInfoBuffer);
            particleBitonicSortStep.Bindings.SetUAV("Data", aliveIndexBuffer.UAV);

            particleSortInner512 = device.CreateComputePipelineState(new ComputePipelineDesc("forward/particles/SortInner512CS.hlsl"));
            particleSortInner512.Bindings.SetCBV("NumElementsCB", activeListCountBuffer);
            particleSortInner512.Bindings.SetUAV("Data", aliveIndexBuffer.UAV);

            particles = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "forward/particles/ParticleVS.hlsl",
                PixelShader = "forward/particles/ParticlePS.hlsl",
            }, new()
            {
                Blend = BlendDescription.AlphaBlend,
                BlendFactor = Vector4.One,
                Rasterizer = RasterizerDescription.CullBack,
            });
            particles.Bindings.SetCBV("ActiveListCountCBuffer", activeListCountBuffer);
            particles.Bindings.SetSRV("ParticleBufferA", particleBufferA.SRV);
            particles.Bindings.SetSRV("ViewSpacePositions", viewSpacePositionsBuffer.SRV);
            particles.Bindings.SetSRV("IndexBuffer", aliveIndexBuffer.SRV);
            particles.Bindings.SetSRV("ParticleTexture", emitter.ParticleTexture.SRV);
            particles.Bindings.SetSampler("linearClampSampler", linearClampSampler);
        }

        public unsafe void InitializeDeadList(IGraphicsContext context)
        {
            context.SetComputePipelineState(particleInitDeadList);
            context.Dispatch((uint)Math.Ceiling(MaxParticles * 1.0f / 256), 1, 1);
            context.SetComputePipelineState(null);
        }

        public unsafe void ResetParticles(IGraphicsContext context)
        {
            context.SetComputePipelineState(particleReset);
            context.Dispatch((uint)Math.Ceiling(MaxParticles * 1.0f / 256), 1, 1);
            context.SetComputePipelineState(null);
        }

        public unsafe void Emit(IGraphicsContext context)
        {
            if (emitter.NumberToEmit > 0)
            {
                context.SetComputePipelineState(particleEmit);

                EmitterCBuffer emitterBufferData = default;
                emitterBufferData.ElapsedTime = emitter.ElapsedTime;
                emitterBufferData.EmitterPosition = emitter.Position;
                emitterBufferData.EmitterVelocity = emitter.Velocity;
                emitterBufferData.StartSize = emitter.StartSize;
                emitterBufferData.EndSize = emitter.EndSize;
                emitterBufferData.Mass = emitter.Mass;
                emitterBufferData.MaxParticlesThisFrame = emitter.NumberToEmit;
                emitterBufferData.ParticleLifeSpan = emitter.ParticleLifespan;
                emitterBufferData.PositionVariance = emitter.PositionVariance;
                emitterBufferData.VelocityVariance = emitter.VelocityVariance;
                emitterBufferData.Collisions = emitter.CollisionsEnabled ? 1 : 0;
                emitterBufferData.CollisionThickness = emitter.CollisionThickness;
                emitterBuffer.Update(context, emitterBufferData);

                context.CopyStructureCount(deadListCountBuffer, 0, deadListBuffer.UAV);
                uint thread_groups_x = (uint)Math.Ceiling(emitter.NumberToEmit * 1.0f / 1024);
                context.Dispatch(thread_groups_x, 1, 1);

                context.SetComputePipelineState(null);
            }
        }

        public unsafe void Simulate(IGraphicsContext context, IShaderResourceView depthSRV)
        {
            var wind = WeatherSystem.Current?.WindDirection ?? Vector2.Zero;
            SimulationCBuffer simulationData = default;
            simulationData.WindDirectionX = wind.X;
            simulationData.WindDirectionY = wind.Y;
            simulationData.DeltaTime = Time.Delta;
            simulationBuffer.Update(context, simulationData);

            particleSimulate.Bindings.SetSRV("DepthBuffer", depthSRV);

            context.SetComputePipelineState(particleSimulate);
            context.Dispatch((uint)Math.Ceiling(MaxParticles * 1.0f / 256), 1, 1);
            context.SetComputePipelineState(null);

            context.CopyStructureCount(activeListCountBuffer, 0, aliveIndexBuffer.UAV);
        }

        public unsafe void Update()
        {
            var dt = Time.Delta;
            emitter.ElapsedTime += dt;
            if (emitter.ParticlesPerSecond > 0.0f)
            {
                emitter.Accumulation += emitter.ParticlesPerSecond * dt;
                if (emitter.Accumulation > 1.0f)
                {
                    float integer_part = MathF.Truncate(emitter.Accumulation);
                    float fraction = emitter.Accumulation - integer_part;

                    emitter.NumberToEmit = (int)integer_part;
                    emitter.Accumulation = fraction;
                }
            }
        }

        public void Draw(IGraphicsContext context, IShaderResourceView depthSRV)
        {
            if (emitter.ResetEmitter)
            {
                InitializeDeadList(context);
                ResetParticles(context);
                emitter.ResetEmitter = false;
            }

            Emit(context);
            Simulate(context, depthSRV);

            if (emitter.Sort)
            {
                Sort(context);
            }

            Rasterize(context, depthSRV);
        }

        public unsafe void Rasterize(IGraphicsContext context, IShaderResourceView depthSRV)
        {
            context.SetVertexBuffer(null, 0);
            context.SetIndexBuffer(indexBuffer, Format.R32UInt, 0);

            particles.Bindings.SetSRV("DepthTexture", depthSRV);

            context.SetGraphicsPipelineState(particles);
            context.DrawIndexedInstancedIndirect(indirectRenderArgsBuffer, 0);
            context.SetGraphicsPipelineState(null);
        }

        public unsafe void Sort(IGraphicsContext context)
        {
            // Write the indirect args to a UAV

            context.SetComputePipelineState(particleSortInitArgs);
            context.Dispatch(1, 1, 1);

            bool done = SortInitial(context);
            uint presorted = 512;
            while (!done)
            {
                done = SortIncremental(context, presorted);
                presorted *= 2;
            }
        }

        public bool SortInitial(IGraphicsContext context)
        {
            bool done = true;
            uint numThreadGroups = (MaxParticles - 1 >> 9) + 1;
            if (numThreadGroups > 1)
            {
                done = false;
            }

            context.SetComputePipelineState(particleSort512);
            context.DispatchIndirect(indirectSortArgsBuffer, 0);
            return done;
        }

        public bool SortIncremental(IGraphicsContext context, uint presorted)
        {
            bool done = true;
            context.SetComputePipelineState(particleBitonicSortStep);

            uint num_thread_groups = 0;
            if (MaxParticles > presorted)
            {
                if (MaxParticles > presorted * 2)
                {
                    done = false;
                }

                uint pow2 = presorted;
                while (pow2 < MaxParticles)
                {
                    pow2 *= 2;
                }

                num_thread_groups = pow2 >> 9;
            }

            uint merge_size = presorted * 2;
            for (uint merge_subsize = merge_size >> 1; merge_subsize > 256; merge_subsize >>= 1)
            {
                SortDispatchInfo sort_dispatch_info = default;
                sort_dispatch_info.X = (int)merge_subsize;
                if (merge_subsize == merge_size >> 1)
                {
                    sort_dispatch_info.Y = (int)(2 * merge_subsize - 1);
                    sort_dispatch_info.Z = -1;
                }
                else
                {
                    sort_dispatch_info.Y = (int)merge_subsize;
                    sort_dispatch_info.Z = 1;
                }
                sort_dispatch_info.W = 0;
                sortDispatchInfoBuffer.Update(context, sort_dispatch_info);
                context.Dispatch(num_thread_groups, 1, 1);
            }
            context.SetComputePipelineState(particleSortInner512);
            context.Dispatch(num_thread_groups, 1, 1);

            return done;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                randomTexture.Dispose();

                deadListBuffer.Dispose();
                particleBufferA.Dispose();
                particleBufferB.Dispose();
                viewSpacePositionsBuffer.Dispose();
                aliveIndexBuffer.Dispose();

                indirectRenderArgsBuffer.Dispose();
                indirectSortArgsBuffer.Dispose();

                deadListCountBuffer.Dispose();
                activeListCountBuffer.Dispose();
                emitterBuffer.Dispose();
                sortDispatchInfoBuffer.Dispose();

                indexBuffer.Dispose();

                particleInitDeadList.Dispose();
                particleReset.Dispose();
                particleEmit.Dispose();
                particleSimulate.Dispose();
                particles.Dispose();
                particleSortInitArgs.Dispose();
                particleSort512.Dispose();
                particleBitonicSortStep.Dispose();
                particleSortInner512.Dispose();

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}