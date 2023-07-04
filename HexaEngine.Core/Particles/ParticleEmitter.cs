namespace HexaEngine.Rendering
{
    using HexaEngine.Core.Graphics;
    using System.Numerics;

    public class ParticleEmitter
    {
        public ParticleEmitter()
        {
        }

        public Texture2D ParticleTexture;
        public Vector4 Position = new(0, 0, 0, 0);
        public Vector4 Velocity = new(0, 5, 0, 0);
        public Vector4 PositionVariance = new(0, 0, 0, 0);
        public int NumberToEmit = 0;
        public float ParticleLifespan = 5.0f;
        public float StartSize = 0.1f;
        public float EndSize = 0.005f;
        public float Mass = 1.0f;
        public float VelocityVariance = 1.0f;
        public float ParticlesPerSecond = 100;
        public float Accumulation = 0.0f;
        public float ElapsedTime = 0.0f;
        public bool CollisionsEnabled = false;
        public int CollisionThickness = 40;
        public bool AlphaBlended = true;
        public bool Pause = false;
        public bool Sort = false;
        public bool ResetEmitter = true;
    }
}