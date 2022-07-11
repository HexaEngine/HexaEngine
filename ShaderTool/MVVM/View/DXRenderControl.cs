using HexaEngine.Rendering;
using HexaEngine.Windows.Native;
using ShaderTool.MVVM.Model;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace ShaderTool.MVVM.View
{
    public class DXRenderControl : HwndHost
    {
        private bool isRunning = true;
        private Thread renderThread;
        private bool first;
        private bool doResize;
        private bool isInitialized;

        public bool IsRendering { get; set; } = true;

        public float FrameTime { get; set; }

        private IntPtr ChildHandle => ChildWindow.Handle;

        public HexaEngine.Windows.Window ChildWindow { get; set; }

        public IObjectRenderer ObjectRenderer { get; set; }

        public bool VSync { get; set; }

        public bool FPSLimit { get; set; }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            HandleRef href;
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                ChildWindow = new();
                ChildWindow.RenderWidth = 1920;
                ChildWindow.RenderHeight = 1080;
                ChildWindow.Style = WindowStyles.WS_CHILD;
                ChildWindow.Show(hwndParent.Handle);
                href = new HandleRef(this, ChildHandle);
                return href;
            }

            if (ChildWindow is not null)
            {
                ChildWindow.RenderWidth = 1920;
                ChildWindow.RenderHeight = 1080;
                ChildWindow.Style = WindowStyles.WS_CHILD;
                ChildWindow.Show(hwndParent.Handle);
                href = new HandleRef(this, ChildHandle);
            }
            else
            {
                ChildWindow = new();
                ChildWindow.RenderWidth = 1920;
                ChildWindow.RenderHeight = 1080;
                ChildWindow.Style = WindowStyles.WS_CHILD;
                ChildWindow.Show(hwndParent.Handle);
                href = new HandleRef(this, ChildHandle);
            }

            renderThread = new(Render);
            renderThread.Start();
            return href;
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            isRunning = false;
            while (renderThread.IsAlive)
            {
                Thread.Sleep(1);
            }
            DeviceManager.Dispose();
            ChildWindow.Close();
            ChildWindow.Dispose();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (sizeInfo.NewSize.Width > 0 && sizeInfo.NewSize.Height > 0)
            {
                doResize = true;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(Brushes.Black, null, new System.Windows.Rect(0, 0, ActualWidth, ActualHeight));
        }

        protected virtual void Render()
        {
            while (isRunning)
            {
                while (isRunning && !IsRendering)
                {
                    Thread.Sleep(1);
                }

                if (doResize)
                {
                    if (isInitialized)
                    {
                        Dispatcher.Invoke(() => DeviceManager.Resize((int)ActualWidth, (int)ActualHeight));
                    }
                    else
                    {
                        DeviceManager.Initialize(ChildWindow, DeviceManagerUsage.RenderTargetOutput | DeviceManagerUsage.Shared);
                        isInitialized = true;
                    }

                    doResize = false;
                }

                if (!isInitialized)
                    continue;

                if (!first)
                {
                    first = true;
                }

                ObjectRenderer?.Render();
                EndRender();
                LimitFrameRate();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void EndRender()
        {
            DeviceManager.SwapChain.Present(VSync ? 1 : 0);
        }

        public int FPSTarget = 60;
        private long fpsFrameCount;
        private long fpsStartTime;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LimitFrameRate()
        {
            if (FPSLimit & !VSync)
            {
                int fps = FPSTarget;
                long freq = Stopwatch.Frequency;
                long frame = Stopwatch.GetTimestamp();
                while ((frame - fpsStartTime) * fps < freq * fpsFrameCount)
                {
                    int sleepTime = (int)((fpsStartTime * fps + freq * fpsFrameCount - frame * fps) * 1000 / (freq * fps));
                    if (sleepTime > 0) Thread.Sleep(sleepTime);
                    frame = Stopwatch.GetTimestamp();
                }
                if (++fpsFrameCount > fps)
                {
                    fpsFrameCount = 0;
                    fpsStartTime = frame;
                }
            }
        }
    }
}