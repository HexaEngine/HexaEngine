namespace HexaEngine.Editor.TerrainEditor
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using System.Numerics;

    public abstract class TerrainTool : IDisposable
    {
        protected float size = 2;
        protected float strength = 10;
        protected float blendStart = 0;
        protected float blendEnd = 1;
        protected float edgeFadeStart = 0;
        protected float edgeFadeEnd = 2;
        private bool disposedValue;
        private bool isInitialized;
        private EdgeFadeMode edgeFadeMode;

        public float Strength
        {
            get => strength;
            set => strength = value;
        }

        public float Size
        {
            get => size;
            set
            {
                size = value;
                edgeFadeStart = blendStart * value;
                edgeFadeEnd = blendEnd * value;
            }
        }

        public float BlendStart
        {
            get => blendStart; set
            {
                blendStart = value;
                edgeFadeStart = value * size;
            }
        }

        public float BlendEnd
        {
            get => blendEnd; set
            {
                blendEnd = value;
                edgeFadeEnd = value * size;
            }
        }

        public abstract string Name { get; }

        public EdgeFadeMode EdgeFadeMode { get => edgeFadeMode; set => edgeFadeMode = value; }

        public virtual void Initialize(IGraphicsDevice device)
        {
            if (isInitialized)
            {
                return;
            }

            if (disposedValue)
            {
                disposedValue = false;
                GC.ReRegisterForFinalize(this);
            }

            InitializeTool(device);

            isInitialized = true;
        }

        protected virtual void InitializeTool(IGraphicsDevice device)
        {
        }

        public abstract bool Modify(IGraphicsContext context, TerrainToolContext toolContext);

        public virtual void OnMouseDown(Vector3 position)
        {
        }

        public virtual void OnMouseMove(Vector3 position)
        {
        }

        public abstract bool DrawSettings(TerrainToolContext toolContext);

        public float ComputeEdgeFade(float distance)
        {
            switch (edgeFadeMode)
            {
                case EdgeFadeMode.None:
                    return 1;

                case EdgeFadeMode.Linear:
                    return MathUtil.Clamp01((edgeFadeEnd - distance) / (edgeFadeEnd - edgeFadeStart));

                case EdgeFadeMode.Square:
                    return MathUtil.Clamp01(1 - (distance * distance) / (edgeFadeEnd * edgeFadeEnd));

                case EdgeFadeMode.Exponential:
                    {
                        float t = MathUtil.Clamp01((distance - edgeFadeStart) / (edgeFadeEnd - edgeFadeStart));
                        return MathUtil.Clamp01(1 - t);
                    }
                case EdgeFadeMode.Cos:
                    {
                        float t = MathUtil.Clamp01((distance - edgeFadeStart) / (edgeFadeEnd - edgeFadeStart));
                        return MathUtil.Clamp01((float)Math.Cos(Math.PI / 2 * t));
                    }

                case EdgeFadeMode.SmoothStep:
                    {
                        float t = MathUtil.Clamp01((edgeFadeEnd - distance) / (edgeFadeEnd - edgeFadeStart));
                        return t * t * (3 - 2 * t);
                    }

                default:
                    return 0;
            }
        }

        protected virtual void DisposeCore()
        {
        }

        private void DisposeInternal()
        {
            if (!disposedValue)
            {
                DisposeCore();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            DisposeInternal();
            GC.SuppressFinalize(this);
        }
    }
}