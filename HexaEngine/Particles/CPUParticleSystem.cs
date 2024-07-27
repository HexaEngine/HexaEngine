namespace HexaEngine.Particles
{
    using Hexa.NET.Mathematics;
    using Hexa.NET.Utilities;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Structs;
    using HexaEngine.Scenes.Managers;
    using HexaEngine.Weather;
    using System.Numerics;

    public class CPUParticleSystem
    {
        private UnsafeList<uint> deadList = new(MaxParticles);
        private UnsafeList<CPUParticleA> particlesA = new(MaxParticles);
        private UnsafeList<CPUParticleB> particlesB = new(MaxParticles);
        private UnsafeList<ViewSpacePositionRadius> viewSpacePositions = new(MaxParticles);
        private UnsafeList<IndexBufferElement> aliveIndexBuffer = new(MaxParticles);
        private Random random = new();

        private object _lock = new();

        private ParticleEmitter emitter;

        private DrawIndexedInstancedIndirectArgs drawArgs;

        private struct CPUParticleA
        {
            public Vector4 TintAndAlpha;
            public float Rotation;
            public uint IsSleeping;
        };

        private struct CPUParticleB
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

        private struct IndexBufferElement
        {
            public float Distance;
            public float Index;

            public IndexBufferElement(float distance, float index)
            {
                Distance = distance;
                Index = index;
            }
        };

        private struct ViewSpacePositionRadius
        {
            public Vector3 ViewspacePosition;
            public float Radius;

            public ViewSpacePositionRadius(Vector3 viewspacePosition, float radius)
            {
                ViewspacePosition = viewspacePosition;
                Radius = radius;
            }
        };

        public CPUParticleSystem(ParticleEmitter emitter)
        {
            this.emitter = emitter;
        }

        private const int MaxParticles = 400 * 1024;

        public void InitializeDeadList()
        {
            Parallel.For(0, MaxParticles, InitializeDeadList);
        }

        private void InitializeDeadList(int index)
        {
            deadList[index] = (uint)index;
        }

        public void ResetParticles()
        {
            Parallel.For(0, MaxParticles, ResetParticles);
        }

        private void ResetParticles(int id)
        {
            particlesA[id] = default;
            particlesB[id] = default;
        }

        public void Emit()
        {
            Parallel.For(0, emitter.NumberToEmit, Emit);
        }

        private unsafe void Emit(int id)
        {
            if (id < deadList.Count && id < emitter.NumberToEmit)
            {
                CPUParticleA pa = default;
                CPUParticleB pb = default;

                Vector3 randomValues0 = new(random.NextSingle(), random.NextSingle(), random.NextSingle());
                Vector3 randomValues1 = new(random.NextSingle(), random.NextSingle(), random.NextSingle());

                pa.TintAndAlpha = new Vector4(1, 1, 1, 1);
                pa.Rotation = 0;
                pa.IsSleeping = 0;

                float velocityMagnitude = emitter.Velocity.Length();
                pb.Position = emitter.Position.ToVec3() + randomValues0 * emitter.PositionVariance.ToVec3();
                pb.Mass = emitter.Mass;
                pb.Velocity = emitter.Velocity.ToVec3() + randomValues1 * velocityMagnitude * emitter.VelocityVariance;
                pb.Lifespan = emitter.ParticleLifespan;
                pb.Age = pb.Lifespan;
                pb.StartSize = emitter.StartSize;
                pb.EndSize = emitter.EndSize;

                uint index;
                lock (_lock)
                {
                    index = *deadList.Back;
                    deadList.PopBack();
                }

                particlesA[index] = pa;
                particlesB[index] = pb;
            }
        }

        public void Simulate()
        {
        }

        private void Simulate(int id)
        {
            var camera = CameraManager.Current;
            var wind_direction = WeatherSystem.Current?.WindDirection ?? Vector2.Zero;
            float delta_time = Time.Delta;
            if (id == 0)
            {
                drawArgs = default;
            }

            Vector3 Gravity = new Vector3(0.0f, -9.81f, 0.0f);

            CPUParticleA pa = particlesA[id];
            CPUParticleB pb = particlesB[id];

            if (pb.Age > 0.0f)
            {
                pb.Age -= delta_time;

                pa.Rotation += 0.24f * delta_time;

                Vector3 NewPosition = pb.Position;

                if (pa.IsSleeping == 0)
                {
                    pb.Velocity += pb.Mass * Gravity * delta_time;

                    Vector3 windDir = new(wind_direction.X, wind_direction.Y, 0);
                    float windLength = windDir.Length();
                    const float windStrength = 0.1f;
                    if (windLength > 0.0f)
                    {
                        pb.Velocity += windDir / windLength * windStrength * delta_time;
                    }

                    NewPosition += pb.Velocity * delta_time;
                }

                float fScaledLife = 1.0f - MathUtil.Clamp01(pb.Age / pb.Lifespan);

                float radius = MathUtil.Lerp(pb.StartSize, pb.EndSize, fScaledLife);

                bool killParticle = false;

                if (emitter.CollisionsEnabled)
                {
                    /*
                     * TODO: Add physics ray-casting here.
                    Vector3 viewSpaceParticlePosition = mul(float4(NewPosition, 1), view).xyz;
                    Vector4 screenSpaceParticlePosition = mul(float4(NewPosition, 1), viewProj);
                    float w = screenSpaceParticlePosition.W;
                    screenSpaceParticlePosition /= screenSpaceParticlePosition.W;
                    screenSpaceParticlePosition.W = w;

                    if (pa.IsSleeping == 0 && screenSpaceParticlePosition.X > -1 && screenSpaceParticlePosition.X < 1 && screenSpaceParticlePosition.Y > -1 && screenSpaceParticlePosition.Y < 1)
                    {
                        Vector3 viewSpacePosOfDepthBuffer = CalcViewSpacePositionFromDepth(screenSpaceParticlePosition.xy, int2(0, 0));

                        if ((viewSpaceParticlePosition.Z > viewSpacePosOfDepthBuffer.Z) && (viewSpaceParticlePosition.Z < viewSpacePosOfDepthBuffer.Z + emitter.CollisionThickness))
                        {
                            Vector3 surfaceNormal;

                            Vector3 p0 = viewSpacePosOfDepthBuffer;
                            Vector3 p1 = CalcViewSpacePositionFromDepth(screenSpaceParticlePosition.xy, int2(1, 0));
                            Vector3 p2 = CalcViewSpacePositionFromDepth(screenSpaceParticlePosition.xy, int2(0, 1));

                            Vector3 viewSpaceNormal = Vector3.Normalize(cross(p2 - p0, p1 - p0));

                            surfaceNormal = Vector3.Normalize(mul(-viewSpaceNormal, (float3x4)viewInv).xyz);

                            Vector3 newVelocity = Vector3.Reflect(pb.Velocity, surfaceNormal);

                            pb.Velocity = 0.3f * newVelocity;

                            NewPosition = pb.Position + (pb.Velocity * delta_time);
                        }
                    }
                    */
                }

                if (pb.Velocity.Length() < 0.01)
                {
                    pa.IsSleeping = 1;
                }

                if (NewPosition.Y < -10)
                {
                    killParticle = true;
                }

                pb.Position = NewPosition;

                Vector3 vec = NewPosition - camera.Transform.Position;
                pb.DistanceToEye = vec.Length();

                float alpha = MathUtil.Lerp(1, 0, MathUtil.Clamp01(fScaledLife - 0.8f) / 0.2f);
                pa.TintAndAlpha = Vector4.One;
                pa.TintAndAlpha.W = pb.Age <= 0 ? 0 : alpha;

                ViewSpacePositionRadius viewSpacePositionAndRadius = new(Vector3.Transform(NewPosition, camera.Transform.View), radius);

                viewSpacePositions[id] = viewSpacePositionAndRadius;
                if (pb.Age <= 0.0f || killParticle)
                {
                    pb.Age = -1;
                    lock (_lock)
                    {
                        deadList.PushBack((uint)id);
                    }
                }
                else
                {
                    uint index = aliveIndexBuffer.InterlockedIncrementCounter();
                    aliveIndexBuffer[index] = new(pb.DistanceToEye, id);
                    uint dstIdx = 0;

                    Interlocked.Add(ref drawArgs.IndexCountPerInstance, 6);
                    Interlocked.Add(ref drawArgs.StartIndexLocation, dstIdx);
                }
            }

            particlesA[id] = pa;
            particlesB[id] = pb;
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

        public void Draw(IGraphicsContext context, IShaderResourceView particleSRV)
        {
            if (emitter.ResetEmitter)
            {
                InitializeDeadList();
                ResetParticles();
                emitter.ResetEmitter = false;
            }

            Emit();
            Simulate();

            if (emitter.Sort)
            {
                Sort();
            }

            Rasterize(context, emitter, particleSRV);
        }

        public void Rasterize(IGraphicsContext context, ParticleEmitter emitter, IShaderResourceView particleSRV)
        {
            // TODO: add drawing method.
            throw new NotImplementedException();
        }

        public void Sort()
        {
            // TODO: add sorting we could do sorting on the gpu.
            throw new NotImplementedException();
        }
    }
}