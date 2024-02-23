namespace HexaEngine.Meshes
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Terrains;

    public class TerrainDrawLayer
    {
        private readonly Texture2D mask;
        private readonly TerrainMaterial material;
        private readonly TerrainLayerGroup group;

        public unsafe TerrainDrawLayer(TerrainLayerGroup group, bool isDynamic, bool isDefault = false)
        {
            this.group = group;
            IGraphicsDevice device = Application.GraphicsDevice;
            if (isDefault)
            {
                for (uint i = 0; i < 1024 * 1024; i++)
                {
                    group.Mask[i] = new(1, 0, 0, 0);
                }
            }

            mask = group.Mask.CreateLayerMask(device, isDynamic ? GpuAccessFlags.RW : GpuAccessFlags.Read); ;
            material = new(group);
        }

        public TerrainLayerGroup LayerGroup => group;

        public int LayerCount => group.Count;

        public ChannelMask GetChannelMask(TerrainLayer layer)
        {
            return GetChannelMask(group.IndexOf(layer));
        }

        public Texture2D Mask => mask;

        public TerrainMaterial Material => material;

        public static ChannelMask GetChannelMask(int layer)
        {
            return layer switch
            {
                0 => ChannelMask.R,
                1 => ChannelMask.G,
                2 => ChannelMask.B,
                3 => ChannelMask.A,
                _ => ChannelMask.None,
            };
        }

        public bool AddLayer(TerrainLayer layer)
        {
            return group.Add(layer);
        }

        public bool RemoveLayer(TerrainLayer layer)
        {
            return group.Remove(layer);
        }

        public bool ContainsLayer(TerrainLayer layer)
        {
            return group.Contains(layer);
        }

        public void UpdateLayerMaterials()
        {
            material.Update();
        }

        public void Dispose()
        {
            material.Dispose();
        }
    }
}