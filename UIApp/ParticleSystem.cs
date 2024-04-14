namespace UIApp
{
    using HexaEngine;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using HexaEngine.UI.Graphics;
    using System.Numerics;

    public struct Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Mass;
        public float LifeTime;
        public uint Type;
    }

    public class Emitter
    {
        public Vector2 Position = new(0, 0);
        public Vector2 Velocity = new(0, 10);
        public Vector2 PositionVariance = new(10, 10);
        public int NumberToEmit = 0;
        public float ParticleLifespan = float.MaxValue;
        public float Mass = 10;
        public int ParticlesPerSecond = 10000;
        public float VelocityVariance = 1000.0f;
        public float Accumulation = 0.0f;
        public float ElapsedTime = 0.0f;
        public RectangleF Boundaries;
    }

    public class ParticleSystem
    {
        public Particle[] Particles;
        public List<int> DeadList = new();
        public List<int> AliveIndexBuffer = new();

        public int MaxParticles = 20000;

        public ParticleSystem(int maxParticles = 20000)
        {
            MaxParticles = maxParticles;
            Particles = new Particle[MaxParticles];
            AliveIndexBuffer.Capacity = MaxParticles;
            DeadList.Capacity = MaxParticles;
            InitializeDeadList();
        }

        private void InitializeDeadList()
        {
            // init dead list
            for (int i = 0; i < MaxParticles; i++)
            {
                DeadList.Add(i);
            }
        }

        public void Emit(Emitter emitter)
        {
            int particlesToEmit = emitter.NumberToEmit;

            if (particlesToEmit == 0)
            {
                return;
            }

            for (int i = 0; i < particlesToEmit; i++)
            {
                if (DeadList.Count == 0)
                {
                    break;
                }

                float randomX = Random.Shared.NextSingle().Map01ToN1P1();
                float randomY = Random.Shared.NextSingle().Map01ToN1P1();

                var index = DeadList[^1];
                DeadList.RemoveAt(DeadList.Count - 1);

                Vector2 position = emitter.Position + emitter.PositionVariance * new Vector2(randomX, randomY);
                Vector2 velocity = emitter.Velocity + emitter.VelocityVariance * new Vector2(randomX, randomY);
                float mass = emitter.Mass;
                Particle particle = default;
                particle.Mass = mass;
                particle.Velocity = velocity;
                particle.Position = position;
                particle.LifeTime = emitter.ElapsedTime + emitter.ParticleLifespan;
                particle.Type = 0;
                Particles[index] = particle;

                AliveIndexBuffer.Add(index);
            }
        }

        private readonly object _lock = new();

        public void Simulate(Emitter emitter, float deltaTime)
        {
            Vector2 gravity = default;
            Parallel.For(0, AliveIndexBuffer.Count, i =>
            {
                int index = AliveIndexBuffer[i];
                Particle particle = Particles[index];

                if (particle.LifeTime < emitter.ElapsedTime)
                {
                    lock (_lock)
                    {
                        DeadList.Add(index);

                        AliveIndexBuffer.RemoveAt(i);
                    }
                }

                particle.Velocity += particle.Mass * gravity * deltaTime;
                particle.Position += particle.Velocity * deltaTime;

                if (particle.Position.Y <= emitter.Boundaries.Top)
                {
                    particle.Position.Y = 0;
                    particle.Velocity.Y *= -1f;
                }

                if (particle.Position.X <= emitter.Boundaries.Left)
                {
                    particle.Position.X = 0;
                    particle.Velocity.X *= -1f;
                }

                if (particle.Position.X >= emitter.Boundaries.Right)
                {
                    particle.Position.X = emitter.Boundaries.Right;
                    particle.Velocity.X *= -1f;
                }

                if (particle.Position.Y >= emitter.Boundaries.Bottom)
                {
                    particle.Position.Y = emitter.Boundaries.Bottom;
                    particle.Velocity.Y *= -1f;
                }

                Particles[index] = particle;
            });
        }

        public void Draw(UICommandList commandList, Brush brush)
        {
            for (int i = 0; i < AliveIndexBuffer.Count; i++)
            {
                int index = AliveIndexBuffer[i];
                Particle particle = Particles[index];
                commandList.FillEllipse(particle.Position, new Vector2(2), brush);
            }
        }

        public void Update(Emitter emitter)
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
    }
}