namespace HexaEngine.Editor.TerrainEditor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
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

        public abstract bool DrawSettings(TerrainToolContext toolContext);

        public float ComputeEdgeFade(float distance)
        {
            return MathUtil.Clamp01((edgeFadeEnd - distance) / (edgeFadeEnd - edgeFadeStart));
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

        ~TerrainTool()
        {
            DisposeInternal();
        }

        public void Dispose()
        {
            DisposeInternal();
            GC.SuppressFinalize(this);
        }
    }
}