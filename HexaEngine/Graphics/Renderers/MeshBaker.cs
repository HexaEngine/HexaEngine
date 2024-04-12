namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Threading;
    using HexaEngine.Graphics.Filters;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;

    public unsafe class MeshBaker2
    {
        private const int RTWidth = 2048;
        public readonly Texture2D Texture;
        public readonly Texture2D TextureFinal;
        private readonly Camera[] cameras = new Camera[6];

        private IBLDiffuseIrradianceCompute irradiance;

        public MeshBaker2(IGraphicsDevice device)
        {
            Texture = new(Format.R16G16B16A16UNorm, RTWidth, RTWidth, 6, 1, CpuAccessFlags.None, GpuAccessFlags.All, miscFlag: ResourceMiscFlag.TextureCube);
            Texture.CreateArraySlices();

            TextureFinal = new(Format.R16G16B16A16UNorm, 256, 256, 6, 1, CpuAccessFlags.None, GpuAccessFlags.All, miscFlag: ResourceMiscFlag.TextureCube);
            TextureFinal.CreateArraySlices();

            irradiance = new(device);

            irradiance.Target = TextureFinal;
            irradiance.Source = Texture;
        }

        public void Bake(IThreadDispatcher dispatcher, IGraphicsContext context, Transform objectTransform, Model model, ISceneRenderer sceneRenderer)
        {
            var global = objectTransform.Global;

            for (int i = 0; i < model.Nodes.Length; i++)
            {
                var node = model.Nodes[i];
                var nodeGlobal = node.GetGlobalTransform(global);

                for (int j = 0; j < node.Meshes.Count; j++)
                {
                    var mesh = model.Meshes[j];

                    BakeMesh(dispatcher, context, nodeGlobal, sceneRenderer);
                }
            }
        }

        private void BakeMesh(IThreadDispatcher dispatcher, IGraphicsContext context, Matrix4x4 nodeGlobal, ISceneRenderer sceneRenderer)
        {
            dispatcher.InvokeBlocking(() =>
            {
                var oldSize = sceneRenderer.Size;
                sceneRenderer.Size = Texture.Viewport.Size;
                sceneRenderer.DrawFlags = SceneDrawFlags.NoPostProcessing | SceneDrawFlags.NoOverlay;
                context.ClearRenderTargetView(Texture.RTV, default);
                var position = nodeGlobal.Translation;
                SetViewPoint(position);

                for (int i = 0; i < 6; i++)
                {
                    CameraManager.Current = cameras[i];
                    context.BeginEvent($"Render face {i}");
                    sceneRenderer.RenderTo(context, Texture.RTVArraySlices[i], Texture.Viewport, SceneManager.Current, cameras[i]);
                    context.EndEvent();
                }
                CameraManager.Current = null;

                irradiance.Dispatch(context, 256, 256);

                sceneRenderer.DrawFlags = SceneDrawFlags.None;
                sceneRenderer.Size = oldSize;
            });
        }

        public void SetViewPoint(Vector3 position)
        {
            // The TextureCube Texture2D assumes the
            // following order of faces.

            // The LookAt targets for view matrices
            var targets = new[] {
                  Vector3.UnitX, // +X
                  -Vector3.UnitX, // -X
                  Vector3.UnitY, // +Y
                 - Vector3.UnitY, // -Y
                   Vector3.UnitZ, // +Z
                - Vector3.UnitZ  // -Z
            };

            var upVectors = new[] {
                Vector3.UnitY, // +X
                Vector3.UnitY, // -X
                -Vector3.UnitZ,// +Y
                Vector3.UnitZ,// -Y
                Vector3.UnitY, // +Z
                Vector3.UnitY, // -Z
            };

            for (int i = 0; i < 6; i++)
            {
                var cam = cameras[i] ?? new();
                cameras[i] = cam;
                cam.Fov = 90f.ToRad();
                cam.Width = RTWidth;
                cam.Height = RTWidth;
                cam.Far = 1000f;
                cam.Near = 0.001f;
                cam.Transform.Position = position;

                Matrix4x4.Decompose(MathUtil.LookAtLH(Vector3.Zero, targets[i], upVectors[i]), out _, out var rot, out _);
                cam.Transform.Orientation = rot;
                cam.Transform.Recalculate();
            }
        }
    }
}