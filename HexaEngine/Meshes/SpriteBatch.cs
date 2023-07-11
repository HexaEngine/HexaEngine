namespace HexaEngine.Meshes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using System.Collections.Generic;

    public class SpriteBatch : IDisposable
    {
        private readonly StructuredBuffer<SpriteData> spriteBuffer;

        private readonly List<Sprite> sprites = new();

        private readonly SpriteZIndexComparer comparer = new();

        public SpriteBatch(IGraphicsDevice device)
        {
            spriteBuffer = new(device, CpuAccessFlags.Write);
        }

        public StructuredBuffer<SpriteData> Buffer => spriteBuffer;

        public uint Count => (uint)sprites.Count;

        public Sprite this[int index]
        {
            get => sprites[index];
            set => sprites[index] = value;
        }

        public void Add(Sprite sprite)
        {
            sprites.Add(sprite);
        }

        public void Remove(Sprite sprite)
        {
            sprites.Remove(sprite);
        }

        public void Clear()
        {
            sprites.Clear();
        }

        public bool Contains(Sprite sprite)
        {
            return sprites.Contains(sprite);
        }

        public void Update(IGraphicsContext context)
        {
            sprites.Sort(comparer);

            spriteBuffer.ResetCounter();

            for (int i = 0; i < sprites.Count; i++)
            {
                spriteBuffer.Add(new(sprites[i]));
            }

            spriteBuffer.Update(context);
        }

        public void Dispose()
        {
            spriteBuffer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}