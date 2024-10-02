namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.UI;
    using HexaEngine.Materials.Nodes.Textures;
    using System.Numerics;

    public class TextureFileNodeRenderer : BaseNodeRendererInstanced<TextureFileNode>
    {
        public Ref<Texture2D>? image;
        public SamplerState? sampler;
        public bool showMore;
        public Vector2 size = new(128, 128);

        public override void OnSetInstance(TextureFileNode node)
        {
            node.ReloadRequested += NodeReloadRequested;
            Reload();
        }

        private void NodeReloadRequested(object? sender, TextureFileNodeReloadRequestEventArgs e)
        {
            Reload(e.SamplerOnly);
        }

        public void Reload(bool samplerOnly = false)
        {
            var path = ((AssetRef)Node.Path).GetPath();

            if (File.Exists(path))
            {
                sampler?.Dispose();

                SamplerStateDescription description = new()
                {
                    Filter = (Filter)Node.Filter,
                    AddressU = (TextureAddressMode)Node.U,
                    AddressV = (TextureAddressMode)Node.V,
                    AddressW = (TextureAddressMode)Node.W,
                    ComparisonFunction = ComparisonFunction.Never,
                    BorderColor = Node.BorderColor,
                    MaxAnisotropy = Node.MaxAnisotropy,
                    MaxLOD = Node.MaxLOD,
                    MinLOD = Node.MinLOD,
                    MipLODBias = Node.MipLODBias,
                };

                if (samplerOnly)
                {
                    sampler = new(description);
                    return;
                }
                var cache = SourceAssetsDatabase.ThumbnailCache;
                if (!cache.TryGet(Node.Path, out var texture))
                {
                    var scratchImage = Application.GraphicsDevice.TextureLoader.LoadFormFile(path);
                    cache.GenerateAndSetThumbnail(Node.Path, scratchImage);
                    scratchImage.Dispose();
                    cache.Get(Node.Path, out texture);
                }

                image = texture;

                sampler = new(description);
            }
        }

        protected override void DisposeCore()
        {
            Node.ReloadRequested -= NodeReloadRequested;
            image?.Dispose();
            sampler?.Dispose();
        }

        protected override void DrawContent(TextureFileNode node)
        {
            ImGui.Image(image?.Value?.SRV?.NativePointer ?? 0, size);

            ImGui.PushItemWidth(100);

            AssetRef assetRef = node.Path;

            if (ComboHelper.ComboForAssetRef("Texture", ref assetRef, AssetType.Texture2D))
            {
                node.Path = assetRef;
                Reload();
                node.OnValueChanged();
            }

            if (!showMore && ImGui.Button("more..."))
            {
                showMore = true;
            }

            if (showMore && ImGui.Button("less..."))
            {
                showMore = false;
            }

            if (showMore)
            {
                bool active = false;
                bool changed = false;
                changed |= ComboEnumHelper<TextureMapFilter>.Combo("Filter", ref node.Filter);
                active |= ImGui.IsItemActive();

                changed |= ComboEnumHelper<TextureMapMode>.Combo("AddressU", ref node.U);
                active |= ImGui.IsItemActive();

                changed |= ComboEnumHelper<TextureMapMode>.Combo("AddressV", ref node.V);
                active |= ImGui.IsItemActive();

                changed |= ComboEnumHelper<TextureMapMode>.Combo("AddressW", ref node.W);
                active |= ImGui.IsItemActive();

                changed |= ImGui.InputFloat("MipLODBias", ref node.MipLODBias);
                active |= ImGui.IsItemActive();

                changed |= ImGui.SliderInt("Anisotropy", ref node.MaxAnisotropy, 1, TextureFileNode.MaxMaxAnisotropy);
                active |= ImGui.IsItemActive();

                changed |= ImGui.ColorEdit4("BorderColor", ref node.BorderColor);
                active |= ImGui.IsItemActive();

                changed |= ImGui.InputFloat("MinLOD", ref node.MinLOD);
                active |= ImGui.IsItemActive();

                changed |= ImGui.InputFloat("MaxLOD", ref node.MaxLOD);
                active |= ImGui.IsItemActive();

                if (changed)
                {
                    node.changed = true;
                }

                if (!active && node.changed)
                {
                    node.changed = false;
                    Reload(true);
                }
            }

            ImGui.PopItemWidth();
        }
    }
}