namespace HexaEngine.Components.Renderer
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using System;
    using System.Numerics;

    [EditorComponent<SpriteRendererComponent>("Sprite Renderer")]
    public class SpriteRendererComponent : BaseRendererComponent
    {
        private SpriteRenderer renderer;
        private SpriteBatch spriteBatch;
        private SpriteAtlas? spriteAtlas;
        private Sprite sprite = new();
        private string atlasPath = string.Empty;

        [JsonIgnore]
        public override string DebugName { get; protected set; } = nameof(SpriteRenderer);

        [JsonIgnore]
        public override uint QueueIndex { get; } = (uint)RenderQueueIndex.Geometry;

        [JsonIgnore]
        public override BoundingBox BoundingBox { get; }

        [JsonIgnore]
        public override RendererFlags Flags { get; } = RendererFlags.Draw;

        [EditorProperty("Atlas", null)]
        public string AtlasPath
        {
            get => atlasPath; set
            {
                atlasPath = value;

                UpdateAtlasAsync();
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

        public override void Load(IGraphicsDevice device)
        {
            DebugName = GameObject.Name + DebugName;
            renderer = new(device);
            spriteBatch = new(device);
            spriteBatch.Add(sprite);

            UpdateAtlasAsync().Wait();
        }

        public override void Unload()
        {
            renderer.Dispose();
            spriteAtlas.Dispose();
            spriteBatch.Dispose();
        }

        public override void Draw(IGraphicsContext context, RenderPath path)
        {
            if (!GameObject.IsEnabled || spriteAtlas == null)
            {
                return;
            }
            renderer.Draw(context, spriteBatch, spriteAtlas, GameObject.Transform);
        }

        public override void Bake(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public override void DrawDepth(IGraphicsContext context)
        {
        }

        public override void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            throw new NotImplementedException();
        }

        public override void Update(IGraphicsContext context)
        {
        }

        public override void VisibilityTest(CullingContext context)
        {
            throw new NotImplementedException();
        }

        private Task UpdateAtlasAsync()
        {
            Loaded = false;
            var tmpAtlas = spriteAtlas;
            spriteAtlas = null;
            tmpAtlas?.Dispose();

            var state = new Tuple<IGraphicsDevice, SpriteRendererComponent>(Application.GraphicsDevice, this);
            return Task.Factory.StartNew(async (state) =>
            {
                var p = (Tuple<IGraphicsDevice, SpriteRendererComponent>)state;
                var device = p.Item1;
                var component = p.Item2;
                var path = Paths.CurrentAssetsPath + component.atlasPath;

                if (FileSystem.Exists(path))
                {
                    component.spriteAtlas = new(device, SamplerStateDescription.PointClamp, path);
                    component.Loaded = true;
                }
            }, state);
        }
    }
}