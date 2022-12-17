namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public enum RenderFlags
    {
        None = 0,
        Depth = 1,
        Skinned = 2,
        Instanced = 4,
    }

    public enum MeshType
    {
        None = 0,

        /// <summary>
        /// <seealso cref="Meshes.MeshVertex"/>
        /// </summary>
        Mesh,

        /// <summary>
        /// <seealso cref="Meshes.SkinnedMeshVertex"/>
        /// </summary>
        Skinned,

        /// <summary>
        /// <seealso cref="Meshes.TerrainVertex"/>
        /// </summary>
        Terrain,

        /// <summary>
        /// <seealso cref="Meshes.TerrainVertexStatic"/>
        /// </summary>
        TerrainStatic,

        //TODO: Add Vertex Type
        /// <summary>
        /// Not yet implemented
        /// </summary>
        Particle,
    }

    public unsafe struct Mesh2
    {
        public UnsafeString* Name;
        public void* VertexBuffer;
        public void* IndexBuffer;
        public int VertexCount;
        public int IndexCount;
        public MeshType Type;
    }

    public struct MeshInstance
    {
        public int Id;
        public Matrix4x4 Transform;
        public BoundingBox BoundingBox;
        public BoundingSphere BoundingSphere;
        public RenderFlags Flags;
    }

    public static class GraphicsManager
    {
        private static IGraphicsDevice graphicsDevice;
        private static RenderQueue queue;
        private static IResourceManager resources;
        private static ILightManager lights;
        private static IPostProcessManager postProcess;

        public static void Initialize(IGraphicsDevice device)
        {
            graphicsDevice = device;
            queue = new();
        }

        public static RenderQueue Queue => queue;

        public static IResourceManager Resources => resources;

        public static ILightManager Lights => lights;

        public static IPostProcessManager PostProcess => postProcess;
    }

    public interface IInstanceManger
    {
    }
}