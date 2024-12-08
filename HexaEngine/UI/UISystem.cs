namespace HexaEngine.UI
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Scenes;
    using HexaEngine.UI.Animation;
    using HexaEngine.UI.Graphics;
    using HexaEngine.UI.Graphics.Text;
    using HexaEngine.Windows;
    using System.Numerics;

    public class UISystem : ISceneSystem
    {
        private TextFactory textFactory = null!;
        private UIWindow? window;
        private readonly UICommandList commandList = new();
        private bool invalidate = true;

        public string Name { get; } = "UI";

        public SystemFlags Flags { get; } = SystemFlags.Awake | SystemFlags.Unload | SystemFlags.GraphicsUpdate;

        public static UISystem? Current { get; set; }

        public TextFactory TextFactory => textFactory;

        public UIWindow? Window
        {
            get => window;
            set
            {
                if (window != null)
                {
                    window.Dispose();
                    window.OnInvalidateVisual -= OnInvalidateVisual;
                }

                window = value;
                if (value != null)
                {
                    value.Show();
                    var renderer = SceneRenderer.Current;
                    var size = renderer.Size;
                    value.Width = size.X;
                    value.Height = size.Y;
                    value.InvalidateMeasure();
                    value.SetInputTransform(ComputeInputTransform(renderer.OutputViewport));
                    value.OnInvalidateVisual += OnInvalidateVisual;
                }
                invalidate = true;
            }
        }

        public UICommandList CommandList => commandList;

        public void Awake(Scene scene)
        {
            textFactory = new TextFactory(Application.GraphicsDevice);
            Current = this;

            SceneRenderer.Resized += SceneRendererResized;
            SceneRenderer.OutputViewportChanged += OutputViewportChanged;
            Application.MainWindow.Moved += MainWindow_Moved;
            Application.MainWindow.Resized += MainWindow_Resized;
        }

        private void MainWindow_Resized(object? sender, Core.Windows.Events.ResizedEventArgs e)
        {
            if (Window == null) return;
            Window.SetInputTransform(ComputeInputTransform(SceneRenderer.Current.OutputViewport));
        }

        private void MainWindow_Moved(object? sender, Core.Windows.Events.MovedEventArgs e)
        {
            if (Window == null) return;
            Window.SetInputTransform(ComputeInputTransform(SceneRenderer.Current.OutputViewport));
        }

        private void OutputViewportChanged(object? sender, OutputViewportChangedEventArgs e)
        {
            if (Window == null) return;
            Window.SetInputTransform(ComputeInputTransform(SceneRenderer.Current.OutputViewport));
        }

        private Matrix3x2 ComputeInputTransform(Viewport offscreenViewport)
        {
            var mainWindow = Application.MainWindow;
            var mainWindowPos = new Vector2(mainWindow.X, mainWindow.Y);

            var windowSize = new Vector2(Window!.Width, Window.Height);

            Matrix3x2 translationMatrix0 = Matrix3x2.CreateTranslation(-(offscreenViewport.Offset + mainWindowPos));

            Matrix3x2 scaleMatrix0 = Matrix3x2.CreateScale(Vector2.One / offscreenViewport.Size);

            Matrix3x2 scaleMatrix1 = Matrix3x2.CreateScale(windowSize);

            Matrix3x2 inputTransform = translationMatrix0 * scaleMatrix0 * scaleMatrix1;
            return inputTransform;
        }

        private void SceneRendererResized(object? sender, RendererResizedEventArgs e)
        {
            var window = Window;
            if (window == null) return;
            window.Width = e.NewSize.X;
            window.Height = e.NewSize.Y;
            window.InvalidateMeasure();
        }

        private void OnInvalidateVisual(object? sender, EventArgs e)
        {
            invalidate = true;
        }

        public void Update(float delta)
        {
            var window = Window;
            if (window == null) return;

            AnimationScheduler.Tick();

            if (!invalidate) return;

            commandList.BeginDraw();

            window.Render(commandList);

            commandList.Transform = Matrix3x2.Identity;

            commandList.EndDraw();

            invalidate = false;
        }

        public void Unload()
        {
            if (window != null)
            {
                window.OnInvalidateVisual -= OnInvalidateVisual;
            }
            Application.MainWindow.Moved -= MainWindow_Moved;
            Application.MainWindow.Resized -= MainWindow_Resized;
            SceneRenderer.OutputViewportChanged -= OutputViewportChanged;
            SceneRenderer.Resized -= SceneRendererResized;
            Window?.Dispose();
            Current = null;
            textFactory?.Dispose();
            commandList.Dispose();
        }
    }
}