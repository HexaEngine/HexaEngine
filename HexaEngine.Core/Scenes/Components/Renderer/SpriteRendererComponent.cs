namespace HexaEngine.Scenes.Components.Renderer
{
    using HexaEngine.Core;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Renderers;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Mathematics;
    using HexaEngine.Rendering;
    using System;
    using System.Numerics;

    [EditorComponent<SpriteRendererComponent>("Sprite Renderer")]
    public class SpriteRendererComponent : IRendererComponent
    {
        private IGraphicsDevice device;
        private GameObject gameObject;
        private SpriteRenderer renderer;
        private SpriteBatch spriteBatch;
        private SpriteAtlas? spriteAtlas;
        private Sprite sprite = new();
        private string atlasPath = string.Empty;

        public uint QueueIndex { get; } = (uint)RenderQueueIndex.Geometry;

        public BoundingBox BoundingBox { get; }

        public RendererFlags Flags { get; } = RendererFlags.Draw;

        [EditorProperty("Atlas", null)]
        public string AtlasPath
        {
            get => atlasPath; set
            {
                atlasPath = value;
                if (device == null)
                {
                    return;
                }

                UpdateAtlasAsync(device);
            }
        }

        [EditorProperty("Offset")]
        public Vector2 Offset { get => sprite.ScreenPos; set => sprite.ScreenPos = value; }

        [EditorProperty("Size")]
        public Vector2 Size { get => sprite.Size; set => sprite.Size = value; }

        [EditorProperty("AtlasPos")]
        public Vector2 AtlasPos { get => sprite.AltasPos; set => sprite.AltasPos = value; }

        [EditorProperty("ZIndex")]
        public int ZIndex { get => sprite.ZIndex; set => sprite.ZIndex = value; }

        [EditorProperty("Layer")]
        public uint Layer { get => sprite.Layer; set => sprite.Layer = value; }

        [JsonIgnore]
        public SpriteBatch SpriteBatch => spriteBatch;

        [JsonIgnore]
        public SpriteAtlas? SpriteAtlas => spriteAtlas;

        public async void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            this.device = device;
            this.gameObject = gameObject;
            renderer = new(device);
            spriteBatch = new(device);
            spriteBatch.Add(sprite);

            await UpdateAtlasAsync(device);
        }

        public void Destory()
        {
            renderer.Dispose();
            spriteAtlas.Dispose();
            spriteBatch.Dispose();
        }

        public void Draw(IGraphicsContext context)
        {
            if (!gameObject.IsEnabled || spriteAtlas == null)
            {
                return;
            }
            renderer.Draw(context, spriteBatch, spriteAtlas, gameObject.Transform);
        }

        public void DrawDepth(IGraphicsContext context)
        {
        }

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            throw new NotImplementedException();
        }

        public void Update(IGraphicsContext context)
        {
        }

        public void VisibilityTest(IGraphicsContext context, Camera camera)
        {
            throw new NotImplementedException();
        }

        private Task UpdateAtlasAsync(IGraphicsDevice device)
        {
            var tmpAtlas = spriteAtlas;
            spriteAtlas = null;
            tmpAtlas?.Dispose();

            var state = new Tuple<IGraphicsDevice, SpriteRendererComponent>(device, this);
            return Task.Factory.StartNew(async (state) =>
            {
                var p = (Tuple<IGraphicsDevice, SpriteRendererComponent>)state;
                var device = p.Item1;
                var component = p.Item2;
                var path = Paths.CurrentAssetsPath + component.atlasPath;

                if (FileSystem.Exists(path))
                {
                    component.spriteAtlas = new(device, SamplerDescription.PointClamp, path);
                }
            }, state);
        }
    }
}