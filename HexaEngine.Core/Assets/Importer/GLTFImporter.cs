using Hexa.NET.Logging;
using HexaEngine.Core.IO.Binary.Meshes;
using HexaEngine.Core.UI;
using SharpGLTF.Schema2;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using GLTFNode = SharpGLTF.Schema2.Node;
using Node = HexaEngine.Core.IO.Binary.Meshes.Node;

namespace HexaEngine.Core.Assets.Importer
{
    public class GLTFImporter : IAssetImporter
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(GLTFImporter));

        public Type? SettingsType { get; } = typeof(ModelImporterSettings);

        public string SettingsKey { get; } = "ModelImportSettings";

        public string? SettingsDisplayName { get; } = "Model Import Settings";

        public bool CanImport(ReadOnlySpan<char> fileExtension)
        {
            return fileExtension switch
            {
                ".glb" => true,
                ".gltf" => true,
                _ => false
            };
        }

        public void Import(TargetPlatform targetPlatform, ImportContext context)
        {
            ModelImporterSettings settings = context.GetOrCreateAdditionalMetadata<ModelImporterSettings>(SettingsKey);

            if (!Load(context, settings, out ModelRoot? scene))
            {
                Logger.Error($"Failed to import {context.SourcePath}");
                return;
            }
        }

        private static bool Load(ImportContext context, ModelImporterSettings settings, [NotNullWhen(true)] out ModelRoot? scene)
        {
            var modelName = Path.GetFileNameWithoutExtension(context.SourcePath);
            var importDir = Path.GetDirectoryName(context.SourcePath);
            var sourceDir = context.ImportSourcePath != null ? Path.GetDirectoryName(context.ImportSourcePath) : Path.GetDirectoryName(context.SourcePath);

            if (importDir == null)
            {
                throw new InvalidOperationException($"Cannot continue '{nameof(importDir)}' is null.");
            }

            if (sourceDir == null)
            {
                throw new InvalidOperationException($"Cannot continue '{nameof(sourceDir)}' is null.");
            }

            if (Path.GetExtension(context.SourcePath) == ".gltf")
            {
                string path = Directory.EnumerateFiles(sourceDir, $"{modelName}*.bin").First();

                if (File.Exists(path))
                {
                    SourceAssetsDatabase.ImportFile(path, new DefaultGuidProvider(context.AssetMetadata.Guid));
                }
            }

            ModelRoot root;
            try
            {
                root = ModelRoot.Load(context.ImportSourcePath, new ReadSettings() { Validation = SharpGLTF.Validation.ValidationMode.TryFix });
            }
            catch (Exception ex)
            {
                scene = null;
                Logger.Log(ex);
                return false;
            }

            scene = root;

            return true;
        }

        /*
        private unsafe bool LoadSceneGraph(AssimpScene* scene, [MaybeNullWhen(false)] out Node root, [MaybeNullWhen(false)] out Dictionary<string, Node> nameToNode, [MaybeNullWhen(false)] out Dictionary<GLTFNode, Node> pToNode)
        {
            try
            {
                List<Node> nodes = [];
                pToNode = [];
                nameToNode = [];
                root = WalkNode(nameToNode, pToNode, scene->MRootNode, null);
            }
            catch (Exception ex)
            {
                pToNode = null;
                nameToNode = null;
                root = null;
                Logger.Log(ex);
                MessageBox.Show("Failed to load scene graph", ex.Message);
                return false;
            }
            return true;
        }

        private unsafe Node WalkNode(Dictionary<string, Node> nameToNode, Dictionary<GLTFNode, Node> pToNode, GLTFNode node, Node? parent)
        {
            string name = node.Name;

            Matrix4x4 transform = node.LocalTransform.Matrix;

            Node sceneNode = new(name, transform, node. == 0 ? NodeFlags.None : NodeFlags.Drawable, parent, new([]));

            for (int i = 0; i < node->MNumMeshes; i++)
            {
                sceneNode.Meshes.Add(node->MMeshes[i]);
            }

            for (int i = 0; i < node->MNumChildren; i++)
            {
                var child = WalkNode(nameToNode, pToNode, node->MChildren[i], sceneNode);
                sceneNode.Children.Add(child);
            }

            string orgName = name;
            int x = 1;
            while (nameToNode.ContainsKey(name))
            {
                name = $"{orgName}.{x}";
                x++;
            }

            sceneNode.Name = name;

            nameToNode.Add(name, sceneNode);
            pToNode.Add(node, sceneNode);

            return sceneNode;
        }*/
    }
}