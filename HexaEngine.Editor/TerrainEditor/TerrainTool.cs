namespace HexaEngine.Editor.TerrainEditor
{
    using HexaEngine.Mathematics;
    using System.Numerics;

    public abstract class TerrainTool
    {
        private float size = 2;
        private float strength = 10;
        private float blendStart = 0;
        private float blendEnd = 1;
        private float edgeFadeStart = 0;
        private float edgeFadeEnd = 2;

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

        public abstract bool Modify(TerrainToolContext context);

        public virtual void OnMouseDown(Vector3 position)
        {
        }

        public abstract void DrawSettings();

        public float ComputeEdgeFade(float distance)
        {
            return MathUtil.Clamp01((edgeFadeEnd - distance) / (edgeFadeEnd - edgeFadeStart));
        }
    }
}